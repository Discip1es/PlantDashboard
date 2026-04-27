using FluentAssertions;
using PlantDashboard.Models;

namespace PlantDashboard.Tests.Models;

public class PlantRoomConfigTests
{
    [Fact]
    public void PlantRoomConfig_Should_HaveDefaultValues()
    {
        // Arrange & Act
        var config = new PlantRoomConfig();

        // Assert
        config.TempMin.Should().Be(18);
        config.TempOptimal.Should().Be(24);
        config.TempMax.Should().Be(30);
        config.HumidityMin.Should().Be(50);
        config.HumidityOptimal.Should().Be(65);
        config.HumidityMax.Should().Be(80);
        config.SoilMoistureMin.Should().Be(40);
        config.SoilMoistureOptimal.Should().Be(65);
        config.SoilMoistureMax.Should().Be(85);
    }

    [Fact]
    public void PlantRoomConfig_Should_AllowModifyingValues()
    {
        // Arrange
        var config = new PlantRoomConfig();

        // Act
        config.TempOptimal = 25;
        config.HumidityOptimal = 70;

        // Assert
        config.TempOptimal.Should().Be(25);
        config.HumidityOptimal.Should().Be(70);
    }
}
