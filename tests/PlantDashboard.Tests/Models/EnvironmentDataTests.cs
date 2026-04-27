using FluentAssertions;
using PlantDashboard.Models;

namespace PlantDashboard.Tests.Models;

public class EnvironmentDataTests
{
    [Fact]
    public void EnvironmentData_Should_SetAndGetProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var data = new EnvironmentData();

        // Act
        data.Id = 42;
        data.Timestamp = now;
        data.Temperature = 23.5;
        data.Humidity = 65.2;
        data.SoilMoisture = 70.0;
        data.LightIntensity = 35.5;
        data.Co2Level = 450;
        data.Pressure = 1015.3;

        // Assert
        data.Id.Should().Be(42);
        data.Timestamp.Should().Be(now);
        data.Temperature.Should().Be(23.5);
        data.Humidity.Should().Be(65.2);
        data.SoilMoisture.Should().Be(70.0);
        data.LightIntensity.Should().Be(35.5);
        data.Co2Level.Should().Be(450);
        data.Pressure.Should().Be(1015.3);
    }
}
