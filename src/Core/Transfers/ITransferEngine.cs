namespace RKWorkspace.Core.Transfers;

/// <summary>
/// Defines platform-neutral logical transfer planning and execution operations.
/// </summary>
public interface ITransferEngine
{
    /// <summary>
    /// Creates a transfer plan from a request.
    /// </summary>
    /// <param name="request">The transfer request.</param>
    /// <returns>The transfer plan.</returns>
    TransferPlan CreatePlan(TransferRequest request);

    /// <summary>
    /// Validates a transfer plan.
    /// </summary>
    /// <param name="plan">The transfer plan.</param>
    /// <returns>The validated transfer plan.</returns>
    TransferPlan ValidatePlan(TransferPlan plan);

    /// <summary>
    /// Prepares the transfer object for logical transfer.
    /// </summary>
    /// <param name="plan">The transfer plan.</param>
    /// <returns>The prepared transfer plan.</returns>
    TransferPlan PrepareTransfer(TransferPlan plan);

    /// <summary>
    /// Completes a prepared logical transfer.
    /// </summary>
    /// <param name="plan">The transfer plan.</param>
    /// <returns>The transfer result.</returns>
    TransferResult CompleteTransfer(TransferPlan plan);

    /// <summary>
    /// Cancels a logical transfer.
    /// </summary>
    /// <param name="plan">The transfer plan.</param>
    /// <returns>The transfer result.</returns>
    TransferResult CancelTransfer(TransferPlan plan);

    /// <summary>
    /// Marks a logical transfer as failed.
    /// </summary>
    /// <param name="plan">The transfer plan.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>The transfer result.</returns>
    TransferResult FailTransfer(TransferPlan plan, string message);

    /// <summary>
    /// Executes a full logical transfer.
    /// </summary>
    /// <param name="request">The transfer request.</param>
    /// <returns>The transfer result.</returns>
    TransferResult ExecuteLogicalTransfer(TransferRequest request);
}
