using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using PlantDashboard.Hubs;

namespace PlantDashboard.Tests.Hubs;

public class SensorHubTests
{
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly SensorHub _hub;

    public SensorHubTests()
    {
        _mockClients = new Mock<IHubCallerClients>();
        _mockGroups = new Mock<IGroupManager>();
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.ConnectionId).Returns("test-connection");

        _hub = new SensorHub();
        _hub.Clients = _mockClients.Object;
        _hub.Groups = _mockGroups.Object;
        _hub.Context = mockContext.Object;
    }

    [Fact]
    public async Task JoinRoom_Should_AddConnectionToGroup()
    {
        // Arrange
        const string roomId = "room1";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinRoom(roomId);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection", roomId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LeaveRoom_Should_RemoveConnectionFromGroup()
    {
        // Arrange
        const string roomId = "room1";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveRoom(roomId);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync("test-connection", roomId, It.IsAny<CancellationToken>()), Times.Once);
    }
}