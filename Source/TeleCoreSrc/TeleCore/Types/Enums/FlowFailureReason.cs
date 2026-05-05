namespace TeleCore.Types.Enums;

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