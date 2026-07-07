import AppKit
import SwiftUI

@main
struct MacPdfFrameGuestApp: App {
    @StateObject private var model = FrameGuestModel()

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
                Text("Ablage macOS")
                    .font(.system(size: 28, weight: .semibold))
                Text(model.visibleState)
                    .foregroundStyle(.secondary)
            }
            Spacer()
            VStack(alignment: .trailing, spacing: 4) {
                Text("Original bleibt bei Windows")
                    .font(.callout)
                Text("FrameOnly / MemoryOnly")
                    .foregroundStyle(.secondary)
                    .font(.caption)
            }
        }
    }

    private var connection: some View {
        HStack(spacing: 10) {
            TextField("Windows-IP", text: $model.host)
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

@MainActor
final class FrameGuestModel: ObservableObject {
    @Published var host = "127.0.0.1"
    @Published var portText = "57120"
    @Published var status = "macOSGuestAblage: STARTED"
    @Published var visibleState = "bereit fuer Frame"
    @Published var noFileIngressText = "GuestHasPdfFile: NO"
    @Published var image: NSImage?
    @Published var hasError = false

    var frameSessionId = ""
    private var leaseId = ""
    private var sessionId = ""

    func applyCommandLine() {
        let args = CommandLine.arguments
        for index in args.indices {
            if args[index] == "--host", index + 1 < args.count {
                host = args[index + 1]
            }

            if args[index] == "--port", index + 1 < args.count {
                portText = args[index + 1]
            }
        }
    }

    func openFrame() {
        hasError = false
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

                let frame = try await client.request(RkwpMessage(
                    messageType: "FrameUpdate",
                    sourceAblageId: "ablage-macos-guest",
                    targetAblageId: "ablage-windows-owner",
                    sessionId: sessionId,
                    payload: [
                        "request": "openFrame",
                        "noFileIngress": "true"
                    ]))

                guard frame.payload["containsOriginalFileBytes"] == "false",
                      frame.payload["hasOriginalPath"] == "false",
                      frame.payload["noFileIngress"] == "true",
                      let pngBase64 = frame.payload["pngBase64"],
                      let pngData = Data(base64Encoded: pngBase64),
                      let nsImage = NSImage(data: pngData) else {
                    throw FrameGuestError.invalidFrame
                }

                frameSessionId = frame.payload["frameSessionId"] ?? ""
                leaseId = frame.payload["leaseId"] ?? ""
                image = nsImage
                visibleState = "liegt hier im Frame"
                status = "FrameView: OK"
                noFileIngressText = "GuestHasPdfFile: NO | GuestHasOriginalPath: NO | OriginalFileBytes: NO | NoFileIngress: SUCCESS"
            } catch {
                hasError = true
                status = "nicht verfuegbar: \(error.localizedDescription)"
            }
        }
    }

    func returnFrame() {
        guard !frameSessionId.isEmpty else {
            return
        }

        hasError = false
        status = "gebe zurueck..."
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

                image = nil
                frameSessionId = ""
                leaseId = ""
                visibleState = "zurueckgegeben"
                status = "Return: SUCCESS"
            } catch {
                hasError = true
                status = "nicht verfuegbar: \(error.localizedDescription)"
            }
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

final class RkwpDevLanClient {
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
        }
    }
}
