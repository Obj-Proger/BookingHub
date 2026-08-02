namespace BookingHub.Domain.Tests.Entities;

public class EmployeeLocationAssignmentTests
{
    [Fact]
    public void Create_ValidIds_SucceedsAndIsActiveByDefault()
    {
        var result = EmployeeLocationAssignment.Create(Guid.CreateVersion7(), Guid.CreateVersion7());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyEmployeeId_FailsWithValidationError()
    {
        var result = EmployeeLocationAssignment.Create(Guid.Empty, Guid.CreateVersion7());

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_EmptyLocationId_FailsWithValidationError()
    {
        var result = EmployeeLocationAssignment.Create(Guid.CreateVersion7(), Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var assignment = EmployeeLocationAssignment.Create(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;

        assignment.Deactivate();

        assignment.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_SetsIsActiveToTrue()
    {
        var assignment = EmployeeLocationAssignment.Create(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;
        assignment.Deactivate();

        assignment.Reactivate();

        assignment.IsActive.Should().BeTrue();
    }
}