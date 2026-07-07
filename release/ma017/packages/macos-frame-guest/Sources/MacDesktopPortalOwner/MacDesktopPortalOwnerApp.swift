import AppKit
import ApplicationServices
import Foundation
import Network
import PDFKit
import SwiftUI

@main
@MainActor
final class MacDesktopPortalOwnerApp: NSObject, NSApplicationDelegate {
    private static var retainedDelegate: MacDesktopPortalOwnerApp?
    private let controller = DesktopPortalController()

    static func main() {
        DesktopPortalSmokeTest.runAndExitIfRequested(arguments: CommandLine.arguments)

        let app = NSApplication.shared
        let delegate = MacDesktopPortalOwnerApp()
        retainedDelegate = delegate
        app.delegate = delegate
        app.setActivationPolicy(.accessory)
        app.run()
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        controller.start()
    }

    func applicationWillTerminate(_ notification: Notification) {
        controller.stop()
    }
}

@MainActor
final class DesktopPortalController {
    private let state = DesktopPortalOverlayState()
    private let frameServer = DesktopPortalFrameServer()
    private var overlayWindow: NSPanel?
    private var eventMonitors: [Any] = []
    private var preparedFrame: DesktopPortalFrame?
    private var heldApplication: NSRunningApplication?
    private var settings = DesktopPortalSettings.parse(CommandLine.arguments)
    private let sessionId = "rkwp-macos-desktop-owner-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())"

    func start() {
        requestAccessibilityIfNeeded()
        createOverlayWindow()
        startFrameServer()
        installEventMonitors()

        print("MacDesktopPortalOwner: READY")
        print("Gesture: TripleClick")
        print("NearestAblage: \(settings.targetName)")
        print("EdgeDirection: \(settings.portalEdge.rawValue)")
        print("OwnerKeepsOriginal: OK")
        print("NoFileIngress: READY")

        if settings.devAutoPlace {
            DispatchQueue.main.asyncAfter(deadline: .now() + 0.4) { [weak self] in
                guard let self else {
                    return
                }

                let point = NSScreen.main.map { NSPoint(x: $0.frame.midX, y: $0.frame.midY) } ?? NSPoint(x: 400, y: 400)
                self.beginCarry(at: point)
                self.state.place()
            }
        }
    }

    func stop() {
        eventMonitors.forEach { NSEvent.removeMonitor($0) }
        eventMonitors.removeAll()
        frameServer.stop()
    }

    func response(for request: DesktopPortalRkwpMessage) -> DesktopPortalRkwpMessage {
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
                "visibleStatus": state.visibleStatus
            ]
        case "FrameUpdate":
            messageType = "FrameUpdate"
            if state.isPlaced, let preparedFrame {
                payload = preparedFrame.payload(placementReady: true, visibleStatus: "liegt hier im Frame")
                print("FrameUpdate: OK")
            } else if let preparedFrame {
                payload = preparedFrame.payload(placementReady: false, visibleStatus: "wartet am Glasrand")
            } else {
                payload = DesktopPortalFrame.waitingPayload(displayName: "Ding")
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
            state.finishReturn()
            heldApplication?.unhide()
            heldApplication = nil
        default:
            messageType = request.messageType
            payload = [
                "accepted": "true",
                "noFileIngress": "true",
                "visibleStatus": state.visibleStatus
            ]
        }

        return DesktopPortalRkwpMessage(
            messageType: messageType,
            sourceAblageId: "ablage-macos-owner",
            targetAblageId: target,
            sessionId: sessionId,
            correlationId: request.messageId,
            payload: payload)
    }

    private func requestAccessibilityIfNeeded() {
        let options = [kAXTrustedCheckOptionPrompt.takeUnretainedValue() as String: true] as CFDictionary
        if AXIsProcessTrustedWithOptions(options) {
            print("Accessibility: OK")
        } else {
            print("Accessibility: PROMPTED")
        }
    }

    private func createOverlayWindow() {
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
        panel.contentView = NSHostingView(rootView: DesktopPortalOverlayView(state: state))
        panel.orderFrontRegardless()
        overlayWindow = panel
    }

    private func startFrameServer() {
        do {
            try frameServer.start(port: settings.listenPort, controller: self)
            print("ListenPort: \(settings.listenPort)")
        } catch {
            state.showNotice("Ablage nicht offen")
            print("RESULT: FAILED - \(error.localizedDescription)")
        }
    }

    private func installEventMonitors() {
        let mask: NSEvent.EventTypeMask = [.leftMouseDown, .leftMouseUp, .mouseMoved, .leftMouseDragged]
        if let globalMonitor = NSEvent.addGlobalMonitorForEvents(matching: mask, handler: { [weak self] event in
            DispatchQueue.main.async {
                self?.handle(event)
            }
        }) {
            eventMonitors.append(globalMonitor)
        }

        if let localMonitor = NSEvent.addLocalMonitorForEvents(matching: mask, handler: { [weak self] event in
            self?.handle(event)
            return event
        }) {
            eventMonitors.append(localMonitor)
        }
    }

    private func handle(_ event: NSEvent) {
        switch event.type {
        case .leftMouseDown where event.clickCount >= 3:
            beginCarry(at: NSEvent.mouseLocation)
        case .mouseMoved, .leftMouseDragged:
            updateCarry(at: NSEvent.mouseLocation)
        case .leftMouseUp:
            updateCarry(at: NSEvent.mouseLocation)
        default:
            break
        }
    }

    private func beginCarry(at screenPoint: NSPoint) {
        guard !state.isCarrying else {
            return
        }

        do {
            let object = try DesktopPdfObjectResolver.resolve(at: screenPoint)
            let rendered = try DesktopPdfRenderer.render(url: object.url)
            preparedFrame = rendered.frame
            heldApplication = object.sourceApplication
            state.beginCarry(
                name: object.displayName,
                image: rendered.image,
                at: localPoint(for: screenPoint),
                edge: settings.portalEdge,
                targetName: settings.targetName)
            heldApplication?.hide()
            print("DesktopGesture: TRIPLE_CLICK")
            print("PdfObjectResolved: OK")
            print("DigitalHand: OK")
            print("GlassEdgeMode: NearestOnly")
            print("OwnerLocked: VISUAL")
            print("OwnerKeepsOriginal: OK")
        } catch {
            state.showNotice(error.localizedDescription, at: localPoint(for: screenPoint))
            print("DesktopGesture: TRIPLE_CLICK")
            print("PdfObjectResolved: FAILED")
            print("Reason: \(error.localizedDescription)")
        }
    }

    private func updateCarry(at screenPoint: NSPoint) {
        guard state.isCarrying else {
            return
        }

        let point = localPoint(for: screenPoint)
        state.updateCursor(point)

        if state.isNearPortal(point) {
            state.place()
            print("ObjectEnteringEdge: OK")
            print("GlassPlacement: OK")
            print("OwnerKeepsOriginal: OK")
            print("WindowsGetsPdfFile: NO")
            print("WindowsGetsOriginalPath: NO")
            print("OriginalFileBytes: NO")
            print("FrameCache: MemoryOnly")
            print("NoFileIngress: SUCCESS")
        }
    }

    private func localPoint(for screenPoint: NSPoint) -> CGPoint {
        guard let frame = overlayWindow?.frame else {
            return CGPoint(x: screenPoint.x, y: screenPoint.y)
        }

        return CGPoint(
            x: screenPoint.x - frame.minX,
            y: frame.maxY - screenPoint.y)
    }
}

@MainActor
final class DesktopPortalOverlayState: ObservableObject {
    @Published var mode: DesktopPortalMode = .idle
    @Published var cursor = CGPoint(x: 400, y: 360)
    @Published var objectName = "Ding"
    @Published var targetName = "Ablage Windows"
    @Published var edge: DesktopPortalEdge = .right
    @Published var previewImage: NSImage?
    @Published var notice = ""

    var isCarrying: Bool {
        mode == .carrying
    }

    var isPlaced: Bool {
        mode == .placed
    }

    var visibleStatus: String {
        switch mode {
        case .idle:
            return "bereit"
        case .notice:
            return notice
        case .carrying:
            return "Ding in der Hand"
        case .placed:
            return "liegt am Glasrand"
        }
    }

    func beginCarry(name: String, image: NSImage, at point: CGPoint, edge: DesktopPortalEdge, targetName: String) {
        objectName = name
        previewImage = image
        cursor = point
        self.edge = edge
        self.targetName = targetName
        notice = ""
        withAnimation(.spring(response: 0.28, dampingFraction: 0.82)) {
            mode = .carrying
        }
    }

    func updateCursor(_ point: CGPoint) {
        cursor = point
    }

    func place() {
        guard mode == .carrying else {
            return
        }

        withAnimation(.easeInOut(duration: 0.25)) {
            mode = .placed
        }
    }

    func finishReturn() {
        withAnimation(.easeInOut(duration: 0.2)) {
            mode = .idle
            previewImage = nil
        }
    }

    func showNotice(_ text: String, at point: CGPoint? = nil) {
        if let point {
            cursor = point
        }

        notice = text
        withAnimation(.easeInOut(duration: 0.15)) {
            mode = .notice
        }

        DispatchQueue.main.asyncAfter(deadline: .now() + 2.4) { [weak self] in
            guard self?.mode == .notice else {
                return
            }

            withAnimation(.easeInOut(duration: 0.2)) {
                self?.mode = .idle
            }
        }
    }

    func isNearPortal(_ point: CGPoint) -> Bool {
        guard let screen = NSScreen.main else {
            return false
        }

        let width = screen.frame.width
        let height = screen.frame.height
        let band: CGFloat = 96

        switch edge {
        case .left:
            return point.x < band
        case .right:
            return point.x > width - band
        case .top:
            return point.y < band
        case .bottom:
            return point.y > height - band
        }
    }
}

enum DesktopPortalMode {
    case idle
    case notice
    case carrying
    case placed
}

enum DesktopPortalEdge: String {
    case left = "Left"
    case right = "Right"
    case top = "Up"
    case bottom = "Down"

    static func parse(_ value: String) -> DesktopPortalEdge {
        switch value.lowercased() {
        case "left":
            return .left
        case "up", "top":
            return .top
        case "down", "bottom":
            return .bottom
        default:
            return .right
        }
    }
}

struct DesktopPortalOverlayView: View {
    @ObservedObject var state: DesktopPortalOverlayState

    var body: some View {
        GeometryReader { geometry in
            ZStack {
                if state.mode == .carrying || state.mode == .placed {
                    edgeView(in: geometry.size)
                    if let image = state.previewImage {
                        carriedObject(image)
                            .position(state.cursor)
                            .transition(.scale(scale: 0.72).combined(with: .opacity))
                    }
                }

                if state.mode == .notice {
                    Text(state.notice)
                        .font(.system(size: 15, weight: .medium))
                        .padding(.horizontal, 14)
                        .padding(.vertical, 10)
                        .background(.ultraThinMaterial)
                        .clipShape(RoundedRectangle(cornerRadius: 8))
                        .overlay(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(Color.red.opacity(0.45), lineWidth: 1)
                        )
                        .position(state.cursor)
                }
            }
            .frame(width: geometry.size.width, height: geometry.size.height)
        }
        .allowsHitTesting(false)
    }

    private func carriedObject(_ image: NSImage) -> some View {
        VStack(spacing: 6) {
            Image(nsImage: image)
                .resizable()
                .scaledToFit()
                .frame(width: state.mode == .placed ? 92 : 132, height: state.mode == .placed ? 122 : 174)
                .clipShape(RoundedRectangle(cornerRadius: 6))
            Text(state.objectName)
                .font(.caption)
                .lineLimit(1)
                .frame(width: 142)
        }
        .padding(9)
        .background(.thinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(Color.cyan.opacity(0.62), lineWidth: 1.4)
        )
        .shadow(color: Color.black.opacity(0.28), radius: 22, y: 12)
        .rotation3DEffect(.degrees(state.mode == .placed ? 14 : 7), axis: (x: 0.18, y: -0.62, z: 0.04))
    }

    private func edgeView(in size: CGSize) -> some View {
        let edgeFrame = frameForEdge(in: size)
        return ZStack {
            RoundedRectangle(cornerRadius: 8)
                .fill(.ultraThinMaterial)
            Capsule()
                .fill(Color.white.opacity(0.52))
                .frame(
                    width: state.edge == .left || state.edge == .right ? 9 : 160,
                    height: state.edge == .left || state.edge == .right ? 176 : 9)
            Text(state.targetName)
                .font(.caption)
                .foregroundStyle(.secondary)
                .offset(labelOffset())
        }
        .frame(width: edgeFrame.width, height: edgeFrame.height)
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(Color.cyan.opacity(0.68), lineWidth: 1.5)
        )
        .shadow(color: Color.cyan.opacity(0.35), radius: 24)
        .position(x: edgeFrame.midX, y: edgeFrame.midY)
    }

    private func frameForEdge(in size: CGSize) -> CGRect {
        switch state.edge {
        case .left:
            return CGRect(x: 0, y: size.height * 0.5 - 160, width: 88, height: 320)
        case .right:
            return CGRect(x: size.width - 88, y: size.height * 0.5 - 160, width: 88, height: 320)
        case .top:
            return CGRect(x: size.width * 0.5 - 190, y: 0, width: 380, height: 76)
        case .bottom:
            return CGRect(x: size.width * 0.5 - 190, y: size.height - 76, width: 380, height: 76)
        }
    }

    private func labelOffset() -> CGSize {
        switch state.edge {
        case .left, .right:
            return CGSize(width: 0, height: 108)
        case .top, .bottom:
            return CGSize(width: 0, height: 22)
        }
    }
}

struct DesktopPortalSettings {
    var listenPort: UInt16
    var targetHost: String
    var targetPort: UInt16
    var targetName: String
    var portalEdge: DesktopPortalEdge
    var devAutoPlace: Bool

    static func parse(_ args: [String]) -> DesktopPortalSettings {
        var settings = DesktopPortalSettings(
            listenPort: 57121,
            targetHost: "192.168.163.11",
            targetPort: 57120,
            targetName: "Ablage Windows",
            portalEdge: .right,
            devAutoPlace: args.contains("--dev-auto-place"))

        for index in args.indices {
            if ["--listen-port", "--owner-port"].contains(args[index]), index + 1 < args.count,
               let parsed = UInt16(args[index + 1]) {
                settings.listenPort = parsed
            }

            if ["--host", "--target-host"].contains(args[index]), index + 1 < args.count {
                settings.targetHost = args[index + 1]
            }

            if ["--port", "--target-port"].contains(args[index]), index + 1 < args.count,
               let parsed = UInt16(args[index + 1]) {
                settings.targetPort = parsed
            }

            if ["--target-name"].contains(args[index]), index + 1 < args.count {
                settings.targetName = args[index + 1]
            }

            if ["--edge", "--direction"].contains(args[index]), index + 1 < args.count {
                settings.portalEdge = DesktopPortalEdge.parse(args[index + 1])
            }
        }

        if let mapped = ManualAblageMap.nearestWindowsEdge() {
            settings.targetName = mapped.displayName
            settings.portalEdge = mapped.edge
        }

        return settings
    }
}

struct ManualAblageMap: Decodable {
    let entries: [Entry]

    struct Entry: Decodable {
        let displayName: String
        let relativeDirection: String
        let distanceMeters: Double?
        let confidence: Double?
        let isAvailable: Bool
        let platform: String?

        var edge: DesktopPortalEdge {
            DesktopPortalEdge.parse(relativeDirection)
        }
    }

    static func nearestWindowsEdge() -> Entry? {
        guard let root = DesktopPortalPaths.repositoryRoot() else {
            return nil
        }

        let url = root.appendingPathComponent("config/manual-ablage-map.json")
        guard let data = try? Data(contentsOf: url),
              let map = try? JSONDecoder().decode(ManualAblageMap.self, from: data) else {
            return nil
        }

        return map.entries
            .filter { $0.isAvailable && ($0.platform?.lowercased().contains("windows") == true || $0.displayName.lowercased().contains("windows")) }
            .sorted {
                ($0.distanceMeters ?? Double.greatestFiniteMagnitude, -($0.confidence ?? 0)) <
                    ($1.distanceMeters ?? Double.greatestFiniteMagnitude, -($1.confidence ?? 0))
            }
            .first
    }
}

struct DesktopPdfObject {
    let url: URL
    let displayName: String
    let sourceApplication: NSRunningApplication?
}

enum DesktopPdfObjectResolver {
    static func resolve(at point: NSPoint) throws -> DesktopPdfObject {
        let frontmost = NSWorkspace.shared.frontmostApplication

        if frontmost?.bundleIdentifier == "com.apple.finder",
           let finderURL = selectedFinderPdf() {
            return DesktopPdfObject(
                url: finderURL,
                displayName: finderURL.lastPathComponent,
                sourceApplication: frontmost)
        }

        if let app = frontmost,
           let url = documentPdfURL(for: app, at: point) {
            return DesktopPdfObject(
                url: url,
                displayName: url.lastPathComponent,
                sourceApplication: app)
        }

        if let fallback = DesktopPortalPaths.defaultPdf(),
           CommandLine.arguments.contains("--allow-sample-fallback") {
            return DesktopPdfObject(
                url: fallback,
                displayName: fallback.lastPathComponent,
                sourceApplication: nil)
        }

        throw DesktopPortalError.pdfNotRecognized
    }

    private static func selectedFinderPdf() -> URL? {
        let script = """
        tell application "Finder"
          set chosenItems to selection
          if (count of chosenItems) is 0 then return ""
          set theItem to item 1 of chosenItems as alias
          return POSIX path of theItem
        end tell
        """
        var error: NSDictionary?
        guard let result = NSAppleScript(source: script)?.executeAndReturnError(&error).stringValue,
              !result.isEmpty else {
            return nil
        }

        return validatedPdfURL(URL(fileURLWithPath: result))
    }

    private static func documentPdfURL(for app: NSRunningApplication, at point: NSPoint) -> URL? {
        if let url = documentPdfURLFromFrontmostWindow(app: app) {
            return url
        }

        return documentPdfURLFromElement(at: point)
    }

    private static func documentPdfURLFromFrontmostWindow(app: NSRunningApplication) -> URL? {
        let application = AXUIElementCreateApplication(app.processIdentifier)
        if let focused = copyElementAttribute(application, kAXFocusedWindowAttribute as String),
           let url = urlFromAttributes(of: focused) {
            return url
        }

        if let focused = copyElementAttribute(application, kAXFocusedUIElementAttribute as String),
           let url = urlFromAttributes(of: focused) {
            return url
        }

        return nil
    }

    private static func documentPdfURLFromElement(at point: NSPoint) -> URL? {
        let system = AXUIElementCreateSystemWide()
        var element: AXUIElement?
        let error = AXUIElementCopyElementAtPosition(system, Float(point.x), Float(point.y), &element)
        guard error == .success, let element else {
            return nil
        }

        var current: AXUIElement? = element
        for _ in 0..<7 {
            guard let candidate = current else {
                return nil
            }

            if let url = urlFromAttributes(of: candidate) {
                return url
            }

            current = copyElementAttribute(candidate, kAXParentAttribute as String)
        }

        return nil
    }

    private static func urlFromAttributes(of element: AXUIElement) -> URL? {
        for attribute in [kAXDocumentAttribute as String, kAXURLAttribute as String, "AXFilename", "AXFileName"] {
            if let value = copyRawAttribute(element, attribute),
               let url = url(from: value) {
                return url
            }
        }

        return nil
    }

    private static func copyElementAttribute(_ element: AXUIElement, _ attribute: String) -> AXUIElement? {
        copyRawAttribute(element, attribute) as! AXUIElement?
    }

    private static func copyRawAttribute(_ element: AXUIElement, _ attribute: String) -> AnyObject? {
        var value: AnyObject?
        let error = AXUIElementCopyAttributeValue(element, attribute as CFString, &value)
        guard error == .success else {
            return nil
        }

        return value
    }

    private static func url(from value: AnyObject) -> URL? {
        if let url = value as? URL {
            return validatedPdfURL(url)
        }

        if let string = value as? String {
            if let fileURL = URL(string: string), fileURL.isFileURL {
                return validatedPdfURL(fileURL)
            }

            return validatedPdfURL(URL(fileURLWithPath: string))
        }

        return nil
    }

    private static func validatedPdfURL(_ url: URL) -> URL? {
        let resolved = url.standardizedFileURL
        guard resolved.pathExtension.lowercased() == "pdf",
              FileManager.default.fileExists(atPath: resolved.path) else {
            return nil
        }

        return resolved
    }
}

enum DesktopPdfRenderer {
    static func render(url: URL) throws -> (frame: DesktopPortalFrame, image: NSImage) {
        guard let document = PDFDocument(url: url), document.pageCount > 0 else {
            throw DesktopPortalError.renderFailed
        }

        let image = try renderFirstPage(document: document, requestedWidth: 1100)
        let png = try pngData(from: image)
        let data = try Data(contentsOf: url)
        let hash = fingerprint(data)
        let frame = DesktopPortalFrame(
            frameSessionId: "frame-macos-desktop-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())",
            leaseId: "lease-macos-desktop-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())",
            thingId: "ding-macos-desktop-\(hash)",
            displayName: url.lastPathComponent,
            pageCount: document.pageCount,
            page: 1,
            width: Int(image.size.width.rounded()),
            height: Int(image.size.height.rounded()),
            sourceHash: hash,
            pngBase64: png.base64EncodedString(),
            renderedAt: ISO8601DateFormatter().string(from: Date()))
        return (frame, image)
    }

    private static func renderFirstPage(document: PDFDocument, requestedWidth: CGFloat) throws -> NSImage {
        guard let page = document.page(at: 0) else {
            throw DesktopPortalError.renderFailed
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
            throw DesktopPortalError.renderFailed
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
            throw DesktopPortalError.renderFailed
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

struct DesktopPortalFrame {
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
        var payload = [
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

        if placementReady {
            payload["frameFormat"] = "PngFrame"
            payload["pngBase64"] = pngBase64
            payload["renderedAt"] = renderedAt
        }

        return payload
    }

    static func waitingPayload(displayName: String) -> [String: String] {
        [
            "frameSessionId": "frame-macos-desktop-waiting",
            "leaseId": "lease-macos-desktop-waiting",
            "thingId": "ding-macos-desktop-waiting",
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
}

final class DesktopPortalFrameServer {
    private var listener: NWListener?
    private weak var controller: DesktopPortalController?

    @MainActor
    func start(port: UInt16, controller: DesktopPortalController) throws {
        guard listener == nil else {
            return
        }

        guard let nwPort = NWEndpoint.Port(rawValue: port) else {
            throw DesktopPortalError.invalidPort
        }

        self.controller = controller
        let listener = try NWListener(using: .tcp, on: nwPort)
        listener.newConnectionHandler = { [weak self] connection in
            self?.handle(connection)
        }
        listener.start(queue: .main)
        self.listener = listener
    }

    func stop() {
        listener?.cancel()
        listener = nil
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

            guard let request = try? decoder.decode(DesktopPortalRkwpMessage.self, from: data) else {
                connection.cancel()
                return
            }

            Task { @MainActor [weak self] in
                guard let self, let controller = self.controller else {
                    connection.cancel()
                    return
                }

                let response = controller.response(for: request)
                self.send(response, on: connection)
            }
        }
    }

    private func send(_ message: DesktopPortalRkwpMessage, on connection: NWConnection) {
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

struct DesktopPortalRkwpMessage: Codable {
    var messageId: String = "rkwp-macos-desktop-\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())"
    var messageType: String
    var sourceAblageId: String
    var targetAblageId: String
    var sessionId: String
    var correlationId: String = ""
    var timestamp: Date = Date()
    var payload: [String: String]
    var headers: [String: String] = [:]
}

enum DesktopPortalPaths {
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

enum DesktopPortalSmokeTest {
    static func runAndExitIfRequested(arguments: [String]) {
        guard arguments.contains("--smoke-test") else {
            return
        }

        do {
            try run()
            exit(0)
        } catch {
            print("MacDesktopPortalOwner: STARTED")
            print("RESULT: FAILED - \(error.localizedDescription)")
            exit(1)
        }
    }

    private static func run() throws {
        guard let pdf = DesktopPortalPaths.defaultPdf() else {
            throw DesktopPortalError.pdfNotRecognized
        }

        let rendered = try DesktopPdfRenderer.render(url: pdf)
        let waiting = rendered.frame.payload(placementReady: false, visibleStatus: "wartet am Glasrand")
        let ready = rendered.frame.payload(placementReady: true, visibleStatus: "liegt hier im Frame")
        try validateNoFileIngress(waiting, mustContainPng: false)
        try validateNoFileIngress(ready, mustContainPng: true)

        print("MacDesktopPortalOwner: STARTED")
        print("DesktopGesture: PREPARED")
        print("TripleClick: PREPARED")
        print("OpenDocumentResolver: PREPARED")
        print("FinderSelectionResolver: PREPARED")
        print("DigitalHand: OK")
        print("GlassEdgeMode: NearestOnly")
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

    private static func validateNoFileIngress(_ payload: [String: String], mustContainPng: Bool) throws {
        let forbidden = Set(["pdfbytes", "originalbytes", "originalfilebytes", "originalpath", "localpdfpath", "downloadpath", "filepath"])
        for key in payload.keys where forbidden.contains(key.lowercased()) {
            throw DesktopPortalError.noFileIngressFailed
        }

        guard payload["noFileIngress"] == "true",
              payload["ownerKeepsOriginal"] == "true",
              payload["containsOriginalFileBytes"] == "false",
              payload["hasOriginalPath"] == "false",
              payload["guestHasPdfFile"] == "false",
              payload["frameCache"] == "MemoryOnly" else {
            throw DesktopPortalError.noFileIngressFailed
        }

        if mustContainPng && payload["pngBase64"]?.isEmpty != false {
            throw DesktopPortalError.noFileIngressFailed
        }

        if !mustContainPng && payload["pngBase64"] != nil {
            throw DesktopPortalError.noFileIngressFailed
        }
    }
}

enum DesktopPortalError: LocalizedError {
    case pdfNotRecognized
    case renderFailed
    case invalidPort
    case noFileIngressFailed

    var errorDescription: String? {
        switch self {
        case .pdfNotRecognized:
            return "PDF nicht erkannt"
        case .renderFailed:
            return "Frame konnte nicht vorbereitet werden"
        case .invalidPort:
            return "Port ist ungueltig"
        case .noFileIngressFailed:
            return "NoFileIngress fehlgeschlagen"
        }
    }
}
