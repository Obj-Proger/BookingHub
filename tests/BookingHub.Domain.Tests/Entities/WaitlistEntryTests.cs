using BookingHub.Domain.Tests.TestDoubles;

namespace BookingHub.Domain.Tests.Entities;

public class WaitlistEntryTests
{
    // Create

    [Fact]
    public void Create_ValidData_SucceedsWithWaitingStatus()
    {
        var result = WaitlistEntryFixture.CreateWaitingResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WaitlistEntryStatus.Waiting);
    }

    [Fact]
    public void Create_NullEmployeeId_SucceedsMeaningAnyEmployee()
    {
        var result = WaitlistEntryFixture.CreateWaitingResult(employeeId: null);

        result.IsSuccess.Should().BeTrue();
        result.Value.EmployeeId.Should().BeNull();
    }

    [Fact]
    public void Create_SpecificEmployeeId_SetsEmployeeId()
    {
        var employeeId = Guid.CreateVersion7();

        var result = WaitlistEntryFixture.CreateWaitingResult(employeeId: employeeId);

        result.Value.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public void Create_EmptyEmployeeId_FailsWithValidationError()
    {
        var result = WaitlistEntryFixture.CreateWaitingResult(employeeId: Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_EmptyOrganizationId_FailsWithValidationError()
    {
        var result = WaitlistEntry.Create(
            Guid.Empty, WaitlistEntryFixture.LocationId, null, WaitlistEntryFixture.ServiceId,
            WaitlistEntryFixture.ClientContact, WaitlistEntryFixture.DesiredWindow, WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_DesiredWindowInPast_FailsWithSlotInPastError()
    {
        var pastWindow = TimeSlot.Create(WaitlistEntryFixture.UtcNow.AddHours(-2), WaitlistEntryFixture.UtcNow.AddHours(-1)).Value;

        var result = WaitlistEntryFixture.CreateWaitingResult(desiredWindow: pastWindow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.SlotInPast);
    }

    [Fact]
    public void Create_ValidData_GeneratesManagementToken()
    {
        var result = WaitlistEntryFixture.CreateWaitingResult();

        result.Value.ManagementToken.Value.Should().NotBeNullOrEmpty();
    }

    // Offer

    [Fact]
    public void Offer_FromWaitingWithSlotInsideDesiredWindow_Succeeds()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();
        var offeredEmployeeId = Guid.CreateVersion7();

        var result = entry.Offer(
            offeredEmployeeId, WaitlistEntryFixture.OfferedSlotWithinWindow, WaitlistEntryFixture.UtcNow.AddMinutes(30), WaitlistEntryFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Offered);
        entry.OfferedEmployeeId.Should().Be(offeredEmployeeId);
        entry.OfferedSlot.Should().Be(WaitlistEntryFixture.OfferedSlotWithinWindow);
    }

    [Fact]
    public void Offer_FromWaiting_RaisesWaitlistSlotOfferedEvent()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();

        entry.Offer(Guid.CreateVersion7(), WaitlistEntryFixture.OfferedSlotWithinWindow, WaitlistEntryFixture.UtcNow.AddMinutes(30), WaitlistEntryFixture.UtcNow);

        entry.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<WaitlistSlotOfferedEvent>();
    }

    [Fact]
    public void Offer_AlreadyOffered_FailsWithCannotOfferError()
    {
        var entry = WaitlistEntryFixture.CreateOffered();

        var result = entry.Offer(Guid.CreateVersion7(), WaitlistEntryFixture.OfferedSlotWithinWindow, WaitlistEntryFixture.UtcNow.AddMinutes(30), WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.CannotOffer);
    }

    [Fact]
    public void Offer_EmptyEmployeeId_FailsWithValidationError()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();

        var result = entry.Offer(Guid.Empty, WaitlistEntryFixture.OfferedSlotWithinWindow, WaitlistEntryFixture.UtcNow.AddMinutes(30), WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Offer_SlotInPast_FailsWithSlotInPastError()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();
        var laterUtcNow = WaitlistEntryFixture.OfferedSlotWithinWindow.StartUtc.AddMinutes(1);

        var result = entry.Offer(Guid.CreateVersion7(), WaitlistEntryFixture.OfferedSlotWithinWindow, laterUtcNow.AddMinutes(30), laterUtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.SlotInPast);
    }

    [Fact]
    public void Offer_SlotOutsideDesiredWindow_FailsWithOfferOutsideDesiredWindowError()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();
        var nextDaySlot = TimeSlot.Create(
            new DateTime(2026, 3, 11, 10, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 11, 11, 0, 0, DateTimeKind.Utc)).Value;

        var result = entry.Offer(Guid.CreateVersion7(), nextDaySlot, WaitlistEntryFixture.UtcNow.AddMinutes(30), WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.OfferOutsideDesiredWindow);
    }

    // Convert

    [Fact]
    public void Convert_FromOffered_Succeeds()
    {
        var entry = WaitlistEntryFixture.CreateOffered();

        var result = entry.Convert(WaitlistEntryFixture.UtcNow.AddMinutes(10));

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Converted);
    }

    [Fact]
    public void Convert_NotOffered_FailsWithCannotConvertError()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();

        var result = entry.Convert(WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.CannotConvert);
    }

    // Expire

    [Fact]
    public void Expire_AfterOfferWindowElapsed_Succeeds()
    {
        var entry = WaitlistEntryFixture.CreateOffered();

        var result = entry.Expire(WaitlistEntryFixture.UtcNow.AddMinutes(31));

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Expired);
    }

    [Fact]
    public void Expire_AfterOfferWindowElapsed_RaisesWaitlistOfferExpiredEvent()
    {
        var entry = WaitlistEntryFixture.CreateOffered();

        entry.Expire(WaitlistEntryFixture.UtcNow.AddMinutes(31));

        entry.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<WaitlistOfferExpiredEvent>();
    }

    [Fact]
    public void Expire_OfferWindowNotYetElapsed_FailsWithOfferNotYetExpiredError()
    {
        var entry = WaitlistEntryFixture.CreateOffered();

        var result = entry.Expire(WaitlistEntryFixture.UtcNow.AddMinutes(10));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.OfferNotYetExpired);
    }

    [Fact]
    public void Expire_NotOffered_FailsWithCannotExpireError()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();

        var result = entry.Expire(WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.CannotExpire);
    }

    // Cancel

    [Fact]
    public void Cancel_FromWaiting_Succeeds()
    {
        var entry = WaitlistEntryFixture.CreateWaiting();

        var result = entry.Cancel(WaitlistEntryFixture.UtcNow);

        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitlistEntryStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyOffered_FailsWithCannotCancelError()
    {
        var entry = WaitlistEntryFixture.CreateOffered();

        var result = entry.Cancel(WaitlistEntryFixture.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.CannotCancel);
    }
}