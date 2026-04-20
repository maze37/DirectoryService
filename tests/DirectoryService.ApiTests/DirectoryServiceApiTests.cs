using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace DirectoryService.ApiTests;

[Collection(DirectoryServiceApiCollection.Name)]
public sealed class DirectoryServiceApiTests
{
    private static readonly Guid MissingLocationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MissingDepartmentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly HttpClient _httpClient;

    public DirectoryServiceApiTests(DockerComposeFixture fixture)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = fixture.BaseAddress,
        };
    }

    [Fact]
    public async Task CreateDepartment_ShouldReturnOk_WhenRequestUsesExistingLocationGuid()
    {
        var locationId = await CreateLocationAsync();

        var request = new
        {
            name = $"QA Department {Guid.NewGuid():N}",
            identifier = BuildLettersOnlyIdentifier("department"),
            parentId = (Guid?)null,
            locationIds = new[] { locationId },
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/departments", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var createdDepartmentId = await ReadEnvelopeResultIdAsync(response);
        Assert.NotEqual(Guid.Empty, createdDepartmentId);
    }

    [Fact]
    public async Task CreateDepartment_ShouldReturnNotFound_WhenRequestUsesMissingLocationGuid()
    {
        var request = new
        {
            name = $"QA Department Missing {Guid.NewGuid():N}",
            identifier = BuildLettersOnlyIdentifier("missing"),
            parentId = (Guid?)null,
            locationIds = new[] { MissingLocationId },
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/departments", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertContainsErrorCodeAsync(response, "record.not.found");
    }

    [Fact]
    public async Task CreatePosition_ShouldReturnOk_WhenRequestUsesExistingDepartmentGuid()
    {
        var locationId = await CreateLocationAsync();
        var departmentId = await CreateDepartmentAsync(locationId);

        var request = new
        {
            name = $"QA Position {Guid.NewGuid():N}",
            description = "API test position created through docker compose stack",
            departmentIds = new[] { departmentId },
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/positions", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var createdPositionId = await ReadEnvelopeResultIdAsync(response);
        Assert.NotEqual(Guid.Empty, createdPositionId);
    }

    [Fact]
    public async Task CreatePosition_ShouldReturnConflict_WhenNameAlreadyExistsAmongActivePositions()
    {
        var locationId = await CreateLocationAsync();
        var firstDepartmentId = await CreateDepartmentAsync(locationId);
        var secondDepartmentId = await CreateDepartmentAsync(locationId);
        var positionName = $"QA Duplicate Position {Guid.NewGuid():N}";

        using var firstResponse = await _httpClient.PostAsJsonAsync("/api/positions", new
        {
            name = positionName,
            description = "First position creation",
            departmentIds = new[] { firstDepartmentId },
        });

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var secondResponse = await _httpClient.PostAsJsonAsync("/api/positions", new
        {
            name = positionName,
            description = "Second position creation should fail",
            departmentIds = new[] { secondDepartmentId },
        });

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        await AssertContainsErrorCodeAsync(secondResponse, "position.name.taken");
    }

    [Fact]
    public async Task CreatePosition_ShouldReturnNotFound_WhenRequestUsesMissingDepartmentGuid()
    {
        var request = new
        {
            name = $"QA Missing Department Position {Guid.NewGuid():N}",
            description = "API test with missing department guid",
            departmentIds = new[] { MissingDepartmentId },
        };

        using var response = await _httpClient.PostAsJsonAsync("/api/positions", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertContainsErrorCodeAsync(response, "record.not.found");
    }

    private async Task<Guid> CreateLocationAsync()
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/locations", new
        {
            name = $"QA Location {Guid.NewGuid():N}",
            address = new
            {
                country = "Russia",
                city = "Moscow",
                street = $"Test Street {Guid.NewGuid():N}",
                building = "10A",
                office = "501",
                postalCode = "101000",
            },
            timezone = "Europe/Moscow",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadEnvelopeResultIdAsync(response);
    }

    private async Task<Guid> CreateDepartmentAsync(Guid locationId)
    {
        using var response = await _httpClient.PostAsJsonAsync("/api/departments", new
        {
            name = $"QA Department {Guid.NewGuid():N}",
            identifier = BuildLettersOnlyIdentifier("qa"),
            parentId = (Guid?)null,
            locationIds = new[] { locationId },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadEnvelopeResultIdAsync(response);
    }

    private static async Task<Guid> ReadEnvelopeResultIdAsync(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var idValue = document.RootElement
            .GetProperty("result")
            .GetProperty("id")
            .GetString();

        return Guid.Parse(idValue!);
    }

    private static async Task AssertContainsErrorCodeAsync(HttpResponseMessage response, string expectedCode)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var codes = document.RootElement
            .GetProperty("errors")
            .EnumerateArray()
            .Select(error => error.GetProperty("code").GetString())
            .ToArray();

        Assert.Contains(expectedCode, codes);
    }

    private static string BuildLettersOnlyIdentifier(string prefix)
    {
        var guidText = Guid.NewGuid().ToString("N");
        var lettersOnly = string.Concat(guidText.Select(MapHexCharToLetter));
        return $"{prefix}{lettersOnly[..12]}";
    }

    private static char MapHexCharToLetter(char value) =>
        value switch
        {
            >= '0' and <= '9' => (char)('a' + (value - '0')),
            >= 'a' and <= 'f' => value,
            >= 'A' and <= 'F' => char.ToLowerInvariant(value),
            _ => 'z',
        };
}
