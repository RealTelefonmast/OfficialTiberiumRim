namespace TeleCore.Unsorted;

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