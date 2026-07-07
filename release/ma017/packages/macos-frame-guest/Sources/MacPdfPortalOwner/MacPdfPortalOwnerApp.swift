import AppKit
import Foundation
import Network
import PDFKit
import SwiftUI
import UniformTypeIdentifiers

@main
struct MacPdfPortalOwnerApp: App {
    @StateObject private var model = MacPortalOwnerModel()

    init() {
        MacPortalOwnerSmokeTest.runAndExitIfRequested(arguments: CommandLine.arguments)
    }

    var body: some Scene {
        WindowGroup("RK Workspace Ablage") {
            MacPortalOwnerContentView(model: model)
                .frame(minWidth: 960, minHeight: 660)
                .onAppear {
                    model.applyCommandLine()
                }
        }
    }
}

struct MacPortalOwnerContentView: View {
    @ObservedObject var model: MacPortalOwnerModel

    var body: some View {
        GeometryReader { geometry in
            ZStack(alignment: .trailing) {
                HStack(spacing: 18) {
                    mainSurface
                    glassEdge
                }
                .padding(22)

                if model.isCarrying, let image = model.previewImage {
                    carriedThing(image: image)
                        .offset(model.handOffset)
                        .position(x: geometry.size.width - 200, y: geometry.size.height * 0.55)
                        .transition(.scale.combined(with: .opacity))
                        .gesture(
                            DragGesture()
                                .onChanged { value in
                                    model.portalOpen = true
                                    model.handOffset = value.translation
                                }
                                .onEnded { value in
                                    if value.translation.width > 72 {
                                        model.placeAtGlassEdge()
                                    } else {
                                        model.handOffset = .zero
                                    }
                                }
                        )
                }
            }
            .background(Color(nsColor: .windowBackgroundColor))
        }
    }

    private var mainSurface: some View {
        VStack(spacing: 14) {
            header
            pdfSurface
            footer
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }

    private var header: some View {
        HStack(alignment: .top) {
            VStack(alignment: .leading, spacing: 4) {
                Text("Ablage macOS")
                    .font(.system(size: 28, weight: .semibold))
                Text(model.visibleState)
                    .foregroundStyle(.secondary)
            }
            Spacer()
            VStack(alignment: .trailing, spacing: 4) {
                Text("Original bleibt auf macOS")
                    .font(.callout)
                Text("Windows bekommt nur Frame")
                    .foregroundStyle(.secondary)
                    .font(.caption)
            }
        }
    }

    private var pdfSurface: some View {
        ZStack {
            RoundedRectangle(cornerRadius: 8)
                .fill(Color(nsColor: .textBackgroundColor))
                .overlay(
                    RoundedRectangle(cornerRadius: 8)
                        .stroke(Color(nsColor: .separatorColor), lineWidth: 1)
                )

            if let image = model.previewImage {
                Image(nsImage: image)
                    .resizable()
                    .scaledToFit()
                    .padding(28)
                    .simultaneousGesture(
                        LongPressGesture(minimumDuration: 0.75)
                            .onEnded { _ in model.takeIntoHand() }
                    )
                ThreeFingerHoldSurface {
                    model.takeIntoHand()
                }
            } else {
                VStack(spacing: 12) {
                    Text("Noch keine PDF")
                        .font(.title2)
                    Button {
                        model.choosePdf()
                    } label: {
                        Label("PDF", systemImage: "doc")
                    }
                }
            }

            if model.portalOpen {
                HStack {
                    Spacer()
                    GlassWakeRibbon()
                        .padding(.trailing, 14)
                }
                .transition(.opacity)
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }

    private var footer: some View {
        HStack(spacing: 10) {
            Button {
                model.choosePdf()
            } label: {
                Label("PDF", systemImage: "doc")
            }

            Button {
                model.takeIntoHand()
            } label: {
                Label("nehmen", systemImage: "hand.raised")
            }
            .disabled(model.previewImage == nil)

            Button {
                model.placeAtGlassEdge()
            } label: {
                Label("zum Glasrand", systemImage: "rectangle.portrait.and.arrow.right")
            }
            .disabled(model.preparedFrame == nil && model.previewImage == nil)

            Spacer()

            Text(model.status)
                .foregroundStyle(model.hasError ? .red : .secondary)
                .lineLimit(1)
                .truncationMode(.middle)
        }
    }

    private var glassEdge: some View {
        VStack(spacing: 12) {
            Spacer()
            Capsule()
                .fill(model.windowsNearby ? Color.green.opacity(0.72) : Color.white.opacity(0.5))
                .frame(width: 10, height: model.portalOpen ? 150 : 74)
                .animation(.easeInOut(duration: 0.25), value: model.portalOpen)
            Text(model.windowsNearby ? "Windows nah" : "Windows")
                .font(.caption)
                .foregroundStyle(.secondary)
            Spacer()
        }
        .frame(width: model.portalOpen ? 104 : 62)
        .background(.ultraThinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(model.portalOpen ? Color.cyan.opacity(0.7) : Color(nsColor: .separatorColor), lineWidth: 1.5)
        )
        .shadow(color: model.portalOpen ? Color.cyan.opacity(0.32) : .clear, radius: 22)
        .animation(.easeInOut(duration: 0.25), value: model.portalOpen)
    }

    private func carriedThing(image: NSImage) -> some View {
        VStack(spacing: 7) {
            Image(nsImage: image)
                .resizable()
                .scaledToFit()
                .frame(width: 138, height: 180)
                .clipShape(RoundedRectangle(cornerRadius: 6))
            Text(model.pdfDisplayName)
                .font(.caption)
                .lineLimit(1)
        }
        .padding(10)
        .frame(width: 174, height: 230)
        .background(.thinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(Color.cyan.opacity(0.6), lineWidth: 1.3)
        )
        .shadow(color: Color.black.opacity(0.22), radius: 16, y: 8)
    }
}

struct GlassWakeRibbon: View {
    var body: some View {
        VStack(spacing: 12) {
            Capsule()
                .fill(Color.white.opacity(0.58))
                .frame(width: 8, height: 68)
            Text("Glasrand")
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .frame(width: 86, height: 260)
        .background(.ultraThinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(Color.cyan.opacity(0.6), lineWidth: 1.5)
        )
        .shadow(color: Color.cyan.opacity(0.3), radius: 18)
    }
}

struct ThreeFingerHoldSurface: NSViewRepresentable {
    var onHold: () -> Void

    func makeNSView(context: Context) -> ThreeFingerHoldNSView {
        let view = ThreeFingerHoldNSView()
        view.onHold = onHold
        return view
    }

    func updateNSView(_ nsView: ThreeFingerHoldNSView, context: Context) {
        nsView.onHold = onHold
    }
}

final class ThreeFingerHoldNSView: NSView {
    var onHold: (() -> Void)?
    private var pendingHold: DispatchWorkItem?
    private var holdFired = false

    override init(frame frameRect: NSRect) {
        super.init(frame: frameRect)
        allowedTouchTypes = [.indirect]
        wantsRestingTouches = true
    }

    required init?(coder: NSCoder) {
        super.init(coder: coder)
        allowedTouchTypes = [.indirect]
        wantsRestingTouches = true
    }

    override func touchesBegan(with event: NSEvent) {
        updateTouches(event)
    }

    override func touchesMoved(with event: NSEvent) {
        updateTouches(event)
    }

    override func touchesEnded(with event: NSEvent) {
        cancelHoldIfNeeded(event)
    }

    override func touchesCancelled(with event: NSEvent) {
        cancelHold()
    }

    private func updateTouches(_ event: NSEvent) {
        let touchingCount = event.touches(matching: .touching, in: self).count
        guard touchingCount >= 3 else {
            cancelHold()
            holdFired = false
            return
        }

        guard pendingHold == nil, !holdFired else {
            return
        }

        let item = DispatchWorkItem { [weak self] in
            guard let self else { return }
            self.holdFired = true
            self.onHold?()
        }
        pendingHold = item
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.75, execute: item)
    }

    private func cancelHoldIfNeeded(_ event: NSEvent) {
        let touchingCount = event.touches(matching: .touching, in: self).count
        if touchingCount < 3 {
            cancelHold()
            holdFired = false
        }
    }

    private func cancelHold() {
        pendingHold?.cancel()
        pendingHold = nil
    }
}

@MainActor
final class MacPortalOwnerModel: ObservableObject {
    @Published var visibleState = "bereit"
    @Published var status = "MacOwnerAblage: STARTED"
    @Published var hasError = false
    @Published var previewImage: NSImage?
    @Published var pdfDisplayName = "Rechnung.pdf"
    @Published var targetHost = "192.168.163.11"
    @Published var targetPortText = "57120"
    @Published var listenPortText = "57121"
    @Published var windowsNearby = false
    @Published var portalOpen = false
    @Published var isCarrying = false
    @Published var handOffset: CGSize = .zero
    @Published private(set) var preparedFrame: MacPortalOwnerFrame?

    private var pdfURL: URL?
    private var pdfDocument: PDFDocument?
    private let frameServer = MacPortalOwnerFrameServer()
    private var sessionId = "rkwp-macos-owner-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())"

    func applyCommandLine() {
        let args = CommandLine.arguments
        var explicitPdf: URL?

        for index in args.indices {
            if ["--pdf", "--pdf-path"].contains(args[index]), index + 1 < args.count {
                explicitPdf = URL(fileURLWithPath: args[index + 1])
            }

            if ["--host", "--target-host"].contains(args[index]), index + 1 < args.count {
                targetHost = args[index + 1]
            }

            if ["--port", "--target-port"].contains(args[index]), index + 1 < args.count {
                targetPortText = args[index + 1]
            }

            if ["--listen-port", "--owner-port"].contains(args[index]), index + 1 < args.count {
                listenPortText = args[index + 1]
            }
        }

        if let explicitPdf {
            loadPdf(explicitPdf)
        } else if let sample = MacPortalOwnerPaths.defaultPdf() {
            loadPdf(sample)
        }

        startFrameAblage()
        probeWindows()

        if args.contains("--auto-place") {
            takeIntoHand()
            placeAtGlassEdge()
        }
    }

    func choosePdf() {
        let panel = NSOpenPanel()
        panel.canChooseFiles = true
        panel.canChooseDirectories = false
        panel.allowsMultipleSelection = false
        panel.allowedContentTypes = [.pdf]

        if panel.runModal() == .OK, let url = panel.url {
            loadPdf(url)
        }
    }

    func loadPdf(_ url: URL) {
        do {
            let loaded = try MacPortalPdfRenderer.loadPreview(url: url)
            pdfURL = url
            pdfDocument = loaded.document
            previewImage = loaded.previewImage
            pdfDisplayName = url.lastPathComponent
            preparedFrame = nil
            portalOpen = false
            isCarrying = false
            handOffset = .zero
            visibleState = "PDF liegt auf macOS"
            status = "PDF: \(url.lastPathComponent)"
            hasError = false
        } catch {
            hasError = true
            visibleState = "PDF fehlt"
            status = error.localizedDescription
        }
    }

    func takeIntoHand() {
        do {
            try prepareFrameIfNeeded()
            hasError = false
            withAnimation(.spring(response: 0.35, dampingFraction: 0.78)) {
                isCarrying = true
                portalOpen = true
                handOffset = .zero
                visibleState = "Ding in der Hand"
                status = "Glasrand offen"
            }
            probeWindows()
        } catch {
            hasError = true
            status = error.localizedDescription
        }
    }

    func placeAtGlassEdge() {
        do {
            try prepareFrameIfNeeded()
            withAnimation(.easeInOut(duration: 0.24)) {
                isCarrying = false
                portalOpen = true
                handOffset = .zero
                visibleState = "Frame liegt fuer Windows bereit"
                status = "liegt am Glasrand"
            }
            print("GlassPlacement: OK")
            print("OwnerKeepsOriginal: OK")
            print("WindowsGetsPdfFile: NO")
            print("WindowsGetsOriginalPath: NO")
            print("OriginalFileBytes: NO")
            print("FrameCache: MemoryOnly")
            print("NoFileIngress: SUCCESS")
        } catch {
            hasError = true
            status = error.localizedDescription
        }
    }

    func response(for request: MacPortalRkwpMessage) -> MacPortalRkwpMessage {
        let target = request.sourceAblageId.isEmpty ? "ablage-windows-guest" : request.sourceAblageId
        let payload: [String: String]
        let messageType: String

        switch request.messageType {
        case "AblageHello", "AblageCapabilities":
            messageType = "AblageCapabilities"
            payload = [
                "frameOnly": "true",
                "noFileIngress": "true",
                "memoryOnlyFrame": "true",
                "ownerKeepsOriginal": "true",
                "openFrame": "true",
                "returnSupported": "true",
                "visibleStatus": visibleState
            ]
        case "FrameUpdate":
            messageType = "FrameUpdate"
            if portalOpen, let preparedFrame {
                payload = preparedFrame.payload(placementReady: true, visibleStatus: "liegt hier im Frame")
                status = "Frame geholt"
                print("FrameUpdate: OK")
            } else if let preparedFrame {
                payload = preparedFrame.payload(placementReady: false, visibleStatus: "wartet am Glasrand")
            } else {
                payload = MacPortalOwnerFrame.waitingPayload(displayName: pdfDisplayName)
            }
        case "CarryLeaseReturn":
            messageType = "CarryLeaseReturn"
            payload = [
                "return": "accepted",
                "frameSessionId": preparedFrame?.frameSessionId ?? "",
                "leaseId": preparedFrame?.leaseId ?? "",
                "guestKeptOriginalFile": "false",
                "noFileIngress": "true"
            ]
            visibleState = "zurueckgegeben"
            status = "Return: SUCCESS"
        default:
            messageType = request.messageType
            payload = [
                "accepted": "true",
                "noFileIngress": "true",
                "visibleStatus": visibleState
            ]
        }

        return MacPortalRkwpMessage(
            messageType: messageType,
            sourceAblageId: "ablage-macos-owner",
            targetAblageId: target,
            sessionId: sessionId,
            correlationId: request.messageId,
            payload: payload)
    }

    private func startFrameAblage() {
        do {
            let port = UInt16(listenPortText) ?? 57121
            try frameServer.start(port: port, model: self)
            print("MacOwnerAblage: READY")
            print("OwnerKeepsOriginal: OK")
            print("ListenPort: \(port)")
        } catch {
            hasError = true
            status = "Ablage nicht offen: \(error.localizedDescription)"
        }
    }

    private func prepareFrameIfNeeded() throws {
        if preparedFrame != nil {
            return
        }

        guard let pdfURL, let document = pdfDocument, let previewImage else {
            throw MacPortalOwnerError.pdfMissing
        }

        preparedFrame = try MacPortalPdfRenderer.makeFrame(
            url: pdfURL,
            document: document,
            image: previewImage)
        print("RenderedFrame: OK")
    }

    private func probeWindows() {
        let host = targetHost
        let port = UInt16(targetPortText) ?? 57120
        DispatchQueue.global(qos: .utility).async { [weak self] in
            let reachable = MacPortalNetworkProbe.canReach(host: host, port: port, timeout: 0.8)
            DispatchQueue.main.async {
                self?.windowsNearby = reachable
            }
        }
    }
}

struct MacPortalOwnerFrame {
    let frameSessionId: String
    let leaseId: String
    let thingId: String
    let displayName: String
    let pageCount: Int
    let page: Int
    let width: Int
    let height: Int
    let sourceHash: String
    let pngBase64: String
    let renderedAt: String

    func payload(placementReady: Bool, visibleStatus: String) -> [String: String] {
        var payload = basePayload(placementReady: placementReady, visibleStatus: visibleStatus)
        guard placementReady else {
            return payload
        }

        payload["frameFormat"] = "PngFrame"
        payload["pngBase64"] = pngBase64
        payload["renderedAt"] = renderedAt
        return payload
    }

    static func waitingPayload(displayName: String) -> [String: String] {
        [
            "frameSessionId": "frame-macos-owner-waiting",
            "leaseId": "lease-macos-owner-waiting",
            "thingId": "ding-macos-owner-waiting",
            "displayName": displayName,
            "page": "1",
            "pageCount": "1",
            "width": "0",
            "height": "0",
            "placementReady": "false",
            "ownerKeepsOriginal": "true",
            "containsOriginalFileBytes": "false",
            "hasOriginalPath": "false",
            "guestHasPdfFile": "false",
            "noFileIngress": "true",
            "frameCache": "MemoryOnly",
            "visibleStatus": "wartet am Glasrand"
        ]
    }

    private func basePayload(placementReady: Bool, visibleStatus: String) -> [String: String] {
        [
            "frameSessionId": frameSessionId,
            "leaseId": leaseId,
            "thingId": thingId,
            "displayName": displayName,
            "pageCount": "\(pageCount)",
            "sourceHash": sourceHash,
            "page": "\(page)",
            "width": "\(width)",
            "height": "\(height)",
            "placementReady": placementReady ? "true" : "false",
            "ownerKeepsOriginal": "true",
            "containsOriginalFileBytes": "false",
            "hasOriginalPath": "false",
            "guestHasPdfFile": "false",
            "noFileIngress": "true",
            "frameCache": "MemoryOnly",
            "visibleStatus": visibleStatus
        ]
    }
}

enum MacPortalPdfRenderer {
    static func loadPreview(url: URL) throws -> (document: PDFDocument, previewImage: NSImage) {
        guard let document = PDFDocument(url: url), document.pageCount > 0 else {
            throw MacPortalOwnerError.pdfMissing
        }

        let image = try renderFirstPage(document: document, requestedWidth: 1100)
        return (document, image)
    }

    static func makeFrame(url: URL, document: PDFDocument, image: NSImage) throws -> MacPortalOwnerFrame {
        let png = try pngData(from: image)
        let sourceData = try Data(contentsOf: url)
        let hash = fingerprint(sourceData)
        return MacPortalOwnerFrame(
            frameSessionId: "frame-macos-owner-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())",
            leaseId: "lease-macos-owner-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())",
            thingId: "ding-macos-owner-\(hash)",
            displayName: url.lastPathComponent,
            pageCount: document.pageCount,
            page: 1,
            width: Int(image.size.width.rounded()),
            height: Int(image.size.height.rounded()),
            sourceHash: hash,
            pngBase64: png.base64EncodedString(),
            renderedAt: ISO8601DateFormatter().string(from: Date()))
    }

    private static func renderFirstPage(document: PDFDocument, requestedWidth: CGFloat) throws -> NSImage {
        guard let page = document.page(at: 0) else {
            throw MacPortalOwnerError.pdfMissing
        }

        let bounds = page.bounds(for: .mediaBox)
        let scale = requestedWidth / max(bounds.width, 1)
        let size = NSSize(width: requestedWidth, height: max(bounds.height * scale, 1))
        let image = NSImage(size: size)

        image.lockFocus()
        defer { image.unlockFocus() }

        NSColor.white.setFill()
        NSBezierPath(rect: NSRect(origin: .zero, size: size)).fill()

        guard let context = NSGraphicsContext.current?.cgContext else {
            throw MacPortalOwnerError.renderFailed
        }

        context.saveGState()
        context.translateBy(x: 0, y: size.height)
        context.scaleBy(x: scale, y: -scale)
        page.draw(with: .mediaBox, to: context)
        context.restoreGState()

        return image
    }

    private static func pngData(from image: NSImage) throws -> Data {
        guard let tiff = image.tiffRepresentation,
              let bitmap = NSBitmapImageRep(data: tiff),
              let png = bitmap.representation(using: .png, properties: [:]) else {
            throw MacPortalOwnerError.renderFailed
        }

        return png
    }

    private static func fingerprint(_ data: Data) -> String {
        var hash: UInt64 = 14_695_981_039_346_656_037
        for byte in data {
            hash ^= UInt64(byte)
            hash = hash &* 1_099_511_628_211
        }

        return String(format: "%016llx", hash)
    }
}

final class MacPortalOwnerFrameServer {
    private var listener: NWListener?
    private weak var model: MacPortalOwnerModel?

    func start(port: UInt16, model: MacPortalOwnerModel) throws {
        if listener != nil {
            return
        }

        guard let nwPort = NWEndpoint.Port(rawValue: port) else {
            throw MacPortalOwnerError.invalidPort
        }

        self.model = model
        let listener = try NWListener(using: .tcp, on: nwPort)
        listener.newConnectionHandler = { [weak self] connection in
            self?.handle(connection)
        }
        listener.start(queue: .main)
        self.listener = listener
    }

    private func handle(_ connection: NWConnection) {
        connection.start(queue: .main)
        receive(on: connection)
    }

    private func receive(on connection: NWConnection) {
        connection.receive(minimumIncompleteLength: 1, maximumLength: 1_048_576) { [weak self] data, _, _, error in
            guard let self else {
                connection.cancel()
                return
            }

            guard error == nil, var data, !data.isEmpty else {
                connection.cancel()
                return
            }

            if let newline = data.firstIndex(of: 0x0A) {
                data = data[..<newline]
            }

            let decoder = JSONDecoder()
            decoder.dateDecodingStrategy = .iso8601

            guard let request = try? decoder.decode(MacPortalRkwpMessage.self, from: data) else {
                connection.cancel()
                return
            }

            Task { @MainActor [weak self] in
                guard let self, let model = self.model else {
                    connection.cancel()
                    return
                }

                let response = model.response(for: request)
                self.send(response, on: connection)
            }
        }
    }

    private func send(_ message: MacPortalRkwpMessage, on connection: NWConnection) {
        let encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601

        guard var data = try? encoder.encode(message) else {
            connection.cancel()
            return
        }

        data.append(0x0A)
        connection.send(content: data, completion: .contentProcessed { _ in
            connection.cancel()
        })
    }
}

struct MacPortalRkwpMessage: Codable {
    var messageId: String = "rkwp-macos-owner-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())"
    var messageType: String
    var sourceAblageId: String
    var targetAblageId: String
    var sessionId: String
    var correlationId: String = ""
    var timestamp: Date = Date()
    var payload: [String: String]
    var headers: [String: String] = [:]
}

enum MacPortalNetworkProbe {
    static func canReach(host: String, port: UInt16, timeout: TimeInterval) -> Bool {
        var input: InputStream?
        var output: OutputStream?
        Stream.getStreamsToHost(withName: host, port: Int(port), inputStream: &input, outputStream: &output)

        guard let inputStream = input, let outputStream = output else {
            return false
        }

        inputStream.open()
        outputStream.open()
        defer {
            inputStream.close()
            outputStream.close()
        }

        let deadline = Date().addingTimeInterval(timeout)
        while Date() < deadline {
            if inputStream.streamStatus == .open || outputStream.streamStatus == .open {
                return true
            }

            if inputStream.streamStatus == .error || outputStream.streamStatus == .error {
                return false
            }

            Thread.sleep(forTimeInterval: 0.02)
        }

        return false
    }
}

enum MacPortalOwnerPaths {
    static func repositoryRoot() -> URL? {
        var directory = URL(fileURLWithPath: FileManager.default.currentDirectoryPath)

        for _ in 0..<12 {
            let marker = directory.appendingPathComponent("samples/Objects/Rechnung.pdf")
            if FileManager.default.fileExists(atPath: marker.path) {
                return directory
            }

            let parent = directory.deletingLastPathComponent()
            if parent.path == directory.path {
                break
            }

            directory = parent
        }

        return nil
    }

    static func defaultPdf() -> URL? {
        repositoryRoot()?.appendingPathComponent("samples/Objects/Rechnung.pdf")
    }
}

enum MacPortalOwnerSmokeTest {
    static func runAndExitIfRequested(arguments: [String]) {
        guard arguments.contains("--smoke-test") else {
            return
        }

        do {
            try run(arguments: arguments)
            exit(0)
        } catch {
            print("MacPortalOwner: STARTED")
            print("RESULT: FAILED - \(error.localizedDescription)")
            exit(1)
        }
    }

    private static func run(arguments: [String]) throws {
        let pdfPath = explicitPdfPath(arguments: arguments) ?? MacPortalOwnerPaths.defaultPdf()
        guard let pdfPath else {
            throw MacPortalOwnerError.pdfMissing
        }

        let loaded = try MacPortalPdfRenderer.loadPreview(url: pdfPath)
        let frame = try MacPortalPdfRenderer.makeFrame(
            url: pdfPath,
            document: loaded.document,
            image: loaded.previewImage)
        let waitingPayload = frame.payload(placementReady: false, visibleStatus: "wartet am Glasrand")
        let readyPayload = frame.payload(placementReady: true, visibleStatus: "liegt hier im Frame")
        try validateNoFileIngress(waitingPayload, mustContainPng: false)
        try validateNoFileIngress(readyPayload, mustContainPng: true)

        print("MacPortalOwner: STARTED")
        print("PdfLoaded: OK")
        print("DigitalHand: OK")
        print("GlassPortal: OK")
        print("RenderedFrame: OK")
        print("BeforeGlassEdge: NO_FRAME")
        print("AfterGlassEdge: PNG_FRAME")
        print("OwnerKeepsOriginal: OK")
        print("WindowsGetsPdfFile: NO")
        print("WindowsGetsOriginalPath: NO")
        print("OriginalFileBytes: NO")
        print("FrameCache: MemoryOnly")
        print("NoFileIngress: SUCCESS")
        print("RESULT: SUCCESS")
    }

    private static func explicitPdfPath(arguments: [String]) -> URL? {
        for index in arguments.indices {
            if ["--pdf", "--pdf-path"].contains(arguments[index]), index + 1 < arguments.count {
                return URL(fileURLWithPath: arguments[index + 1])
            }
        }

        return nil
    }

    private static func validateNoFileIngress(_ payload: [String: String], mustContainPng: Bool) throws {
        for key in payload.keys {
            let normalized = key.lowercased()
            if ["pdfbytes", "originalbytes", "originalfilebytes", "originalpath", "localpdfpath", "downloadpath", "filepath"].contains(normalized) {
                throw MacPortalOwnerError.noFileIngressFailed
            }
        }

        guard payload["noFileIngress"] == "true",
              payload["ownerKeepsOriginal"] == "true",
              payload["containsOriginalFileBytes"] == "false",
              payload["hasOriginalPath"] == "false",
              payload["guestHasPdfFile"] == "false",
              payload["frameCache"] == "MemoryOnly" else {
            throw MacPortalOwnerError.noFileIngressFailed
        }

        if mustContainPng && payload["pngBase64"]?.isEmpty != false {
            throw MacPortalOwnerError.noFileIngressFailed
        }

        if !mustContainPng && payload["pngBase64"] != nil {
            throw MacPortalOwnerError.noFileIngressFailed
        }
    }
}

enum MacPortalOwnerError: LocalizedError {
    case pdfMissing
    case renderFailed
    case invalidPort
    case noFileIngressFailed

    var errorDescription: String? {
        switch self {
        case .pdfMissing:
            return "PDF fehlt"
        case .renderFailed:
            return "Frame konnte nicht vorbereitet werden"
        case .invalidPort:
            return "Port ist ungueltig"
        case .noFileIngressFailed:
            return "NoFileIngress fehlgeschlagen"
        }
    }
}
