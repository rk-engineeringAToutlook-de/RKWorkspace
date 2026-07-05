namespace RKWorkspace.Protocol;

public static class RkwpMessageValidator
{
    public static IReadOnlyList<RkwpError> Validate(RkwpMessage message, bool developmentMode = false)
    {
        var errors = new List<RkwpError>();
        Required(message.MessageId, nameof(message.MessageId), errors);
        Required(message.SessionId, nameof(message.SessionId), errors);
        Required(message.SourceAblageId, nameof(message.SourceAblageId), errors);
        Required(message.TargetAblageId, nameof(message.TargetAblageId), errors);
        Required(message.Nonce, nameof(message.Nonce), errors);

        if (message.ProtocolVersion.Major < 0 || message.ProtocolVersion.Minor < 0)
        {
            errors.Add(new RkwpError("ProtocolVersionInvalid", "ProtocolVersion must be present.", false));
        }

        if (message.Timestamp == default && !developmentMode)
        {
            errors.Add(new RkwpError("TimestampMissing", "Timestamp is required.", false));
        }

        if (message.SequenceNumber <= 0 && !developmentMode)
        {
            errors.Add(new RkwpError("SequenceNumberInvalid", "SequenceNumber must be positive.", false));
        }

        if (message.MessageType is RkwpMessageType.CarryLeaseHeartbeat or RkwpMessageType.CarryLeaseReturn or RkwpMessageType.CarryLeaseRevoked &&
            string.IsNullOrWhiteSpace(message.LeaseId) &&
            !developmentMode)
        {
            errors.Add(new RkwpError("LeaseIdMissing", "Lease-bound messages require LeaseId.", false));
        }

        return errors;
    }

    public static void ThrowIfInvalid(RkwpMessage message, bool developmentMode = false)
    {
        var errors = Validate(message, developmentMode);
        if (errors.Count > 0)
        {
            throw new RkwpProtocolException(string.Join("; ", errors.Select(error => $"{error.Code}: {error.Message}")));
        }
    }

    private static void Required(string? value, string fieldName, ICollection<RkwpError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new RkwpError($"{fieldName}Missing", $"{fieldName} is required.", false));
        }
    }
}
