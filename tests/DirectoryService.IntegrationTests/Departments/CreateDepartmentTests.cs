using DirectoryService.Application.UseCases.DepartmentCases.Commands.CreateDepartment;
using DirectoryService.Contracts.DepartmentContracts;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{ 
    public CreateDepartmentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateDepartment_WithValidData_ShouldSucceed()
    {
        // Arrange
        Guid locationId = await CreateLocation("location 1");
        
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest(
                "Подразделение", "zxc", null, [locationId]));

            return sut.HandleAsync(command, cancellationToken);
        });

        // Assert
        // хотим убедиться, что данные реально есть в бд
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstOrDefaultAsync(
                d => d.Id == result.Value.Id, cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, result.Value.Id);

            Assert.True(result.IsSuccess); // True() - проверяет, тру ли аргумент.
            Assert.NotEqual(Guid.Empty, result.Value.Id); // проверяет, что result.Value != Guid.Empty
        });
    }
    
    private async Task<Guid> CreateLocation(string locationName)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var location = Location.Create(
                Guid.NewGuid(),
                locationName,
                Address.Create(country: "Каракалпакстан", city: "Нукус", street: "Каракалпакстан", building: "1").Value,
                Timezone.Create("Asia/Tashkent").Value,
                DateTimeOffset.UtcNow);

            dbContext.Locations.Add(location.Value);
            await dbContext.SaveChangesAsync();

            return location.Value.Id;
        });
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentCommandHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentCommandHandler>();

        return await action(sut);
    }
}