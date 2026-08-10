using BookingHub.Application.Common.Behaviors;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Tests.TestDoubles;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Tests.Common.Behaviors;

public class AuthorizationBehaviorTests
{
    private sealed record BookingAccessTestRequest(Guid OrganizationId, Guid LocationId, Guid EmployeeId)
        : IRequest<Result>, IRequireBookingAccess;

    private readonly IOrganizationMemberRepository _memberRepository = Substitute.For<IOrganizationMemberRepository>();
    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();

    private AuthorizationBehavior<TRequest, Result> CreateSut<TRequest>() where TRequest : IRequest<Result> =>
        new(new FakeCurrentUser(UserId), _memberRepository);

    private static RequestHandlerDelegate<Result> NextReturningSuccess() => () => Task.FromResult(Result.Success());

    [Fact]
    public async Task Handle_RequestWithoutOrganizationScope_SkipsAuthorizationEntirely()
    {
        var sut = CreateSut<UnscopedTestRequest>();

        var result = await sut.Handle(new UnscopedTestRequest(), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _memberRepository.DidNotReceive().GetByOrganizationAndUserAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallerNotAMember_FailsWithNotAMemberError()
    {
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>())
            .Returns((OrganizationMember?)null);
        var sut = CreateSut<OrgScopedTestRequest>();

        var result = await sut.Handle(new OrgScopedTestRequest(OrganizationId), NextReturningSuccess(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Authorization.NotAMember);
    }

    [Theory]
    [InlineData(OrganizationRole.Owner)]
    [InlineData(OrganizationRole.Administrator)]
    [InlineData(OrganizationRole.Employee)]
    public async Task Handle_MembershipOnlyRequest_AnyRoleSucceeds(OrganizationRole role)
    {
        var employeeId = role == OrganizationRole.Employee ? Guid.CreateVersion7() : (Guid?)null;
        var member = OrganizationMember.Create(OrganizationId, UserId, role, employeeId: employeeId).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<OrgScopedTestRequest>();

        var result = await sut.Handle(new OrgScopedTestRequest(OrganizationId), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(OrganizationRole.Owner, true)]
    [InlineData(OrganizationRole.Administrator, true)]
    [InlineData(OrganizationRole.Employee, false)]
    public async Task Handle_OrganizationManagementRequest_OnlyOwnerOrAdministratorSucceed(OrganizationRole role, bool expectedSuccess)
    {
        var employeeId = role == OrganizationRole.Employee ? Guid.CreateVersion7() : (Guid?)null;
        var member = OrganizationMember.Create(OrganizationId, UserId, role, employeeId: employeeId).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<OrgManagementTestRequest>();

        var result = await sut.Handle(new OrgManagementTestRequest(OrganizationId), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().Be(expectedSuccess);
        if (!expectedSuccess)
            result.Error.Should().Be(ApplicationErrors.Authorization.InsufficientRole);
    }

    [Fact]
    public async Task Handle_LocationManagementRequest_OrgWideManagerSucceedsRegardlessOfLocation()
    {
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Administrator).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<LocationManagementTestRequest>();

        var result = await sut.Handle(new LocationManagementTestRequest(OrganizationId, LocationId), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LocationManagementRequest_LocationManagerScopedToThisLocation_Succeeds()
    {
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.LocationManager, LocationId).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<LocationManagementTestRequest>();

        var result = await sut.Handle(new LocationManagementTestRequest(OrganizationId, LocationId), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_LocationManagementRequest_LocationManagerScopedToDifferentLocation_FailsWithInsufficientRoleError()
    {
        var otherLocationId = Guid.CreateVersion7();
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.LocationManager, otherLocationId).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<LocationManagementTestRequest>();

        var result = await sut.Handle(new LocationManagementTestRequest(OrganizationId, LocationId), NextReturningSuccess(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Authorization.InsufficientRole);
    }

    [Fact]
    public async Task Handle_LocationManagementRequest_PlainEmployee_FailsWithInsufficientRoleError()
    {
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Employee, employeeId: Guid.CreateVersion7()).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<LocationManagementTestRequest>();

        var result = await sut.Handle(new LocationManagementTestRequest(OrganizationId, LocationId), NextReturningSuccess(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Authorization.InsufficientRole);
    }

    [Fact]
    public async Task Handle_BookingAccessRequest_OwningEmployeeSucceeds()
    {
        var employeeId = Guid.CreateVersion7();
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Employee, employeeId: employeeId).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<BookingAccessTestRequest>();

        var result = await sut.Handle(new BookingAccessTestRequest(OrganizationId, LocationId, employeeId), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_BookingAccessRequest_DifferentEmployee_FailsWithInsufficientRoleError()
    {
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Employee, employeeId: Guid.CreateVersion7()).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<BookingAccessTestRequest>();

        var result = await sut.Handle(
            new BookingAccessTestRequest(OrganizationId, LocationId, Guid.CreateVersion7()), NextReturningSuccess(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Authorization.InsufficientRole);
    }

    [Fact]
    public async Task Handle_BookingAccessRequest_LocationManagerScopedToThisLocation_Succeeds()
    {
        var member = OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.LocationManager, locationId: LocationId).Value;
        _memberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut<BookingAccessTestRequest>();

        var result = await sut.Handle(
            new BookingAccessTestRequest(OrganizationId, LocationId, Guid.CreateVersion7()), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}