import AppKit
import Darwin
import PDFKit
import SwiftUI

@main
@MainActor
final class MacPdfFrameGuestApp: NSObject, NSApplicationDelegate, NSWindowDelegate {
    private static var retainedDelegate: MacPdfFrameGuestApp?
    private let glassOverlay = GuestGlassOverlayController()
    private var window: NSWindow?
    private var model: FrameGuestModel?

    static func main() {
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
        model.onDocumentReady = { [weak self] size in
            self?.resizeFrameWindow(for: size)
        }
        model.onPortalChanged = { [weak self] isVisible, edge in
            if isVisible {
                self?.glassOverlay.show(edge: edge)
            } else {
                self?.glassOverlay.hide()
            }
        }

        let content = ContentView(model: model)
        let window = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 520, height: 680),
            styleMask: [.titled, .closable, .miniaturizable, .resizable],
            backing: .buffered,
            defer: false)
        window.title = "RK Workspace Ablage"
        window.contentView = NSHostingView(rootView: content)
        window.isReleasedWhenClosed = false
        window.delegate = self
        window.center()
        window.makeKeyAndOrderFront(nil)
        NSApplication.shared.activate(ignoringOtherApps: true)
        self.window = window

        model.applyCommandLine()
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        false
    }

    func windowWillClose(_ notification: Notification) {
        model?.returnFrame()
        model?.discardTransientFrame()
    }

    private func resizeFrameWindow(for documentSize: CGSize) {
        guard let window else {
            return
        }

        let screenFrame = window.screen?.visibleFrame ?? NSScreen.main?.visibleFrame ?? NSRect(x: 0, y: 0, width: 1280, height: 800)
        let statusHeight: CGFloat = 28
        let maxWidth = screenFrame.width * 0.78
        let maxHeight = screenFrame.height * 0.82
        let rawWidth = max(documentSize.width, 260)
        let rawHeight = max(documentSize.height, 320)
        let scale = min(1.0, maxWidth / rawWidth, (maxHeight - statusHeight) / rawHeight)
        let contentSize = NSSize(
            width: min(max(rawWidth * scale, 320), maxWidth),
            height: min(max(rawHeight * scale + statusHeight, 420), maxHeight))
        var frame = window.frameRect(forContentRect: NSRect(origin: .zero, size: contentSize))
        frame.origin.x = screenFrame.midX - frame.width / 2
        frame.origin.y = screenFrame.midY - frame.height / 2
        window.setFrame(frame, display: true, animate: true)
    }
}

struct ContentView: View {
    @ObservedObject var model: FrameGuestModel

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

            if let document = model.pdfDocument {
                TransientPDFView(document: document)
                    .transition(.opacity)
            } else if let image = model.image {
                Image(nsImage: image)
                    .resizable()
                    .scaledToFit()
                    .transition(.opacity)
            } else {
                VStack(spacing: 10) {
                    Text(model.visibleState)
                        .font(.system(size: 18, weight: .medium))
                    Text("Windows bleibt Owner. macOS wartet auf die fluechtige Frame-PDF.")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }

    private var statusLine: some View {
        HStack(spacing: 8) {
            Circle()
                .fill(model.hasError ? Color.red : Color.green.opacity(0.85))
                .frame(width: 7, height: 7)
            Text(model.status)
                .foregroundStyle(model.hasError ? .red : .secondary)
            Spacer()
            Text(model.policyText)
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
    @Published var visibleState = "wartet auf Portal"
    @Published var policyText = "Owner: Windows | PDF: MemoryOnly | Ablage: NO"
    @Published var pdfDocument: PDFDocument?
    @Published var image: NSImage?
    @Published var hasError = false

    var frameSessionId = ""
    var onDocumentReady: ((CGSize) -> Void)?
    var onPortalChanged: ((Bool, GuestPortalEdge) -> Void)?
    private var leaseId = ""
    private var sessionId = ""
    private var transientPdfData: Data?
    private var usesLocalVerificationFrame = false
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

        if args.contains("--local-frame") {
            showLocalVerificationFrame()
        } else if args.contains("--wait-for-placement") || args.contains("--watch-for-frame") {
            waitForPlacement()
        } else if args.contains("--auto-open") {
            openFrame()
        } else {
            onPortalChanged?(true, portalEdge)
        }
    }

    func openFrame() {
        hasError = false
        status = "frage PDF-Lease an..."
        onPortalChanged?(true, portalEdge)
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

                try presentFrame(frame, localVerification: false)
            } catch {
                hasError = true
                status = "nicht verfuegbar: \(error.localizedDescription)"
            }
        }
    }

    func waitForPlacement() {
        hasError = false
        clearFrame()
        visibleState = "wartet auf Ablage am Glasrand"
        status = "warte auf deine PDF von Windows..."
        onPortalChanged?(true, portalEdge)
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

                        _ = try await client.request(RkwpMessage(
                            messageType: "AblageCapabilities",
                            sourceAblageId: "ablage-macos-guest",
                            targetAblageId: "ablage-windows-owner",
                            sessionId: sessionId,
                            payload: capabilities(waitForPlacement: true)))
                        print("AblageCapabilities: OK")
                        handshakeComplete = true
                    }

                    let frame = try await client.request(RkwpMessage(
                        messageType: "FrameUpdate",
                        sourceAblageId: "ablage-macos-guest",
                        targetAblageId: "ablage-windows-owner",
                        sessionId: sessionId,
                        payload: frameRequest(waitForPlacement: true)))

                    if frame.payload["placementReady"] == "false" || !Self.hasRenderableFrame(frame.payload) {
                        hasError = false
                        visibleState = frame.payload["visibleStatus"] ?? "wartet auf Ablage am Glasrand"
                        status = "bereit am Portal"
                        onPortalChanged?(true, portalEdge)
                        try? await Task.sleep(nanoseconds: 500_000_000)
                        continue
                    }

                    try presentFrame(frame, localVerification: false)
                    break
                } catch {
                    hasError = true
                    status = "warte auf Windows-Ablage: \(error.localizedDescription)"
                    try? await Task.sleep(nanoseconds: 1_000_000_000)
                }
            }
        }
    }

    func returnFrame() {
        guard !frameSessionId.isEmpty else {
            discardTransientFrame()
            return
        }

        hasError = false
        status = "gebe zurueck..."
        if usesLocalVerificationFrame {
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
                    sessionId: sessionId,
                    payload: [
                        "frameSessionId": frameSessionId,
                        "leaseId": leaseId,
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
            }
        }
    }

    private func showLocalVerificationFrame() {
        do {
            try presentFrame(LocalVerificationFrame.transientPdfMessage(), localVerification: true)
        } catch {
            hasError = true
            status = "nicht verfuegbar: \(error.localizedDescription)"
        }
    }

    private func presentFrame(_ frame: RkwpMessage, localVerification: Bool) throws {
        try TransientPdfLeaseVerifier.validatePayload(frame.payload)
        frameSessionId = frame.payload["frameSessionId"] ?? ""
        leaseId = frame.payload["leaseId"] ?? ""
        sessionId = frame.sessionId
        usesLocalVerificationFrame = localVerification
        visibleState = frame.payload["visibleStatus"] ?? "liegt hier im Frame"
        onPortalChanged?(false, portalEdge)

        if let pdfData = Self.decodePdfPayload(frame.payload),
           let document = PDFDocument(data: pdfData) {
            transientPdfData = pdfData
            pdfDocument = document
            image = nil
            onDocumentReady?(Self.documentDisplaySize(document))
            status = "FrameView: OK"
            policyText = "Owner: Windows | PDF: MemoryOnly | Ablage: NO | Text: OK"
            print("FrameView: OK")
            print("TransientPdfLease: OK")
            print("OwnerKeepsOriginal: OK")
            print("GuestPersistedPdfFile: NO")
            print("GuestHasOriginalPath: NO")
            print("PDFCache: MemoryOnly")
            print("TextSelection: OK")
            print("NoDiskPdf: SUCCESS")
            scheduleAutoReturnIfNeeded()
            return
        }

        if let pngBase64 = frame.payload["pngBase64"],
           let pngData = Data(base64Encoded: pngBase64),
           let nsImage = NSImage(data: pngData) {
            transientPdfData = nil
            pdfDocument = nil
            image = nsImage
            onDocumentReady?(nsImage.size)
            status = "FrameView: OK"
            policyText = "Owner: Windows | PNG-Fallback | Ablage: NO"
            print("FrameView: OK")
            print("LegacyPngFrame: OK")
            scheduleAutoReturnIfNeeded()
            return
        }

        throw FrameGuestError.invalidFrame
    }

    func discardTransientFrame() {
        transientPdfData = nil
        pdfDocument = nil
        image = nil
        frameSessionId = ""
        leaseId = ""
        policyText = "Owner: Windows | PDF: verworfen | Ablage: NO"
        onPortalChanged?(false, portalEdge)
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
        transientPdfData = nil
        pdfDocument = nil
        image = nil
        frameSessionId = ""
        leaseId = ""
        usesLocalVerificationFrame = false
    }

    private func finishReturn() {
        clearFrame()
        visibleState = "zurueckgegeben"
        status = "Return: SUCCESS"
        policyText = "Owner: Windows | PDF: verworfen | Ablage: NO"
        onPortalChanged?(false, portalEdge)
        print("Return: SUCCESS")

        if exitAfterReturn {
            NSApplication.shared.terminate(nil)
        }
    }

    private func scheduleAutoReturnIfNeeded() {
        guard let delay = autoReturnDelaySeconds else {
            return
        }

        autoReturnDelaySeconds = nil
        let nanoseconds = UInt64(max(0.1, delay) * 1_000_000_000)
        Task { @MainActor in
            try? await Task.sleep(nanoseconds: nanoseconds)
            returnFrame()
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

@MainActor
final class GuestGlassOverlayController {
    private let state = GuestGlassOverlayState()
    private var window: NSPanel?

    func show(edge: GuestPortalEdge) {
        state.edge = edge
        state.isVisible = true
        if window == nil {
            createWindow()
        }
        window?.orderFrontRegardless()
    }

    func hide() {
        state.isVisible = false
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
    @Published var isVisible = false
    @Published var edge: GuestPortalEdge = .left
}

struct GuestGlassOverlayView: View {
    @ObservedObject var state: GuestGlassOverlayState

    var body: some View {
        GeometryReader { geometry in
            ZStack {
                if state.isVisible {
                    let frame = edgeFrame(in: geometry.size)
                    GuestProgressiveGlassEdge(edge: state.edge)
                        .frame(width: frame.width, height: frame.height)
                        .position(x: frame.midX, y: frame.midY)
                        .transition(.opacity)
                }
            }
            .frame(width: geometry.size.width, height: geometry.size.height)
        }
        .allowsHitTesting(false)
    }

    private func edgeFrame(in size: CGSize) -> CGRect {
        let depth: CGFloat = 220
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

    var body: some View {
        GeometryReader { geometry in
            ZStack {
                Rectangle()
                    .fill(.regularMaterial)
                    .opacity(0.96)
                    .mask(progressiveMask)

                Rectangle()
                    .fill(bodyGradient)
                    .mask(progressiveMask)

                Rectangle()
                    .fill(lightGradient)
                    .blur(radius: 18)
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
            return LinearGradient(colors: [.clear, .white.opacity(0.05), .cyan.opacity(0.12), .white.opacity(0.25)], startPoint: .leading, endPoint: .trailing)
        case .left:
            return LinearGradient(colors: [.white.opacity(0.25), .cyan.opacity(0.12), .white.opacity(0.05), .clear], startPoint: .leading, endPoint: .trailing)
        case .top:
            return LinearGradient(colors: [.white.opacity(0.25), .cyan.opacity(0.12), .white.opacity(0.05), .clear], startPoint: .top, endPoint: .bottom)
        case .bottom:
            return LinearGradient(colors: [.clear, .white.opacity(0.05), .cyan.opacity(0.12), .white.opacity(0.25)], startPoint: .top, endPoint: .bottom)
        }
    }

    private var lightGradient: LinearGradient {
        switch edge {
        case .left, .right:
            return LinearGradient(colors: [.clear, .white.opacity(0.35), .cyan.opacity(0.22), .white.opacity(0.35), .clear], startPoint: .top, endPoint: .bottom)
        case .top, .bottom:
            return LinearGradient(colors: [.clear, .white.opacity(0.35), .cyan.opacity(0.22), .white.opacity(0.35), .clear], startPoint: .leading, endPoint: .trailing)
        }
    }

    @ViewBuilder
    private func throat(in size: CGSize) -> some View {
        if edge == .left || edge == .right {
            Capsule()
                .fill(lightGradient)
                .frame(width: 12, height: min(size.height * 0.36, 320))
                .position(x: edge == .left ? 18 : size.width - 18, y: size.height / 2)
                .shadow(color: Color.cyan.opacity(0.34), radius: 18)
        } else {
            Capsule()
                .fill(lightGradient)
                .frame(width: min(size.width * 0.36, 360), height: 12)
                .position(x: size.width / 2, y: edge == .top ? 18 : size.height - 18)
                .shadow(color: Color.cyan.opacity(0.34), radius: 18)
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
        decoder.dateDecodingStrategy = .iso8601
        return try decoder.decode(RkwpMessage.self, from: response)
    }

    private func openSocket() throws -> Int32 {
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
                Darwin.close(fd)
            }
            candidate = info.ai_next
        }

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
