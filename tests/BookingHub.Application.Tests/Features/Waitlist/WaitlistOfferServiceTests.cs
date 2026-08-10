using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;
using MockQueryable;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class WaitlistOfferServiceTests
{
    private readonly IWaitlistEntryRepository _waitlistEntryRepository = Substitute.For<IWaitlistEntryRepository>();
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();
    private static readonly Guid ServiceId = Guid.CreateVersion7();
    private static readonly TimeSlot FreedSlot = TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1)).Value;

    private WaitlistOfferService CreateSut() => new(_waitlistEntryRepository, _dbContext, _unitOfWork);

    private static WaitlistEntry CreateWaitingEntry(Guid organizationId) => WaitlistEntry.Create(
        organizationId, LocationId, null, ServiceId,
        ClientContact.Create(PhoneNumber.Create("+14155552671").Value), FreedSlot, DateTime.UtcNow).Value;

    private Organization SetUpOrganization(TimeSpan offerWindow)
    {
        var organization = Organization.Create("Name", "slug").Value;
        organization.UpdateWaitlistOfferWindow(offerWindow);
        _dbContext.Organizations.Returns(new[] { organization }.ToList().BuildMock());
        return organization;
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_NoCandidates_DoesNothing()
    {
        var organizationId = Guid.CreateVersion7();
        _waitlistEntryRepository.GetWaitingCandidatesAsync(organizationId, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([]);
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(organizationId, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_OneCandidate_OffersAndSaves()
    {
        var organization = SetUpOrganization(TimeSpan.FromHours(2));
        var entry = CreateWaitingEntry(organization.Id);
        _waitlistEntryRepository.GetWaitingCandidatesAsync(organization.Id, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([entry]);
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(organization.Id, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        entry.Status.Should().Be(WaitlistEntryStatus.Offered);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_FirstCandidateCannotBeOffered_FallsThroughToNext()
    {
        var organization = SetUpOrganization(TimeSpan.FromHours(2));
        var firstEntry = CreateWaitingEntry(organization.Id);
        firstEntry.Cancel(DateTime.UtcNow); // no longer Waiting — Offer() will fail
        var secondEntry = CreateWaitingEntry(organization.Id);
        _waitlistEntryRepository.GetWaitingCandidatesAsync(organization.Id, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([firstEntry, secondEntry]);
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(organization.Id, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        secondEntry.Status.Should().Be(WaitlistEntryStatus.Offered);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryOfferFreedSlotAsync_AllCandidatesFail_DoesNotSave()
    {
        var organization = SetUpOrganization(TimeSpan.FromHours(2));
        var entry = CreateWaitingEntry(organization.Id);
        entry.Cancel(DateTime.UtcNow);
        _waitlistEntryRepository.GetWaitingCandidatesAsync(organization.Id, LocationId, ServiceId, EmployeeId, FreedSlot, Arg.Any<CancellationToken>())
            .Returns([entry]);
        var sut = CreateSut();

        await sut.TryOfferFreedSlotAsync(organization.Id, LocationId, EmployeeId, ServiceId, FreedSlot, CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}