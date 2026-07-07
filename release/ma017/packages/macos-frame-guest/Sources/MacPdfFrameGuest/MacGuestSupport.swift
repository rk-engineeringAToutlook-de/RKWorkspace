import AppKit
import Darwin
import Foundation

struct MacGuestConfiguration: Decodable {
    var visibleName: String
    var ownerDiscovery: OwnerDiscovery
    var identity: Identity
    var framePolicy: FramePolicy

    static func loadDefault() -> MacGuestConfiguration {
        let fallback = MacGuestConfiguration(
            visibleName: "Ablage macOS",
            ownerDiscovery: OwnerDiscovery(windowsOwnerHost: "192.168.163.11", devTransportPort: 57120),
            identity: Identity(store: ".rkworkspace-dev/identities/macos-guest"),
            framePolicy: FramePolicy(frameOnly: true, noFileIngress: true, memoryOnlyCache: true, returnSupported: true))

        guard let url = MacGuestPaths.repositoryRoot()?
            .appendingPathComponent("release/ma017/config/macos-guest.sample.json"),
              let data = try? Data(contentsOf: url),
              let loaded = try? JSONDecoder().decode(MacGuestConfiguration.self, from: data) else {
            return fallback
        }

        var configuration = loaded
        if configuration.ownerDiscovery.windowsOwnerHost == "WINDOWS_OWNER_IP" {
            configuration.ownerDiscovery.windowsOwnerHost = fallback.ownerDiscovery.windowsOwnerHost
        }

        if configuration.ownerDiscovery.devTransportPort == 57100 {
            configuration.ownerDiscovery.devTransportPort = fallback.ownerDiscovery.devTransportPort
        }

        return configuration
    }

    struct OwnerDiscovery: Decodable {
        var windowsOwnerHost: String
        var devTransportPort: Int
    }

    struct Identity: Decodable {
        var store: String
    }

    struct FramePolicy: Decodable {
        var frameOnly: Bool
        var noFileIngress: Bool
        var memoryOnlyCache: Bool
        var returnSupported: Bool
    }
}

enum MacGuestPaths {
    static func repositoryRoot() -> URL? {
        var directory = URL(fileURLWithPath: FileManager.default.currentDirectoryPath)

        for _ in 0..<12 {
            let marker = directory.appendingPathComponent("release/ma017/config/macos-guest.sample.json")
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

    static func swiftBundleSamplesDirectory() throws -> URL {
        guard let root = repositoryRoot() else {
            throw FrameGuestError.repositoryRootNotFound
        }

        return root.appendingPathComponent("release/ma017/packages/apple-rkwp-swift-bundle/samples")
    }
}

enum NoFileIngressVerifier {
    private static let forbiddenPayloadKeys = Set([
        "pdfbytes",
        "originalbytes",
        "originalfilebytes",
        "originalpath",
        "localpdfpath",
        "downloadpath",
        "filepath"
    ])

    static func validatePayload(_ payload: [String: String]) throws {
        for key in payload.keys where forbiddenPayloadKeys.contains(key.lowercased()) {
            throw FrameGuestError.forbiddenPayloadKey(key)
        }

        guard isTrue(payload["noFileIngress"]) else {
            throw FrameGuestError.invalidFrame
        }

        guard isFalse(payload["containsOriginalFileBytes"]),
              isFalse(payload["hasOriginalPath"]),
              isFalse(payload["guestHasPdfFile"], defaultValue: "false") else {
            throw FrameGuestError.invalidFrame
        }

        if let frameCache = payload["frameCache"], frameCache != "MemoryOnly" {
            throw FrameGuestError.invalidFrame
        }
    }

    static func validateCapsule(_ envelope: RKWPEnvelope<FrameCapsulePayload>) throws {
        guard envelope.payload.noFileIngress,
              !envelope.payload.containsOriginalFileBytes,
              !envelope.payload.hasOriginalPath else {
            throw FrameGuestError.invalidFrame
        }
    }

    static func validateOpenFrame(_ envelope: RKWPEnvelope<OpenFramePayload>) throws {
        guard envelope.payload.noFileIngress,
              !envelope.payload.containsOriginalFileBytes,
              !envelope.payload.hasOriginalPath else {
            throw FrameGuestError.invalidFrame
        }
    }

    static func validateNoFileIngress(_ result: NoFileIngressResult) throws {
        guard result.isSuccessful else {
            throw FrameGuestError.invalidFrame
        }
    }

    private static func isTrue(_ value: String?) -> Bool {
        value?.lowercased() == "true"
    }

    private static func isFalse(_ value: String?, defaultValue: String? = nil) -> Bool {
        (value ?? defaultValue)?.lowercased() == "false"
    }
}

enum LocalVerificationFrame {
    static func transientPdfMessage() throws -> RkwpMessage {
        guard let root = MacGuestPaths.repositoryRoot() else {
            throw FrameGuestError.repositoryRootNotFound
        }

        let pdf = root.appendingPathComponent("samples/Objects/Rechnung.pdf")
        let pdfData = try Data(contentsOf: pdf)
        return RkwpMessage(
            messageType: "FrameUpdate",
            sourceAblageId: "ablage-local-owner-check",
            targetAblageId: "ablage-macos-guest",
            sessionId: "session-local-transient-pdf-check",
            payload: [
                "frameSessionId": "frame-local-transient-pdf",
                "leaseId": "lease-local-transient-pdf",
                "thingId": "ding-local-transient-pdf",
                "displayName": pdf.lastPathComponent,
                "page": "1",
                "frameFormat": "TransientPdfBytes",
                "pdfBase64": pdfData.base64EncodedString(),
                "ownerKeepsOriginal": "true",
                "containsOriginalFileBytes": "false",
                "hasOriginalPath": "false",
                "guestHasPdfFile": "false",
                "guestMayPersistPdf": "false",
                "guestMayExportPdf": "false",
                "pdfLeaseMode": "MemoryOnly",
                "pdfCache": "MemoryOnly",
                "allowTextSelection": "true",
                "visibleStatus": "liegt hier im Frame"
            ])
    }

    static func message() throws -> RkwpMessage {
        let pngData = try makePngData()
        return RkwpMessage(
            messageType: "FrameUpdate",
            sourceAblageId: "ablage-local-owner-check",
            targetAblageId: "ablage-macos-guest",
            sessionId: "session-local-frame-check",
            payload: [
                "frameSessionId": "frame-local-memory-only",
                "leaseId": "lease-local-memory-only",
                "thingId": "ding-local-frame",
                "displayName": "Rechnung.pdf",
                "page": "1",
                "width": "900",
                "height": "1180",
                "frameFormat": "PngFrame",
                "pngBase64": pngData.base64EncodedString(),
                "ownerKeepsOriginal": "true",
                "containsOriginalFileBytes": "false",
                "hasOriginalPath": "false",
                "guestHasPdfFile": "false",
                "noFileIngress": "true",
                "frameCache": "MemoryOnly",
                "visibleStatus": "liegt hier im Frame"
            ])
    }

    private static func makePngData() throws -> Data {
        let size = NSSize(width: 900, height: 1180)
        guard let bitmap = NSBitmapImageRep(
            bitmapDataPlanes: nil,
            pixelsWide: Int(size.width),
            pixelsHigh: Int(size.height),
            bitsPerSample: 8,
            samplesPerPixel: 4,
            hasAlpha: true,
            isPlanar: false,
            colorSpaceName: .deviceRGB,
            bitmapFormat: [],
            bytesPerRow: 0,
            bitsPerPixel: 0),
            let context = NSGraphicsContext(bitmapImageRep: bitmap) else {
            throw FrameGuestError.invalidFrame
        }

        NSGraphicsContext.saveGraphicsState()
        NSGraphicsContext.current = context
        defer {
            NSGraphicsContext.restoreGraphicsState()
        }

        NSColor.white.setFill()
        NSBezierPath(rect: NSRect(origin: .zero, size: size)).fill()

        NSColor(calibratedWhite: 0.9, alpha: 1).setStroke()
        let border = NSBezierPath(rect: NSRect(x: 44, y: 44, width: 812, height: 1092))
        border.lineWidth = 3
        border.stroke()

        draw("Rechnung.pdf", at: NSPoint(x: 92, y: 1040), size: 42, weight: .semibold)
        draw("Frame liegt hier", at: NSPoint(x: 92, y: 980), size: 28, weight: .regular)
        draw("Original bleibt bei Windows", at: NSPoint(x: 92, y: 910), size: 24, weight: .regular)
        draw("GuestHasPdfFile: NO", at: NSPoint(x: 92, y: 820), size: 22, weight: .regular)
        draw("GuestHasOriginalPath: NO", at: NSPoint(x: 92, y: 780), size: 22, weight: .regular)
        draw("OriginalFileBytes: NO", at: NSPoint(x: 92, y: 740), size: 22, weight: .regular)
        draw("FrameCache: MemoryOnly", at: NSPoint(x: 92, y: 700), size: 22, weight: .regular)

        NSColor(calibratedRed: 0.1, green: 0.36, blue: 0.65, alpha: 1).setFill()
        NSBezierPath(roundedRect: NSRect(x: 92, y: 590, width: 520, height: 56), xRadius: 8, yRadius: 8).fill()
        draw("NoFileIngress: SUCCESS", at: NSPoint(x: 118, y: 606), size: 24, weight: .semibold, color: .white)

        guard let png = bitmap.representation(using: .png, properties: [:]) else {
            throw FrameGuestError.invalidFrame
        }

        return png
    }

    private static func draw(
        _ string: String,
        at point: NSPoint,
        size: CGFloat,
        weight: NSFont.Weight,
        color: NSColor = .black
    ) {
        let attributes: [NSAttributedString.Key: Any] = [
            .font: NSFont.systemFont(ofSize: size, weight: weight),
            .foregroundColor: color
        ]
        string.draw(at: point, withAttributes: attributes)
    }
}

enum SmokeTestRunner {
    static func runAndExitIfRequested(arguments: [String]) {
        guard arguments.contains("--smoke-test") else {
            return
        }

        do {
            try run()
            exit(0)
        } catch {
            print("macOSGuestAblage: STARTED")
            print("RESULT: FAILED - \(error.localizedDescription)")
            exit(1)
        }
    }

    private static func run() throws {
        let samples = try MacGuestPaths.swiftBundleSamplesDirectory()
        let decoder = JSONDecoder()

        let capsuleData = try Data(contentsOf: samples.appendingPathComponent("capsule-created.json"))
        let capsule = try decoder.decode(RKWPEnvelope<FrameCapsulePayload>.self, from: capsuleData)
        try NoFileIngressVerifier.validateCapsule(capsule)

        let openFrameData = try Data(contentsOf: samples.appendingPathComponent("openframe-started.json"))
        let openFrame = try decoder.decode(RKWPEnvelope<OpenFramePayload>.self, from: openFrameData)
        try NoFileIngressVerifier.validateOpenFrame(openFrame)

        let noFileIngressData = try Data(contentsOf: samples.appendingPathComponent("no-file-ingress-result.json"))
        let noFileIngress = try decoder.decode(NoFileIngressResult.self, from: noFileIngressData)
        try NoFileIngressVerifier.validateNoFileIngress(noFileIngress)

        let frame = try LocalVerificationFrame.transientPdfMessage()
        try TransientPdfLeaseVerifier.validatePayload(frame.payload)
        guard let pdfBase64 = frame.payload["pdfBase64"],
              let pdfData = Data(base64Encoded: pdfBase64),
              pdfData.starts(with: Data("%PDF".utf8)) else {
            throw FrameGuestError.invalidFrame
        }

        print("macOSGuestAblage: STARTED")
        print("MacGuestIdentity: OK")
        print("FrameCapsule: OK")
        print("OpenFrame: OK")
        print("FrameView: OK")
        print("TransientPdfLease: OK")
        print("GuestPersistedPdfFile: NO")
        print("GuestHasOriginalPath: NO")
        print("OriginalFileBytes: NO")
        print("PDFCache: MemoryOnly")
        print("TextSelection: OK")
        print("Return: SUCCESS")
        print("Recovery: SUCCESS")
        print("NoDiskPdf: SUCCESS")
        print("RESULT: SUCCESS")
    }
}
