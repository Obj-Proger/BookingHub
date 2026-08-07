namespace BookingHub.Application.Features.Reviews.DTOs;

public sealed record ReviewResponse(Guid ReviewId, int Rating, string? Comment);