using Docker.DotNet;
using Docker.DotNet.Models;

namespace TunnelGPT.IntegrationTests;

public class DynamoDbFixture : IAsyncLifetime
{
    private readonly DockerClient _dockerClient = new DockerClientConfiguration().CreateClient();
    private string? _containerId;

    public async Task InitializeAsync()
    {
        const string imageName = "amazon/dynamodb-local";
        const string imageTag = "2.5.4";
        const string containerName = "tunnelgpt-test-dynamodb-local";
        
        await PullImageAsync(imageName, imageTag);
        await StartContainerAsync(imageName, containerName);
    }

    private async Task PullImageAsync(string imageName, string imageTag)
    {
        await _dockerClient.Images.CreateImageAsync(
            new ImagesCreateParameters { FromImage = imageName, Tag = imageTag },
            null,
            new Progress<JSONMessage>(m => Console.WriteLine($"{m.Status}"))
        );
    }

    private async Task StartContainerAsync(string imageName, string containerName)
    {
        Task<CreateContainerResponse>? response =
            _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = imageName,
                Name = containerName,
                Cmd = new List<string> { "-jar", "DynamoDBLocal.jar", "-sharedDb", "-inMemory" },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "8000/tcp", new List<PortBinding> { new() { HostPort = "8000" } } }
                    },
                    Memory = 128 * 1024 * 1024,
                    NanoCPUs = 500_000_000
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