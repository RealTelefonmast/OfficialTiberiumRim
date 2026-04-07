namespace TeleCore.FlowCore;

public enum FlowFailureReason
{
    None,
    TransferOverflow,
    TransferUnderflow,
    TriedToAddToFull,
    TriedToRemoveEmptyValue,
    TriedToConsumeMoreThanExists,
    UsedForbiddenValueDef,
    IllegalState
}