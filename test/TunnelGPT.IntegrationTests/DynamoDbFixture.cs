using Docker.DotNet;
using Docker.DotNet.Models;

namespace TunnelGPT.IntegrationTests;

public class DynamoDbFixture : IAsyncLifetime
{
    private readonly DockerClient _dockerClient;
    private string? _containerId;

    public DynamoDbFixture()
    {
        _dockerClient = new DockerClientConfiguration().CreateClient();
    }
    
    public async Task InitializeAsync()
    {
        const string imageName = "amazon/dynamodb-local";
        const string imageTag = "2.5.4";
        const string containerName = "tunnelgpt-test-dynamodb-local";
        
        // Pull the image
        await _dockerClient.Images.CreateImageAsync(
            new ImagesCreateParameters { FromImage = imageName, Tag = imageTag },
            null,
            new Progress<JSONMessage>(m => Console.WriteLine($"{m.Status}"))
        );
        
        // Create and start the container
        Task<CreateContainerResponse>? response =
            _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = imageName,
                Name = containerName,
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "8000/tcp", new List<PortBinding> { new() { HostPort = "8000" } } }
                    }
                }
            });
        _containerId = (await response).ID;
        await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        Console.WriteLine("DynamoDB Local container is started.");
    }

    public async Task DisposeAsync()
    {
        if (!string.IsNullOrEmpty(_containerId))
        {
            await _dockerClient.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
            await _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters { Force = true });
            Console.WriteLine("DynamoDB Local container is stopped and removed.");
        }
    }
}