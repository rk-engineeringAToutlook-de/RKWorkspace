import AppKit
import Darwin
import PDFKit
import SwiftUI

@main
@MainActor
final class MacPdfFrameGuestApp: NSObject, NSApplicationDelegate {
    private static var retainedDelegate: MacPdfFrameGuestApp?
    private let glassOverlay = GuestGlassOverlayController()
    private var model: FrameGuestModel?
    private var frameWindows: [String: GuestFrameWindowController] = [:]

    static func main() {
        setbuf(stdout, nil)
        setbuf(stderr, nil)
        RkwpTrace.log("main start args=\(CommandLine.arguments.dropFirst().joined(separator: " "))")
        SmokeTestRunner.runAndExitIfRequested(arguments: CommandLine.arguments)

        let app = NSApplication.shared
        let delegate = MacPdfFrameGuestApp()
        retainedDelegate = delegate
        app.delegate = delegate
        app.setActivationPolicy(.regular)
        app.run()
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        let model = FrameGuestModel()
        self.model = model
        model.onPortalPulse = { [weak self] edge in
            self?.glassOverlay.pulse(edge: edge)
        }
        model.onPortalError = { [weak self] edge in
            self?.glassOverlay.errorPulse(edge: edge)
        }
        model.onPortalChanged = { [weak self] isVisible, edge in
            isVisible ? self?.glassOverlay.show(edge: edge) : self?.glassOverlay.hide()
        }
        model.onFrameReady = { [weak self] lease in
            self?.openFrameWindow(lease)
        }
        model.onAutoReturnFrame = { [weak self] lease in
            self?.closeFrameWindow(lease)
        }

        model.applyCommandLine()
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        false
    }

    private func openFrameWindow(_ lease: GuestFrameLease) {
        let key = lease.windowKey
        guard frameWindows[key] == nil else {
            RkwpTrace.log("window skip existing key=\(key)")
            return
        }

        RkwpTrace.log("window open key=\(key) display=\(lease.displayName) size=\(Int(lease.documentSize.width))x\(Int(lease.documentSize.height))")
        let controller = GuestFrameWindowController(
            lease: lease,
            cascadeIndex: frameWindows.count,
            onClose: { [weak self] closedLease in
                self?.model?.returnFrame(closedLease)
                self?.frameWindows[closedLease.windowKey] = nil
            })
        frameWindows[key] = controller
        controller.show()
    }

    private func closeFrameWindow(_ lease: GuestFrameLease) {
        if let controller = frameWindows[lease.windowKey] {
            controller.closeFromAutoReturn()
        } else {
            model?.returnFrame(lease)
        }
    }
}

struct GuestFrameLease {
    let windowKey: String
    let frameSessionId: String
    let leaseId: String
    let sessionId: String
    let displayName: String
    let status: String
    let policyText: String
    let document: PDFDocument?
    let image: NSImage?
    let documentSize: CGSize
    let pdfData: Data?
    let usesLocalVerificationFrame: Bool
}

@MainActor
final class GuestFrameWindowController: NSObject, NSWindowDelegate {
    private var lease: GuestFrameLease?
    private var window: NSWindow?
    private let onClose: (GuestFrameLease) -> Void

    init(lease: GuestFrameLease, cascadeIndex: Int, onClose: @escaping (GuestFrameLease) -> Void) {
        self.lease = lease
        self.onClose = onClose
        super.init()
        createWindow(for: lease, cascadeIndex: cascadeIndex)
    }

    func show() {
        window?.makeKeyAndOrderFront(nil)
        NSApplication.shared.activate(ignoringOtherApps: true)
    }

    func closeFromAutoReturn() {
        window?.performClose(nil)
    }

    func windowWillClose(_ notification: Notification) {
        guard let lease else {
            return
        }

        RkwpTrace.log("window close key=\(lease.windowKey)")
        self.lease = nil
        window?.contentView = nil
        onClose(lease)
    }

    private func createWindow(for lease: GuestFrameLease, cascadeIndex: Int) {
        let contentSize = Self.contentSize(for: lease.documentSize)
        let screenFrame = NSScreen.main?.visibleFrame ?? NSRect(x: 0, y: 0, width: 1280, height: 800)
        let offset = CGFloat(min(cascadeIndex, 6) * 28)
        var frame = NSWindow.frameRect(
            forContentRect: NSRect(origin: .zero, size: contentSize),
            styleMask: [.titled, .closable, .miniaturizable, .resizable])
        frame.origin.x = screenFrame.midX - frame.width / 2 + offset
        frame.origin.y = screenFrame.midY - frame.height / 2 - offset

        let window = NSWindow(
            contentRect: frame,
            styleMask: [.titled, .closable, .miniaturizable, .resizable],
            backing: .buffered,
            defer: false)
        window.title = lease.displayName.isEmpty ? "RK Workspace PDF Frame" : lease.displayName
        window.contentView = NSHostingView(rootView: GuestFrameWindowView(lease: lease))
        window.isReleasedWhenClosed = false
        window.delegate = self
        self.window = window
    }

    private static func contentSize(for documentSize: CGSize) -> NSSize {
        let screenFrame = NSScreen.main?.visibleFrame ?? NSRect(x: 0, y: 0, width: 1280, height: 800)
        let statusHeight: CGFloat = 28
        let maxWidth = screenFrame.width * 0.78
        let maxHeight = screenFrame.height * 0.82
        let rawWidth = max(documentSize.width, 260)
        let rawHeight = max(documentSize.height, 320)
        let scale = min(1.0, maxWidth / rawWidth, (maxHeight - statusHeight) / rawHeight)
        return NSSize(
            width: min(max(rawWidth * scale, 320), maxWidth),
            height: min(max(rawHeight * scale + statusHeight, 420), maxHeight))
    }
}

struct GuestFrameWindowView: View {
    let lease: GuestFrameLease

    var body: some View {
        VStack(spacing: 0) {
            frameSurface
            statusLine
        }
        .background(Color(nsColor: .windowBackgroundColor))
    }

    private var frameSurface: some View {
        ZStack {
            Color(nsColor: .textBackgroundColor)

            if let document = lease.document {
                TransientPDFView(document: document)
            } else if let image = lease.image {
                Image(nsImage: image)
                    .resizable()
                    .scaledToFit()
            } else {
                Color(nsColor: .textBackgroundColor)
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }

    private var statusLine: some View {
        HStack(spacing: 8) {
            Circle()
                .fill(Color.green.opacity(0.85))
                .frame(width: 7, height: 7)
            Text(lease.status)
                .foregroundStyle(.secondary)
            Spacer()
            Text(lease.policyText)
                .foregroundStyle(.secondary)
        }
        .font(.system(size: 11))
        .lineLimit(1)
        .padding(.horizontal, 10)
        .frame(height: 28)
        .background(Color(nsColor: .windowBackgroundColor))
    }
}

struct TransientPDFView: NSViewRepresentable {
    let document: PDFDocument

    func makeNSView(context: Context) -> LockedPDFView {
        let view = LockedPDFView()
        view.autoScales = true
        view.displayMode = .singlePageContinuous
        view.displayDirection = .vertical
        view.displaysPageBreaks = true
        view.backgroundColor = .textBackgroundColor
        view.allowsDragging = false
        view.document = document
        DispatchQueue.main.async {
            view.window?.makeFirstResponder(view)
        }
        return view
    }

    func updateNSView(_ nsView: LockedPDFView, context: Context) {
        if nsView.document !== document {
            nsView.document = document
        }
        nsView.autoScales = true
    }
}

final class LockedPDFView: PDFView, NSMenuItemValidation {
    override var acceptsFirstResponder: Bool {
        true
    }

    override func performKeyEquivalent(with event: NSEvent) -> Bool {
        guard let key = event.charactersIgnoringModifiers?.lowercased() else {
            return super.performKeyEquivalent(with: event)
        }

        let modifierFlags = event.modifierFlags.intersection(.deviceIndependentFlagsMask)
        let isCopyModifier = modifierFlags.contains(.command) || modifierFlags.contains(.control)
        if isCopyModifier && key == "c" {
            copy(nil)
            return true
        }

        if isCopyModifier && key == "a" {
            selectAll(nil)
            return true
        }

        return super.performKeyEquivalent(with: event)
    }

    func validateMenuItem(_ menuItem: NSMenuItem) -> Bool {
        if let action = menuItem.action {
            let blockedActions = [
                "saveDocument:",
                "saveDocumentAs:",
                "saveToPDF:",
                "export:",
                "exportAs:",
                "print:",
                "runPageLayout:"
            ]
            if blockedActions.contains(NSStringFromSelector(action)) {
                return false
            }
        }

        return true
    }
}

@MainActor
final class FrameGuestModel: ObservableObject {
    @Published var visibleName: String
    @Published var host: String
    @Published var portText: String
    @Published var status = "macOSGuestAblage: STARTED"
    @Published var hasError = false

    var onPortalPulse: ((GuestPortalEdge) -> Void)?
    var onPortalError: ((GuestPortalEdge) -> Void)?
    var onPortalChanged: ((Bool, GuestPortalEdge) -> Void)?
    var onFrameReady: ((GuestFrameLease) -> Void)?
    var onAutoReturnFrame: ((GuestFrameLease) -> Void)?

    private var sessionId = ""
    private var activeFrameSessionIds = Set<String>()
    private var returnedFrameKeys = Set<String>()
    private var pendingTransferVisual = false
    private var autoReturnDelaySeconds: Double?
    private var exitAfterReturn = false
    private var portalEdge: GuestPortalEdge = .left
    private let configuration: MacGuestConfiguration

    init(configuration: MacGuestConfiguration = .loadDefault()) {
        self.configuration = configuration
        visibleName = configuration.visibleName
        host = configuration.ownerDiscovery.windowsOwnerHost
        portText = String(configuration.ownerDiscovery.devTransportPort)
    }

    func applyCommandLine() {
        let args = CommandLine.arguments
        let environment = ProcessInfo.processInfo.environment
        portalEdge = GuestPortalPlacement.resolveEdge(arguments: args)
        for index in args.indices {
            if args[index] == "--host", index + 1 < args.count {
                host = args[index + 1]
            }

            if args[index] == "--port", index + 1 < args.count {
                portText = args[index + 1]
            }

            if args[index] == "--auto-return-after", index + 1 < args.count {
                autoReturnDelaySeconds = Double(args[index + 1])
            }
        }

        if let returnDelay = environment["RKWS_AUTO_RETURN_AFTER"] {
            autoReturnDelaySeconds = Double(returnDelay)
        }

        exitAfterReturn = args.contains("--exit-after-return") ||
            environment["RKWS_EXIT_AFTER_RETURN"] == "1"
        RkwpTrace.log("apply host=\(host) port=\(portText) wait=\(args.contains("--wait-for-placement") || args.contains("--watch-for-frame")) autoOpen=\(args.contains("--auto-open")) edge=\(portalEdge.rawValue)")

        if args.contains("--local-frame") {
            showLocalVerificationFrame()
        } else if args.contains("--wait-for-placement") || args.contains("--watch-for-frame") {
            waitForPlacement()
        } else if args.contains("--auto-open") {
            openFrame()
        } else {
            waitForPlacement()
        }
    }

    func openFrame() {
        hasError = false
        status = "frage PDF-Lease an..."
        Task {
            do {
                let port = UInt16(portText) ?? 57120
                let client = RkwpDevLanClient(host: host, port: port)
                let hello = try await client.request(RkwpMessage(
                    messageType: "AblageHello",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: "",
                    payload: capabilities(waitForPlacement: false)))
                sessionId = hello.sessionId
                print("AblageHello: OK")

                _ = try await client.request(RkwpMessage(
                    messageType: "AblageCapabilities",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: sessionId,
                    payload: capabilities(waitForPlacement: false)))
                print("AblageCapabilities: OK")

                let frame = try await client.request(RkwpMessage(
                    messageType: "FrameUpdate",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: sessionId,
                    payload: frameRequest(waitForPlacement: false)))

                let lease = try makeFrameLease(frame, localVerification: false)
                if registerFrameLease(lease) {
                    onPortalPulse?(portalEdge)
                    onFrameReady?(lease)
                    scheduleAutoReturnIfNeeded(for: lease)
                }
            } catch {
                hasError = true
                status = "nicht verfuegbar: \(error.localizedDescription)"
                onPortalError?(portalEdge)
                onPortalChanged?(false, portalEdge)
            }
        }
    }

    func waitForPlacement() {
        hasError = false
        status = "hoert im Hintergrund auf PDF-Leases"
        onPortalChanged?(false, portalEdge)
        RkwpTrace.log("wait start host=\(host) port=\(portText)")
        Task {
            let port = UInt16(portText) ?? 57120
            let client = RkwpDevLanClient(host: host, port: port)
            var handshakeComplete = false

            while true {
                do {
                    if !handshakeComplete {
                        let hello = try await client.request(RkwpMessage(
                            messageType: "AblageHello",
                            sourceAblageId: "ablage-macos-guest",
                            targetAblageId: "ablage-windows-owner",
                            sessionId: "",
                            payload: capabilities(waitForPlacement: true)))
                        sessionId = hello.sessionId
                        print("AblageHello: OK")
                        RkwpTrace.log("hello ok session=\(sessionId)")

                        _ = try await client.request(RkwpMessage(
                            messageType: "AblageCapabilities",
                            sourceAblageId: "ablage-macos-guest",
                            targetAblageId: "ablage-windows-owner",
                            sessionId: sessionId,
                            payload: capabilities(waitForPlacement: true)))
                        print("AblageCapabilities: OK")
                        RkwpTrace.log("caps ok session=\(sessionId)")
                        handshakeComplete = true
                    }

                    let frame = try await client.request(RkwpMessage(
                        messageType: "FrameUpdate",
                        sourceAblageId: "ablage-macos-guest",
                        targetAblageId: "ablage-windows-owner",
                        sessionId: sessionId,
                        payload: frameRequest(waitForPlacement: true)))
                    let frameKey = Self.frameWindowKey(from: frame.payload, fallback: frame.payload["frameSessionId"] ?? "")
                    RkwpTrace.log("frame response ready=\(frame.payload["placementReady"] ?? "nil") renderable=\(Self.hasRenderableFrame(frame.payload)) key=\(frameKey) name=\(frame.payload["displayName"] ?? "") pdfChars=\(frame.payload["pdfBase64"]?.count ?? 0)")
                    if frame.payload["placementReady"] == "true" || Self.hasTransferIntent(frame.payload) {
                        pendingTransferVisual = true
                    }

                    if frame.payload["placementReady"] == "false" || !Self.hasRenderableFrame(frame.payload) {
                        hasError = false
                        status = frame.payload["visibleStatus"] ?? "bereit im Hintergrund"
                        if Self.hasTransferIntent(frame.payload) {
                            onPortalPulse?(portalEdge)
                        }
                        try? await Task.sleep(nanoseconds: 500_000_000)
                        continue
                    }

                    if shouldSkipFramePayload(frame.payload) {
                        RkwpTrace.log("frame skip duplicate key=\(frameKey)")
                        status = "Frame bereits offen, hoere weiter"
                        try? await Task.sleep(nanoseconds: 650_000_000)
                        continue
                    }

                    let lease = try makeFrameLease(frame, localVerification: false)
                    if registerFrameLease(lease) {
                        RkwpTrace.log("frame registered key=\(lease.windowKey)")
                        onPortalPulse?(portalEdge)
                        onFrameReady?(lease)
                        scheduleAutoReturnIfNeeded(for: lease)
                        pendingTransferVisual = false
                        status = "Frame geoeffnet, hoere weiter"
                    } else {
                        status = "Frame bereits offen, hoere weiter"
                    }

                    try? await Task.sleep(nanoseconds: 350_000_000)
                } catch {
                    hasError = true
                    status = "warte auf Windows-Ablage: \(error.localizedDescription)"
                    RkwpTrace.log("wait error \(error.localizedDescription)")
                    if pendingTransferVisual {
                        onPortalError?(portalEdge)
                        pendingTransferVisual = false
                    }
                    handshakeComplete = false
                    onPortalChanged?(false, portalEdge)
                    try? await Task.sleep(nanoseconds: 1_000_000_000)
                }
            }
        }
    }

    func returnFrame(_ lease: GuestFrameLease) {
        markFrameClosed(lease)
        guard !lease.frameSessionId.isEmpty else {
            return
        }

        hasError = false
        status = "gebe zurueck..."
        if lease.usesLocalVerificationFrame {
            finishReturn()
            return
        }

        Task {
            do {
                let port = UInt16(portText) ?? 57120
                let client = RkwpDevLanClient(host: host, port: port)
                _ = try await client.request(RkwpMessage(
                    messageType: "CarryLeaseReturn",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: lease.sessionId,
                    payload: [
                        "frameSessionId": lease.frameSessionId,
                        "leaseId": lease.leaseId,
                        "frameWindowKey": lease.windowKey,
                        "displayName": lease.displayName,
                        "returnReason": "guestFrameClosed",
                        "ownerMayReleaseLease": "true",
                        "guestKeptOriginalFile": "false",
                        "guestPersistedPdfFile": "false",
                        "transientPdfDiscarded": "true",
                        "pdfLeaseMode": "MemoryOnly"
                    ]))
                print("CarryLeaseReturn: OK")

                finishReturn()
            } catch {
                hasError = true
                status = "nicht verfuegbar: \(error.localizedDescription)"
                onPortalError?(portalEdge)
            }
        }
    }

    private func showLocalVerificationFrame() {
        do {
            let lease = try makeFrameLease(LocalVerificationFrame.transientPdfMessage(), localVerification: true)
            if registerFrameLease(lease) {
                onPortalPulse?(portalEdge)
                onFrameReady?(lease)
                scheduleAutoReturnIfNeeded(for: lease)
            }
        } catch {
            hasError = true
            status = "nicht verfuegbar: \(error.localizedDescription)"
        }
    }

    private func makeFrameLease(_ frame: RkwpMessage, localVerification: Bool) throws -> GuestFrameLease {
        try TransientPdfLeaseVerifier.validatePayload(frame.payload)
        let frameSessionId = frame.payload["frameSessionId"] ?? "frame-macos-\(UUID().uuidString)"
        let leaseId = frame.payload["leaseId"] ?? ""
        let displayName = frame.payload["displayName"] ?? "RK Workspace PDF Frame"
        let windowKey = Self.frameWindowKey(from: frame.payload, fallback: frameSessionId)

        if let pdfData = Self.decodePdfPayload(frame.payload),
           let document = PDFDocument(data: pdfData) {
            RkwpTrace.log("pdf lease decoded key=\(windowKey) bytes=\(pdfData.count)")
            status = "FrameView: OK"
            print("FrameView: OK")
            print("FrameFormat: \(frame.payload["frameFormat"] ?? "TransientPdfBytes")")
            print("TransientPdfLease: OK")
            print("OwnerKeepsOriginal: OK")
            print("GuestPersistedPdfFile: NO")
            print("GuestHasOriginalPath: NO")
            print("PDFCache: MemoryOnly")
            print("TextSelection: OK")
            print("NoDiskPdf: SUCCESS")
            return GuestFrameLease(
                windowKey: windowKey,
                frameSessionId: frameSessionId,
                leaseId: leaseId,
                sessionId: frame.sessionId,
                displayName: displayName,
                status: "FrameView: OK",
                policyText: "Owner: Windows | PDF: MemoryOnly | Ablage: NO | Text: OK",
                document: document,
                image: nil,
                documentSize: Self.documentDisplaySize(document),
                pdfData: pdfData,
                usesLocalVerificationFrame: localVerification)
        }

        if let pngBase64 = frame.payload["pngBase64"],
           let pngData = Data(base64Encoded: pngBase64),
           let nsImage = NSImage(data: pngData) {
            RkwpTrace.log("png frame decoded key=\(windowKey) bytes=\(pngData.count)")
            status = "FrameView: OK"
            print("FrameView: OK")
            print("LegacyPngFrame: OK")
            return GuestFrameLease(
                windowKey: windowKey,
                frameSessionId: frameSessionId,
                leaseId: leaseId,
                sessionId: frame.sessionId,
                displayName: displayName,
                status: "FrameView: OK",
                policyText: "Owner: Windows | PNG-Fallback | Ablage: NO",
                document: nil,
                image: nsImage,
                documentSize: nsImage.size,
                pdfData: nil,
                usesLocalVerificationFrame: localVerification)
        }

        throw FrameGuestError.invalidFrame
    }

    private func registerFrameLease(_ lease: GuestFrameLease) -> Bool {
        guard !activeFrameSessionIds.contains(lease.windowKey),
              !returnedFrameKeys.contains(lease.windowKey) else {
            return false
        }

        activeFrameSessionIds.insert(lease.windowKey)
        return true
    }

    private func shouldSkipFramePayload(_ payload: [String: String]) -> Bool {
        let key = Self.frameWindowKey(from: payload, fallback: payload["frameSessionId"] ?? "")
        return activeFrameSessionIds.contains(key) || returnedFrameKeys.contains(key)
    }

    private func markFrameClosed(_ lease: GuestFrameLease) {
        activeFrameSessionIds.remove(lease.windowKey)
        returnedFrameKeys.insert(lease.windowKey)
        RkwpTrace.log("frame returned key=\(lease.windowKey)")
    }

    private static func decodePdfPayload(_ payload: [String: String]) -> Data? {
        let keys = [
            "pdfBase64",
            "pdfBytesBase64",
            "transientPdfBase64",
            "leasedPdfBase64",
            "pdfLeaseBase64"
        ]

        for key in keys {
            if let value = payload[key],
               let data = Data(base64Encoded: value, options: [.ignoreUnknownCharacters]) {
                return data
            }
        }

        return nil
    }

    private static func hasRenderableFrame(_ payload: [String: String]) -> Bool {
        decodePdfPayload(payload) != nil || payload["pngBase64"] != nil
    }

    private static func hasTransferIntent(_ payload: [String: String]) -> Bool {
        let truthyKeys = [
            "carryActive",
            "transferActive",
            "portalActive",
            "placementSignal",
            "placementSignalReady",
            "prestreamActive"
        ]
        if truthyKeys.contains(where: { payload[$0]?.lowercased() == "true" }) {
            return true
        }

        let visibleStatus = (payload["visibleStatus"] ?? "").lowercased()
        return visibleStatus.contains("signal erkannt") ||
            visibleStatus.contains("carry") ||
            visibleStatus.contains("transport") ||
            visibleStatus.contains("uebertragung") ||
            visibleStatus.contains("übertragung")
    }

    private static func frameWindowKey(from payload: [String: String], fallback: String) -> String {
        let keys = [
            "transferId",
            "carryId",
            "carrySessionId",
            "placementId",
            "thingId",
            "sourceHash",
            "displayName",
            "frameSessionId"
        ]

        for key in keys {
            if let value = payload[key], !value.isEmpty {
                return "\(key):\(value)"
            }
        }

        return "frameSessionId:\(fallback)"
    }

    private static func documentDisplaySize(_ document: PDFDocument) -> CGSize {
        guard let page = document.page(at: 0) else {
            return CGSize(width: 420, height: 620)
        }

        let bounds = page.bounds(for: .cropBox)
        if bounds.width > 0 && bounds.height > 0 {
            return CGSize(width: bounds.width, height: bounds.height)
        }

        let media = page.bounds(for: .mediaBox)
        return CGSize(width: max(media.width, 420), height: max(media.height, 620))
    }

    private func capabilities(waitForPlacement: Bool) -> [String: String] {
        [
            "displayName": "Ablage macOS",
            "frameOnly": "true",
            "transientPdfFrame": "true",
            "supportsTransientPdfBytes": "true",
            "supportsPngFallback": "true",
            "supportsMultipleFrameWindows": "true",
            "keepsListeningAfterFrameClose": "true",
            "portalIdleVisible": "false",
            "portalAnimation": "fastOpenSlowClose",
            "pdfLeaseMode": "MemoryOnly",
            "ownerKeepsOriginal": "true",
            "guestMayPersistPdf": "false",
            "guestMayExportPdf": "false",
            "allowTextSelection": "true",
            "waitForPlacement": waitForPlacement ? "true" : "false"
        ]
    }

    private func frameRequest(waitForPlacement: Bool) -> [String: String] {
        [
            "request": "openTransientPdfFrame",
            "frameWindowMode": "newWindowPerLease",
            "preferredFrameFormat": "TransientPdfBytes",
            "fallbackFrameFormat": "PngFrame",
            "pdfLeaseMode": "MemoryOnly",
            "ownerKeepsOriginal": "true",
            "guestMayPersistPdf": "false",
            "guestMayExportPdf": "false",
            "allowTextSelection": "true",
            "waitForPlacement": waitForPlacement ? "true" : "false"
        ]
    }

    private func clearFrame() {
        activeFrameSessionIds.removeAll()
    }

    private func finishReturn() {
        status = "Return: SUCCESS"
        onPortalChanged?(false, portalEdge)
        print("Return: SUCCESS")

        if exitAfterReturn {
            NSApplication.shared.terminate(nil)
        }
    }

    private func scheduleAutoReturnIfNeeded(for lease: GuestFrameLease) {
        guard let delay = autoReturnDelaySeconds else {
            return
        }

        autoReturnDelaySeconds = nil
        let nanoseconds = UInt64(max(0.1, delay) * 1_000_000_000)
        Task { @MainActor in
            try? await Task.sleep(nanoseconds: nanoseconds)
            onAutoReturnFrame?(lease)
        }
    }
}

enum TransientPdfLeaseVerifier {
    private static let forbiddenPayloadKeys = Set([
        "originalpath",
        "localpdfpath",
        "downloadpath",
        "filepath",
        "savedpath",
        "cachedpath"
    ])

    static func validatePayload(_ payload: [String: String]) throws {
        for key in payload.keys where forbiddenPayloadKeys.contains(key.lowercased()) {
            throw FrameGuestError.forbiddenPayloadKey(key)
        }

        if payload["ownerKeepsOriginal"]?.lowercased() == "false" {
            throw FrameGuestError.invalidFrame
        }

        if payload["guestMayPersistPdf"]?.lowercased() == "true" ||
            payload["guestHasPdfFile"]?.lowercased() == "true" ||
            payload["hasOriginalPath"]?.lowercased() == "true" {
            throw FrameGuestError.invalidFrame
        }

        if let frameCache = payload["frameCache"], frameCache != "MemoryOnly" {
            throw FrameGuestError.invalidFrame
        }

        if let pdfCache = payload["pdfCache"], pdfCache != "MemoryOnly" {
            throw FrameGuestError.invalidFrame
        }
    }
}

enum GuestPortalEdge: String {
    case left = "Left"
    case right = "Right"
    case top = "Up"
    case bottom = "Down"

    static func parse(_ value: String) -> GuestPortalEdge {
        switch value.lowercased() {
        case "left":
            return .left
        case "right":
            return .right
        case "up", "top":
            return .top
        case "down", "bottom":
            return .bottom
        default:
            return .left
        }
    }
}

enum GuestPortalPlacement {
    static func resolveEdge(arguments: [String]) -> GuestPortalEdge {
        var edge = nearestMappedWindowsEdge() ?? .left
        for index in arguments.indices {
            if ["--edge", "--direction"].contains(arguments[index]), index + 1 < arguments.count {
                edge = GuestPortalEdge.parse(arguments[index + 1])
            }
        }
        return edge
    }

    private static func nearestMappedWindowsEdge() -> GuestPortalEdge? {
        guard let root = MacGuestPaths.repositoryRoot() else {
            return nil
        }

        let candidates = [
            root.appendingPathComponent("config/manual-ablage-map.json"),
            root.appendingPathComponent("config/samples/manual-ablage-map.sample.json")
        ]

        for url in candidates {
            guard let data = try? Data(contentsOf: url),
                  let map = try? JSONDecoder().decode(ManualAblageMap.self, from: data),
                  let entry = map.entries
                    .filter({ $0.isAvailable && ($0.platform?.lowercased().contains("windows") == true || $0.displayName.lowercased().contains("windows")) })
                    .sorted(by: {
                        ($0.distanceMeters ?? Double.greatestFiniteMagnitude, -($0.confidence ?? 0)) <
                            ($1.distanceMeters ?? Double.greatestFiniteMagnitude, -($1.confidence ?? 0))
                    })
                    .first else {
                continue
            }

            return GuestPortalEdge.parse(entry.relativeDirection)
        }

        return nil
    }

    private struct ManualAblageMap: Decodable {
        let entries: [Entry]

        struct Entry: Decodable {
            let displayName: String
            let relativeDirection: String
            let distanceMeters: Double?
            let confidence: Double?
            let isAvailable: Bool
            let platform: String?
        }
    }
}

enum GuestPortalTone {
    case normal
    case error
}

@MainActor
final class GuestGlassOverlayController {
    private let state = GuestGlassOverlayState()
    private var window: NSPanel?
    private var fadeTask: Task<Void, Never>?

    func pulse(edge: GuestPortalEdge) {
        show(edge: edge, tone: .normal)
        fadeTask?.cancel()
        fadeTask = Task { @MainActor [weak self] in
            try? await Task.sleep(nanoseconds: 650_000_000)
            self?.hide()
        }
    }

    func errorPulse(edge: GuestPortalEdge) {
        show(edge: edge, tone: .error)
        fadeTask?.cancel()
        fadeTask = Task { @MainActor [weak self] in
            try? await Task.sleep(nanoseconds: 900_000_000)
            self?.hide()
        }
    }

    func show(edge: GuestPortalEdge) {
        show(edge: edge, tone: .normal)
    }

    private func show(edge: GuestPortalEdge, tone: GuestPortalTone) {
        fadeTask?.cancel()
        state.edge = edge
        state.tone = tone
        if window == nil {
            createWindow()
        }
        window?.orderFrontRegardless()
        state.isMounted = true
        withAnimation(.easeOut(duration: 0.18)) {
            state.openness = 1.0
        }
    }

    func hide() {
        fadeTask?.cancel()
        withAnimation(.easeOut(duration: 2.6)) {
            state.openness = 0.0
        }
        fadeTask = Task { @MainActor [weak self] in
            try? await Task.sleep(nanoseconds: 2_900_000_000)
            self?.orderOutIfClosed()
        }
    }

    private func orderOutIfClosed() {
        guard state.openness <= 0.01 else {
            return
        }

        state.isMounted = false
        window?.orderOut(nil)
    }

    private func createWindow() {
        let frame = NSScreen.screens
            .map(\.frame)
            .reduce(NSRect.null) { $0.union($1) }
        let panel = NSPanel(
            contentRect: frame,
            styleMask: [.borderless, .nonactivatingPanel],
            backing: .buffered,
            defer: false)
        panel.level = .screenSaver
        panel.collectionBehavior = [.canJoinAllSpaces, .fullScreenAuxiliary, .stationary, .ignoresCycle]
        panel.backgroundColor = .clear
        panel.isOpaque = false
        panel.hasShadow = false
        panel.ignoresMouseEvents = true
        panel.contentView = NSHostingView(rootView: GuestGlassOverlayView(state: state))
        window = panel
    }
}

@MainActor
final class GuestGlassOverlayState: ObservableObject {
    @Published var isMounted = false
    @Published var openness = 0.0
    @Published var edge: GuestPortalEdge = .left
    @Published var tone: GuestPortalTone = .normal
}

struct GuestGlassOverlayView: View {
    @ObservedObject var state: GuestGlassOverlayState

    var body: some View {
        GeometryReader { geometry in
            ZStack {
                if state.isMounted {
                    let frame = edgeFrame(in: geometry.size)
                    GuestProgressiveGlassEdge(edge: state.edge, openness: state.openness, tone: state.tone)
                        .frame(width: frame.width, height: frame.height)
                        .position(x: frame.midX, y: frame.midY)
                        .opacity(state.openness)
                }
            }
            .frame(width: geometry.size.width, height: geometry.size.height)
        }
        .allowsHitTesting(false)
    }

    private func edgeFrame(in size: CGSize) -> CGRect {
        let depth = CGFloat(190 + (70 * state.openness))
        switch state.edge {
        case .left:
            return CGRect(x: 0, y: 0, width: depth, height: size.height)
        case .right:
            return CGRect(x: size.width - depth, y: 0, width: depth, height: size.height)
        case .top:
            return CGRect(x: 0, y: 0, width: size.width, height: depth)
        case .bottom:
            return CGRect(x: 0, y: size.height - depth, width: size.width, height: depth)
        }
    }
}

struct GuestProgressiveGlassEdge: View {
    let edge: GuestPortalEdge
    let openness: Double
    let tone: GuestPortalTone

    var body: some View {
        GeometryReader { geometry in
            ZStack {
                Rectangle()
                    .fill(.regularMaterial)
                    .opacity(0.72 + (0.24 * openness))
                    .mask(progressiveMask)

                Rectangle()
                    .fill(bodyGradient)
                    .opacity(0.72 + (0.28 * openness))
                    .mask(progressiveMask)

                Rectangle()
                    .fill(lightGradient)
                    .blur(radius: 12 + (10 * openness))
                    .opacity(0.7 + (0.3 * openness))
                    .mask(progressiveMask)

                throat(in: geometry.size)
                physicalEdge(in: geometry.size)
            }
        }
    }

    private var progressiveMask: LinearGradient {
        switch edge {
        case .right:
            return LinearGradient(
                stops: [
                    .init(color: .clear, location: 0.00),
                    .init(color: .black.opacity(0.08), location: 0.18),
                    .init(color: .black.opacity(0.68), location: 0.58),
                    .init(color: .black, location: 1.00)
                ],
                startPoint: .leading,
                endPoint: .trailing)
        case .left:
            return LinearGradient(
                stops: [
                    .init(color: .black, location: 0.00),
                    .init(color: .black.opacity(0.68), location: 0.42),
                    .init(color: .black.opacity(0.08), location: 0.82),
                    .init(color: .clear, location: 1.00)
                ],
                startPoint: .leading,
                endPoint: .trailing)
        case .top:
            return LinearGradient(
                stops: [
                    .init(color: .black, location: 0.00),
                    .init(color: .black.opacity(0.68), location: 0.42),
                    .init(color: .black.opacity(0.08), location: 0.82),
                    .init(color: .clear, location: 1.00)
                ],
                startPoint: .top,
                endPoint: .bottom)
        case .bottom:
            return LinearGradient(
                stops: [
                    .init(color: .clear, location: 0.00),
                    .init(color: .black.opacity(0.08), location: 0.18),
                    .init(color: .black.opacity(0.68), location: 0.58),
                    .init(color: .black, location: 1.00)
                ],
                startPoint: .top,
                endPoint: .bottom)
        }
    }

    private var bodyGradient: LinearGradient {
        switch edge {
        case .right:
            return LinearGradient(colors: [.clear, .white.opacity(0.05), accent.opacity(0.16), highlight.opacity(0.28)], startPoint: .leading, endPoint: .trailing)
        case .left:
            return LinearGradient(colors: [highlight.opacity(0.28), accent.opacity(0.16), .white.opacity(0.05), .clear], startPoint: .leading, endPoint: .trailing)
        case .top:
            return LinearGradient(colors: [highlight.opacity(0.28), accent.opacity(0.16), .white.opacity(0.05), .clear], startPoint: .top, endPoint: .bottom)
        case .bottom:
            return LinearGradient(colors: [.clear, .white.opacity(0.05), accent.opacity(0.16), highlight.opacity(0.28)], startPoint: .top, endPoint: .bottom)
        }
    }

    private var lightGradient: LinearGradient {
        switch edge {
        case .left, .right:
            return LinearGradient(colors: [.clear, highlight.opacity(0.38), accent.opacity(0.30), highlight.opacity(0.38), .clear], startPoint: .top, endPoint: .bottom)
        case .top, .bottom:
            return LinearGradient(colors: [.clear, highlight.opacity(0.38), accent.opacity(0.30), highlight.opacity(0.38), .clear], startPoint: .leading, endPoint: .trailing)
        }
    }

    private var accent: Color {
        tone == .error ? .red : .cyan
    }

    private var highlight: Color {
        tone == .error ? Color(red: 1.0, green: 0.22, blue: 0.18) : .white
    }

    @ViewBuilder
    private func throat(in size: CGSize) -> some View {
        let open = CGFloat(max(0, min(1, openness)))
        if edge == .left || edge == .right {
            Capsule()
                .fill(lightGradient)
                .frame(width: 8 + (12 * open), height: min(size.height * (0.24 + (0.16 * open)), 360))
                .position(x: edge == .left ? 14 + (8 * open) : size.width - 14 - (8 * open), y: size.height / 2)
                .shadow(color: accent.opacity(0.18 + (0.26 * open)), radius: 10 + (20 * open))
        } else {
            Capsule()
                .fill(lightGradient)
                .frame(width: min(size.width * (0.24 + (0.16 * open)), 420), height: 8 + (12 * open))
                .position(x: size.width / 2, y: edge == .top ? 14 + (8 * open) : size.height - 14 - (8 * open))
                .shadow(color: accent.opacity(0.18 + (0.26 * open)), radius: 10 + (20 * open))
        }
    }

    @ViewBuilder
    private func physicalEdge(in size: CGSize) -> some View {
        if edge == .left || edge == .right {
            Rectangle()
                .fill(lightGradient)
                .frame(width: 2, height: size.height)
                .position(x: edge == .left ? 1 : size.width - 1, y: size.height / 2)
        } else {
            Rectangle()
                .fill(lightGradient)
                .frame(width: size.width, height: 2)
                .position(x: size.width / 2, y: edge == .top ? 1 : size.height - 1)
        }
    }
}

struct RkwpMessage: Codable {
    var messageId: String = "rkwp-macos-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())"
    var messageType: String
    var sourceAblageId: String
    var targetAblageId: String
    var sessionId: String
    var correlationId: String = ""
    var timestamp: Date = Date()
    var payload: [String: String]
    var headers: [String: String] = [:]
}

final class RkwpDevLanClient: @unchecked Sendable {
    private let host: String
    private let port: UInt16

    init(host: String, port: UInt16) {
        self.host = host
        self.port = port
    }

    func request(_ message: RkwpMessage) async throws -> RkwpMessage {
        try await withCheckedThrowingContinuation { continuation in
            DispatchQueue.global(qos: .userInitiated).async {
                do {
                    continuation.resume(returning: try self.requestSync(message))
                } catch {
                    continuation.resume(throwing: error)
                }
            }
        }
    }

    private func requestSync(_ message: RkwpMessage) throws -> RkwpMessage {
        let encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601
        let requestData = try encoder.encode(message)
        guard var requestLine = String(data: requestData, encoding: .utf8) else {
            throw FrameGuestError.encodingFailed
        }

        requestLine.append("\n")
        let fd = try openSocket()
        defer {
            Darwin.close(fd)
        }

        try writeAll(Array(requestLine.utf8), to: fd)
        var response = Data()
        var buffer = [UInt8](repeating: 0, count: 64 * 1024)
        let bufferSize = buffer.count

        while true {
            let read = buffer.withUnsafeMutableBytes {
                Darwin.recv(fd, $0.baseAddress, bufferSize, 0)
            }

            if read < 0 {
                if errno == EAGAIN || errno == EWOULDBLOCK {
                    break
                }
                throw FrameGuestError.readFailed
            }

            if read == 0 {
                break
            }

            response.append(buffer, count: read)
            if response.contains(0x0A) {
                break
            }
        }

        guard !response.isEmpty else {
            throw FrameGuestError.emptyResponse
        }

        if let newline = response.firstIndex(of: 0x0A) {
            response = response[..<newline]
        }

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in
            try RkwpDateCoding.decode(decoder)
        }
        return try decoder.decode(RkwpMessage.self, from: response)
    }

    private func openSocket() throws -> Int32 {
        if let fd = try openNumericIPv4SocketIfPossible() {
            return fd
        }

        var hints = addrinfo(
            ai_flags: 0,
            ai_family: AF_UNSPEC,
            ai_socktype: SOCK_STREAM,
            ai_protocol: IPPROTO_TCP,
            ai_addrlen: 0,
            ai_canonname: nil,
            ai_addr: nil,
            ai_next: nil)
        var result: UnsafeMutablePointer<addrinfo>?
        let lookup = getaddrinfo(host, String(port), &hints, &result)
        guard lookup == 0, let result else {
            RkwpTrace.log("socket getaddrinfo failed host=\(host) port=\(port) code=\(lookup)")
            throw FrameGuestError.connectionFailed
        }
        defer {
            freeaddrinfo(result)
        }

        var candidate: UnsafeMutablePointer<addrinfo>? = result
        while let current = candidate {
            let info = current.pointee
            let fd = Darwin.socket(info.ai_family, info.ai_socktype, info.ai_protocol)
            if fd >= 0 {
                setTimeouts(on: fd)
                if Darwin.connect(fd, info.ai_addr, info.ai_addrlen) == 0 {
                    return fd
                }
                RkwpTrace.log("socket connect candidate failed host=\(host) port=\(port) errno=\(errno)")
                Darwin.close(fd)
            }
            candidate = info.ai_next
        }

        throw FrameGuestError.connectionFailed
    }

    private func openNumericIPv4SocketIfPossible() throws -> Int32? {
        var address = in_addr()
        let parsed = host.withCString {
            inet_pton(AF_INET, $0, &address)
        }
        guard parsed == 1 else {
            return nil
        }

        let fd = Darwin.socket(AF_INET, SOCK_STREAM, IPPROTO_TCP)
        guard fd >= 0 else {
            RkwpTrace.log("socket ipv4 create failed host=\(host) port=\(port) errno=\(errno)")
            throw FrameGuestError.connectionFailed
        }

        setTimeouts(on: fd)

        var socketAddress = sockaddr_in()
        socketAddress.sin_len = UInt8(MemoryLayout<sockaddr_in>.size)
        socketAddress.sin_family = sa_family_t(AF_INET)
        socketAddress.sin_port = port.bigEndian
        socketAddress.sin_addr = address

        let connected = withUnsafePointer(to: &socketAddress) { pointer in
            pointer.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                Darwin.connect(fd, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
            }
        }

        if connected == 0 {
            return fd
        }

        RkwpTrace.log("socket ipv4 connect failed host=\(host) port=\(port) errno=\(errno)")
        Darwin.close(fd)
        throw FrameGuestError.connectionFailed
    }

    private func setTimeouts(on fd: Int32) {
        var timeout = timeval(tv_sec: 10, tv_usec: 0)
        _ = withUnsafePointer(to: &timeout) {
            setsockopt(fd, SOL_SOCKET, SO_RCVTIMEO, $0, socklen_t(MemoryLayout<timeval>.size))
        }
        _ = withUnsafePointer(to: &timeout) {
            setsockopt(fd, SOL_SOCKET, SO_SNDTIMEO, $0, socklen_t(MemoryLayout<timeval>.size))
        }
    }

    private func writeAll(_ bytes: [UInt8], to fd: Int32) throws {
        try bytes.withUnsafeBytes { rawBuffer in
            guard let base = rawBuffer.bindMemory(to: UInt8.self).baseAddress else {
                throw FrameGuestError.writeFailed
            }

            var sent = 0
            while sent < bytes.count {
                let written = Darwin.send(fd, base.advanced(by: sent), bytes.count - sent, 0)
                if written <= 0 {
                    throw FrameGuestError.writeFailed
                }
                sent += written
            }
        }
    }
}

enum RkwpDateCoding {
    private static let fractionalFormatter: ISO8601DateFormatter = {
        let formatter = ISO8601DateFormatter()
        formatter.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
        return formatter
    }()

    private static let plainFormatter: ISO8601DateFormatter = {
        let formatter = ISO8601DateFormatter()
        formatter.formatOptions = [.withInternetDateTime]
        return formatter
    }()

    static func decode(_ decoder: Decoder) throws -> Date {
        let container = try decoder.singleValueContainer()
        let value = try container.decode(String.self)
        if let date = fractionalFormatter.date(from: value) ?? plainFormatter.date(from: value) {
            return date
        }

        throw DecodingError.dataCorruptedError(
            in: container,
            debugDescription: "Invalid RKWP timestamp: \(value)")
    }
}

enum RkwpTrace {
    static func log(_ message: String) {
        let directory = FileManager.default.homeDirectoryForCurrentUser
            .appendingPathComponent("Library/Logs/RKWorkspace", isDirectory: true)
        let url = directory.appendingPathComponent("mac-pdf-frame-guest.trace.log")
        let timestamp = ISO8601DateFormatter().string(from: Date())
        let line = "\(timestamp) \(message)\n"
        guard let data = line.data(using: .utf8) else {
            return
        }

        try? FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
        if !FileManager.default.fileExists(atPath: url.path) {
            FileManager.default.createFile(atPath: url.path, contents: nil)
        }

        guard let handle = try? FileHandle(forWritingTo: url) else {
            return
        }

        defer {
            try? handle.close()
        }

        _ = try? handle.seekToEnd()
        _ = try? handle.write(contentsOf: data)
    }
}

enum FrameGuestError: LocalizedError {
    case connectionFailed
    case encodingFailed
    case writeFailed
    case readFailed
    case emptyResponse
    case invalidFrame
    case forbiddenPayloadKey(String)
    case repositoryRootNotFound

    var errorDescription: String? {
        switch self {
        case .connectionFailed:
            return "keine Verbindung zur Windows-Ablage"
        case .encodingFailed:
            return "Nachricht konnte nicht vorbereitet werden"
        case .writeFailed:
            return "Nachricht konnte nicht gesendet werden"
        case .readFailed:
            return "Antwort konnte nicht gelesen werden"
        case .emptyResponse:
            return "keine Antwort"
        case .invalidFrame:
            return "Frame ist ungueltig"
        case .forbiddenPayloadKey(let key):
            return "Frame enthaelt unzulaessiges Feld: \(key)"
        case .repositoryRootNotFound:
            return "Repository nicht gefunden"
        }
    }
}
