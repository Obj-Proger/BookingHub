namespace BookingHub.API.Controllers.Reviews;

public sealed record SubmitReviewRequest(string? Token, int Rating, string? Comment);