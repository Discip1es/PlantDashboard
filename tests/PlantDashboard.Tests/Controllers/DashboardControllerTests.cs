using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PlantDashboard.Controllers;
using PlantDashboard.Models;

namespace PlantDashboard.Tests.Controllers;

public class DashboardControllerTests
{
    [Fact]
    public void Index_Returns_ViewResult_With_OptimalConfig_In_ViewBag()
    {
        // Arrange
        var controller = new DashboardController();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData["OptimalConfig"].Should().BeOfType<PlantRoomConfig>();
        var config = viewResult.ViewData["OptimalConfig"] as PlantRoomConfig;
        config.TempOptimal.Should().Be(24);
    }
}
