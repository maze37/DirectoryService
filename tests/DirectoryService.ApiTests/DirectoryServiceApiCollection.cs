using Xunit;

namespace DirectoryService.ApiTests;

[CollectionDefinition(Name)]
public sealed class DirectoryServiceApiCollection : ICollectionFixture<DockerComposeFixture>
{
    public const string Name = "directory-service-api";
}
