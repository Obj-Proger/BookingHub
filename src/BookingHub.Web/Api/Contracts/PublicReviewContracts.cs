namespace BookingHub.Web.Api.Contracts;

public sealed record SubmitReviewRequest(string? Token, int Rating, string? Comment);

public sealed record ReviewSubmittedResponse(Guid ReviewId);