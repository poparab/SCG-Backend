namespace SCG.InquiryManagement.Domain.Enums;

public enum BatchStatus
{
    Draft = 0,
    PendingReview = 1,
    PendingPayment = 2,
    Processing = 3,
    Completed = 4,
    PartiallyCompleted = 5,
    Failed = 6
}
