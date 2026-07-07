import Foundation

public enum RKWPMessageType: String, Codable {
    case ablageHello = "AblageHello"
    case ablageCapabilities = "AblageCapabilities"
    case frameSessionOpen = "FrameSessionOpen"
    case frameSessionReady = "FrameSessionReady"
    case frameUpdate = "FrameUpdate"
    case frameInput = "FrameInput"
    case carryLeaseHeartbeat = "CarryLeaseHeartbeat"
    case carryLeaseReturn = "CarryLeaseReturn"
    case carryLeaseRevoked = "CarryLeaseRevoked"
    case capsuleCreated = "CapsuleCreated"
    case capsuleOpened = "CapsuleOpened"
    case openFrameStarted = "OpenFrameStarted"
    case openFrameReady = "OpenFrameReady"
    case noFileIngressChecked = "NoFileIngressChecked"
}

public struct RKWPEnvelope<Payload: Codable>: Codable {
    public let messageId: String
    public let messageType: RKWPMessageType
    public let sessionId: String
    public let sourceAblageId: String
    public let targetAblageId: String
    public let timestamp: String
    public let payload: Payload

    public init(
        messageId: String,
        messageType: RKWPMessageType,
        sessionId: String,
        sourceAblageId: String,
        targetAblageId: String,
        timestamp: String,
        payload: Payload
    ) {
        self.messageId = messageId
        self.messageType = messageType
        self.sessionId = sessionId
        self.sourceAblageId = sourceAblageId
        self.targetAblageId = targetAblageId
        self.timestamp = timestamp
        self.payload = payload
    }
}

public struct FrameCapsulePayload: Codable {
    public let capsuleId: String
    public let thingId: String
    public let displayName: String
    public let frameSessionId: String
    public let leaseId: String
    public let noFileIngress: Bool
    public let containsOriginalFileBytes: Bool
    public let hasOriginalPath: Bool
}

public struct OpenFramePayload: Codable {
    public let frameSessionId: String
    public let leaseId: String
    public let thingId: String
    public let displayName: String
    public let viewerName: String
    public let page: Int
    public let zoom: Double
    public let noFileIngress: Bool
    public let containsOriginalFileBytes: Bool
    public let hasOriginalPath: Bool
}

public struct NoFileIngressResult: Codable {
    public let guestHasPdfFile: Bool
    public let guestHasOriginalPath: Bool
    public let guestHasCopiedPdfBytes: Bool
    public let frameCache: String
    public let sandboxContainsOriginal: Bool
    public let result: String

    public var isSuccessful: Bool {
        !guestHasPdfFile &&
            !guestHasOriginalPath &&
            !guestHasCopiedPdfBytes &&
            frameCache == "MemoryOnly" &&
            !sandboxContainsOriginal &&
            result == "SUCCESS"
    }
}

public struct ReturnPayload: Codable {
    public let leaseId: String
    public let frameSessionId: String
    public let guestKeptOriginalFile: Bool
}
