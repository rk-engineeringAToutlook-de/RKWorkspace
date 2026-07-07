import AppKit
import SwiftUI

@main
struct MacPdfFrameGuestApp: App {
    @StateObject private var model = FrameGuestModel()

    init() {
        SmokeTestRunner.runAndExitIfRequested(arguments: CommandLine.arguments)
    }

    var body: some Scene {
        WindowGroup("RK Workspace Ablage") {
            ContentView(model: model)
                .frame(minWidth: 880, minHeight: 640)
                .onAppear {
                    model.applyCommandLine()
                }
        }
    }
}

struct ContentView: View {
    @ObservedObject var model: FrameGuestModel

    var body: some View {
        VStack(spacing: 16) {
            header
            connection
            frameSurface
            status
        }
        .padding(22)
        .background(Color(nsColor: .windowBackgroundColor))
    }

    private var header: some View {
        HStack {
            VStack(alignment: .leading, spacing: 4) {
                Text(model.visibleName)
                    .font(.system(size: 28, weight: .semibold))
                Text(model.visibleState)
                    .foregroundStyle(.secondary)
            }
            Spacer()
            VStack(alignment: .trailing, spacing: 4) {
                Text("Original bleibt bei Windows")
                    .font(.callout)
                Text("nur Frame / nur im Speicher")
                    .foregroundStyle(.secondary)
                    .font(.caption)
            }
        }
    }

    private var connection: some View {
        HStack(spacing: 10) {
            TextField("Windows", text: $model.host)
                .textFieldStyle(.roundedBorder)
                .frame(width: 180)
            TextField("Port", text: $model.portText)
                .textFieldStyle(.roundedBorder)
                .frame(width: 82)
            Button("Frame holen") {
                model.openFrame()
            }
            .keyboardShortcut(.defaultAction)
            Button("zurueckgeben") {
                model.returnFrame()
            }
            .disabled(model.frameSessionId.isEmpty)
            Spacer()
        }
    }

    private var frameSurface: some View {
        ZStack {
            RoundedRectangle(cornerRadius: 18)
                .fill(Color(nsColor: .textBackgroundColor))
                .overlay(
                    RoundedRectangle(cornerRadius: 18)
                        .stroke(Color(nsColor: .separatorColor), lineWidth: 1)
                )

            if let image = model.image {
                Image(nsImage: image)
                    .resizable()
                    .scaledToFit()
                    .padding(28)
                    .transition(.opacity)
            } else {
                VStack(spacing: 10) {
                    Text("Noch kein Frame")
                        .font(.title2)
                    Text("Das Ding liegt noch nicht hier.")
                        .foregroundStyle(.secondary)
                }
            }

            if model.receivingEdgeOpen {
                HStack {
                    Spacer()
                    ReceivingGlassEdge()
                        .padding(.trailing, 12)
                }
                .transition(.opacity)
            }
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }

    private var status: some View {
        HStack {
            Text(model.status)
                .foregroundStyle(model.hasError ? .red : .secondary)
            Spacer()
            Text(model.noFileIngressText)
                .foregroundStyle(.secondary)
                .font(.caption)
        }
    }
}

struct ReceivingGlassEdge: View {
    var body: some View {
        VStack(spacing: 12) {
            Capsule()
                .fill(Color.white.opacity(0.55))
                .frame(width: 8, height: 64)
            Text("Glasrand")
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .frame(width: 86, height: 260)
        .background(.ultraThinMaterial)
        .clipShape(RoundedRectangle(cornerRadius: 8))
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(Color.cyan.opacity(0.55), lineWidth: 1.5)
        )
        .shadow(color: Color.cyan.opacity(0.28), radius: 18)
    }
}

@MainActor
final class FrameGuestModel: ObservableObject {
    @Published var visibleName: String
    @Published var host: String
    @Published var portText: String
    @Published var status = "macOSGuestAblage: STARTED"
    @Published var visibleState = "bereit fuer Frame"
    @Published var noFileIngressText = "GuestHasPdfFile: NO"
    @Published var image: NSImage?
    @Published var hasError = false
    @Published var receivingEdgeOpen = false

    var frameSessionId = ""
    private var leaseId = ""
    private var sessionId = ""
    private var usesLocalVerificationFrame = false
    private var autoReturnDelaySeconds: Double?
    private var exitAfterReturn = false
    private let configuration: MacGuestConfiguration

    init(configuration: MacGuestConfiguration = .loadDefault()) {
        self.configuration = configuration
        visibleName = configuration.visibleName
        host = configuration.ownerDiscovery.windowsOwnerHost
        portText = String(configuration.ownerDiscovery.devTransportPort)
        noFileIngressText = configuration.framePolicy.memoryOnlyCache
            ? "GuestHasPdfFile: NO | FrameCache: MemoryOnly"
            : "GuestHasPdfFile: NO"
    }

    func applyCommandLine() {
        let args = CommandLine.arguments
        let environment = ProcessInfo.processInfo.environment
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
        }
    }

    func openFrame() {
        hasError = false
        receivingEdgeOpen = false
        status = "frage Frame an..."
        Task {
            do {
                let port = UInt16(portText) ?? 57120
                let client = RkwpDevLanClient(host: host, port: port)
                let hello = try await client.request(RkwpMessage(
                    messageType: "AblageHello",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: "",
                    payload: [
                        "displayName": "Ablage macOS",
                        "frameOnly": "true",
                        "noFileIngress": "true"
                    ]))
                sessionId = hello.sessionId
                print("AblageHello: OK")

                _ = try await client.request(RkwpMessage(
                    messageType: "AblageCapabilities",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: sessionId,
                    payload: [
                        "frameOnly": "true",
                        "noFileIngress": "true",
                        "memoryOnlyFrame": "true"
                    ]))
                print("AblageCapabilities: OK")

                let frame = try await client.request(RkwpMessage(
                    messageType: "FrameUpdate",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: sessionId,
                    payload: [
                        "request": "openFrame",
                        "noFileIngress": "true"
                    ]))

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
        receivingEdgeOpen = true
        visibleState = "wartet auf Ablage am Glasrand"
        status = "warte auf deine PDF von Windows..."
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
                            payload: [
                                "displayName": "Ablage macOS",
                                "frameOnly": "true",
                                "noFileIngress": "true",
                                "waitForPlacement": "true"
                            ]))
                        sessionId = hello.sessionId
                        print("AblageHello: OK")

                        _ = try await client.request(RkwpMessage(
                            messageType: "AblageCapabilities",
                            sourceAblageId: "ablage-macos-guest",
                            targetAblageId: "ablage-windows-owner",
                            sessionId: sessionId,
                            payload: [
                                "frameOnly": "true",
                                "noFileIngress": "true",
                                "memoryOnlyFrame": "true",
                                "waitForPlacement": "true"
                            ]))
                        print("AblageCapabilities: OK")
                        handshakeComplete = true
                    }

                    let frame = try await client.request(RkwpMessage(
                        messageType: "FrameUpdate",
                        sourceAblageId: "ablage-macos-guest",
                        targetAblageId: "ablage-windows-owner",
                        sessionId: sessionId,
                        payload: [
                            "request": "openFrame",
                            "waitForPlacement": "true",
                            "noFileIngress": "true"
                        ]))

                    if frame.payload["placementReady"] == "false" || frame.payload["pngBase64"] == nil {
                        hasError = false
                        visibleState = frame.payload["visibleStatus"] ?? "wartet auf Ablage am Glasrand"
                        status = "bereit: PDF auf Windows am Glasrand ablegen"
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
                        "noFileIngress": "true"
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
            try presentFrame(LocalVerificationFrame.message(), localVerification: true)
        } catch {
            hasError = true
            status = "nicht verfuegbar: \(error.localizedDescription)"
        }
    }

    private func presentFrame(_ frame: RkwpMessage, localVerification: Bool) throws {
        try NoFileIngressVerifier.validatePayload(frame.payload)
        guard let pngBase64 = frame.payload["pngBase64"],
              let pngData = Data(base64Encoded: pngBase64),
              let nsImage = NSImage(data: pngData) else {
            throw FrameGuestError.invalidFrame
        }

        frameSessionId = frame.payload["frameSessionId"] ?? ""
        leaseId = frame.payload["leaseId"] ?? ""
        sessionId = frame.sessionId
        image = nsImage
        receivingEdgeOpen = false
        usesLocalVerificationFrame = localVerification
        visibleState = frame.payload["visibleStatus"] ?? "liegt hier im Frame"
        status = "FrameView: OK"
        noFileIngressText = "GuestHasPdfFile: NO | GuestHasOriginalPath: NO | OriginalFileBytes: NO | NoFileIngress: SUCCESS"
        print("FrameView: OK")
        print("GuestHasPdfFile: NO")
        print("GuestHasOriginalPath: NO")
        print("OriginalFileBytes: NO")
        print("FrameCache: MemoryOnly")
        print("NoFileIngress: SUCCESS")
        scheduleAutoReturnIfNeeded()
    }

    private func clearFrame() {
        image = nil
        receivingEdgeOpen = false
        frameSessionId = ""
        leaseId = ""
        usesLocalVerificationFrame = false
    }

    private func finishReturn() {
        clearFrame()
        visibleState = "zurueckgegeben"
        status = "Return: SUCCESS"
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

        var input: InputStream?
        var output: OutputStream?
        Stream.getStreamsToHost(withName: host, port: Int(port), inputStream: &input, outputStream: &output)

        guard let inputStream = input, let outputStream = output else {
            throw FrameGuestError.connectionFailed
        }

        inputStream.open()
        outputStream.open()
        defer {
            inputStream.close()
            outputStream.close()
        }

        let bytes = Array(requestLine.utf8)
        let written = bytes.withUnsafeBufferPointer {
            outputStream.write($0.baseAddress!, maxLength: bytes.count)
        }

        guard written == bytes.count else {
            throw FrameGuestError.writeFailed
        }

        var response = Data()
        var buffer = [UInt8](repeating: 0, count: 4096)
        let deadline = Date().addingTimeInterval(10)

        while Date() < deadline {
            if inputStream.hasBytesAvailable {
                let read = inputStream.read(&buffer, maxLength: buffer.count)
                if read < 0 {
                    throw FrameGuestError.readFailed
                }

                if read == 0 {
                    break
                }

                response.append(buffer, count: read)
                if response.contains(0x0A) {
                    break
                }
            } else {
                Thread.sleep(forTimeInterval: 0.02)
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
