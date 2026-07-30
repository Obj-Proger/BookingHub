using BookingHub.Domain.Events;
using BookingHub.Domain.Tests.TestDoubles;

namespace BookingHub.Domain.Tests.Entities;

public class BookingTests
{
    // CreatePending

    [Fact]
    public void CreatePending_ValidData_SucceedsWithPendingStatus()
    {
        var result = BookingFixture.CreatePendingResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BookingStatus.Pending);
        result.Value.Source.Should().Be(BookingSource.Public);
    }

    [Fact]
    public void CreatePending_ValidData_RaisesBookingCreatedEvent()
    {
        var booking = BookingFixture.CreatePending();

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BookingCreatedEvent>();
    }

    [Fact]
    public void CreatePending_EmptyOrganizationId_FailsWithValidationError()
    {
        var result = Booking.CreatePending(
            Guid.Empty, BookingFixture.LocationId, BookingFixture.EmployeeId, BookingFixture.ServiceId,
            BookingFixture.ClientContact, BookingFixture.FutureSlot, BookingSource.Public, BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreatePending_EmptyLocationId_FailsWithValidationError()
    {
        var result = Booking.CreatePending(
            BookingFixture.OrganizationId, Guid.Empty, BookingFixture.EmployeeId, BookingFixture.ServiceId,
            BookingFixture.ClientContact, BookingFixture.FutureSlot, BookingSource.Public, BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreatePending_EmptyEmployeeId_FailsWithValidationError()
    {
        var result = Booking.CreatePending(
            BookingFixture.OrganizationId, BookingFixture.LocationId, Guid.Empty, BookingFixture.ServiceId,
            BookingFixture.ClientContact, BookingFixture.FutureSlot, BookingSource.Public, BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreatePending_EmptyServiceId_FailsWithValidationError()
    {
        var result = Booking.CreatePending(
            BookingFixture.OrganizationId, BookingFixture.LocationId, BookingFixture.EmployeeId, Guid.Empty,
            BookingFixture.ClientContact, BookingFixture.FutureSlot, BookingSource.Public, BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreatePending_SlotInPast_FailsWithSlotInPastError()
    {
        var pastSlot = TimeSlot.Create(BookingFixture.UtcNow.AddHours(-2), BookingFixture.UtcNow.AddHours(-1)).Value;

        var result = BookingFixture.CreatePendingResult(pastSlot);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.SlotInPast);
    }

    // Confirm

    [Fact]
    public void Confirm_FromPending_SucceedsAndSetsConfirmedAtUtc()
    {
        var booking = BookingFixture.CreatePending();

        var result = booking.Confirm(BookingFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ConfirmedAtUtc.Should().Be(BookingFixture.UtcNow);
    }

    [Fact]
    public void Confirm_FromPending_RaisesBookingConfirmedEvent()
    {
        var booking = BookingFixture.CreatePending();
        booking.ClearDomainEvents();

        booking.Confirm(BookingFixture.UtcNow);

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BookingConfirmedEvent>();
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_FailsWithCannotConfirmError()
    {
        var booking = BookingFixture.CreateConfirmed();

        var result = booking.Confirm(BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotConfirm);
    }

    // Reschedule

    [Fact]
    public void Reschedule_FromPending_UpdatesTimeSlot()
    {
        var booking = BookingFixture.CreatePending();
        var newSlot = TimeSlot.Create(BookingFixture.FutureSlot.StartUtc.AddDays(1), BookingFixture.FutureSlot.EndUtc.AddDays(1)).Value;

        var result = booking.Reschedule(newSlot, BookingFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        booking.TimeSlot.Should().Be(newSlot);
    }

    [Fact]
    public void Reschedule_FromConfirmed_Succeeds()
    {
        var booking = BookingFixture.CreateConfirmed();
        var newSlot = TimeSlot.Create(BookingFixture.FutureSlot.StartUtc.AddDays(1), BookingFixture.FutureSlot.EndUtc.AddDays(1)).Value;

        var result = booking.Reschedule(newSlot, BookingFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public void Reschedule_ValidNewSlot_RaisesBookingRescheduledEvent()
    {
        var booking = BookingFixture.CreatePending();
        booking.ClearDomainEvents();
        var newSlot = TimeSlot.Create(BookingFixture.FutureSlot.StartUtc.AddDays(1), BookingFixture.FutureSlot.EndUtc.AddDays(1)).Value;

        booking.Reschedule(newSlot, BookingFixture.UtcNow);

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BookingRescheduledEvent>();
    }

    [Fact]
    public void Reschedule_SlotInPast_FailsAndLeavesTimeSlotUnchanged()
    {
        var booking = BookingFixture.CreatePending();
        var pastSlot = TimeSlot.Create(BookingFixture.UtcNow.AddHours(-2), BookingFixture.UtcNow.AddHours(-1)).Value;

        var result = booking.Reschedule(pastSlot, BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.SlotInPast);
        booking.TimeSlot.Should().Be(BookingFixture.FutureSlot);
    }

    [Fact]
    public void Reschedule_AwaitingReview_FailsWithCannotRescheduleError()
    {
        var booking = BookingFixture.CreateAwaitingReview();
        var newSlot = TimeSlot.Create(BookingFixture.FutureSlot.StartUtc.AddDays(1), BookingFixture.FutureSlot.EndUtc.AddDays(1)).Value;

        var result = booking.Reschedule(newSlot, BookingFixture.FutureSlot.EndUtc);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotReschedule);
    }

    // TransitionToAwaitingReview

    [Fact]
    public void TransitionToAwaitingReview_ConfirmedAndSlotEnded_Succeeds()
    {
        var booking = BookingFixture.CreateConfirmed();

        var result = booking.TransitionToAwaitingReview(BookingFixture.FutureSlot.EndUtc);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.AwaitingReview);
    }

    [Fact]
    public void TransitionToAwaitingReview_ConfirmedButSlotNotYetEnded_FailsWithSlotNotYetEndedError()
    {
        var booking = BookingFixture.CreateConfirmed();

        var result = booking.TransitionToAwaitingReview(BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.SlotNotYetEnded);
    }

    [Fact]
    public void TransitionToAwaitingReview_NotConfirmed_FailsWithCannotTransitionError()
    {
        var booking = BookingFixture.CreatePending();

        var result = booking.TransitionToAwaitingReview(BookingFixture.FutureSlot.EndUtc);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotTransitionToAwaitingReview);
    }

    // Complete

    [Fact]
    public void Complete_FromAwaitingReview_SucceedsAndSetsResolvedAtUtc()
    {
        var booking = BookingFixture.CreateAwaitingReview();
        var resolvedAt = BookingFixture.FutureSlot.EndUtc.AddHours(1);

        var result = booking.Complete(resolvedAt);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Completed);
        booking.ResolvedAtUtc.Should().Be(resolvedAt);
    }

    [Fact]
    public void Complete_FromAwaitingReview_RaisesBookingCompletedEvent()
    {
        var booking = BookingFixture.CreateAwaitingReview();

        booking.Complete(BookingFixture.FutureSlot.EndUtc.AddHours(1));

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BookingCompletedEvent>();
    }

    [Fact]
    public void Complete_NotAwaitingReview_FailsWithCannotCompleteError()
    {
        var booking = BookingFixture.CreateConfirmed();

        var result = booking.Complete(BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotComplete);
    }

    // MarkNoShow 

    [Fact]
    public void MarkNoShow_FromAwaitingReview_Succeeds()
    {
        var booking = BookingFixture.CreateAwaitingReview();

        var result = booking.MarkNoShow(BookingFixture.FutureSlot.EndUtc.AddHours(1));

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_FromAwaitingReview_RaisesNoShowRecordedEvent()
    {
        var booking = BookingFixture.CreateAwaitingReview();

        booking.MarkNoShow(BookingFixture.FutureSlot.EndUtc.AddHours(1));

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<NoShowRecordedEvent>();
    }

    [Fact]
    public void MarkNoShow_NotAwaitingReview_FailsWithCannotMarkNoShowError()
    {
        var booking = BookingFixture.CreateConfirmed();

        var result = booking.MarkNoShow(BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotMarkNoShow);
    }

    // Cancel

    [Fact]
    public void Cancel_FromPending_Succeeds()
    {
        var booking = BookingFixture.CreatePending();

        var result = booking.Cancel("Client changed their mind", BookingFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancellationReason.Should().Be("Client changed their mind");
    }

    [Fact]
    public void Cancel_FromConfirmed_RaisesBookingCancelledEvent()
    {
        var booking = BookingFixture.CreateConfirmed();

        booking.Cancel(null, BookingFixture.UtcNow);

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BookingCancelledEvent>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_EmptyOrWhitespaceReason_NormalizesToNull(string? reason)
    {
        var booking = BookingFixture.CreatePending();

        booking.Cancel(reason, BookingFixture.UtcNow);

        booking.CancellationReason.Should().BeNull();
    }

    [Fact]
    public void Cancel_ReasonWithSurroundingWhitespace_IsTrimmed()
    {
        var booking = BookingFixture.CreatePending();

        booking.Cancel("  No longer needed  ", BookingFixture.UtcNow);

        booking.CancellationReason.Should().Be("No longer needed");
    }

    [Fact]
    public void Cancel_AlreadyCancelled_FailsWithCannotCancelError()
    {
        var booking = BookingFixture.CreateCancelled();

        var result = booking.Cancel(null, BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotCancel);
    }

    // Expire

    [Fact]
    public void Expire_FromPending_Succeeds()
    {
        var booking = BookingFixture.CreatePending();

        var result = booking.Expire(BookingFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Expired);
    }

    [Fact]
    public void Expire_FromPending_RaisesBookingExpiredEvent()
    {
        var booking = BookingFixture.CreatePending();
        booking.ClearDomainEvents();

        booking.Expire(BookingFixture.UtcNow);

        booking.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<BookingExpiredEvent>();
    }

    [Fact]
    public void Expire_AlreadyConfirmed_FailsWithCannotExpireError()
    {
        var booking = BookingFixture.CreateConfirmed();

        var result = booking.Expire(BookingFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Booking.CannotExpire);
    }

    // LinkClient

    [Fact]
    public void LinkClient_ValidId_SetsClientId()
    {
        var booking = BookingFixture.CreatePending();
        var clientId = Guid.CreateVersion7();

        var result = booking.LinkClient(clientId);

        result.IsSuccess.Should().BeTrue();
        booking.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void LinkClient_EmptyGuid_FailsWithValidationError()
    {
        var booking = BookingFixture.CreatePending();

        var result = booking.LinkClient(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}