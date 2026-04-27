using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PlantDashboard.Hubs;
using PlantDashboard.Models;
using PlantDashboard.Services;
using System.Reflection;

namespace PlantDashboard.Tests.Services;

public class SensorSimulatorServiceTests
{
    private readonly Mock<IHubContext<SensorHub>> _mockHubContext;
    private readonly Mock<ITimeProvider> _mockTimeProvider;
    private readonly SensorSimulatorService _service;

    public SensorSimulatorServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<SensorHub>>();
        var mockClients = new Mock<IHubClients>();
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _mockTimeProvider = new Mock<ITimeProvider>();
        _mockTimeProvider.Setup(tp => tp.Now).Returns(new DateTime(2025, 3, 15, 12, 0, 0));
        _mockTimeProvider.Setup(tp => tp.HourOfDay).Returns(12);

        _service = new SensorSimulatorService(
            _mockHubContext.Object,
            NullLogger<SensorSimulatorService>.Instance,
            _mockTimeProvider.Object);
    }

    // Helper для вызова приватного метода GenerateNextReading через рефлексию
    private void InvokeGenerateNextReading()
    {
        var method = typeof(SensorSimulatorService)
            .GetMethod("GenerateNextReading", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(_service, null);
    }

    // Получение значения приватного поля _currentData
    private dynamic GetCurrentData()
    {
        var field = typeof(SensorSimulatorService)
            .GetField("_currentData", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(_service);
    }

    [Fact]
    public void GenerateNextReading_Should_UpdateAllPropertiesWithinRealisticRanges()
    {
        // Arrange & Act
        InvokeGenerateNextReading();
        var data = GetCurrentData() as EnvironmentData;

        // Assert
        data.Should().NotBeNull();
        data.Temperature.Should().BeInRange(16, 34);      // общий допустимый диапазон
        data.Humidity.Should().BeInRange(30, 95);
        data.SoilMoisture.Should().BeInRange(20, 90);
        data.LightIntensity.Should().BeInRange(0, 45);    // klx
        data.Co2Level.Should().BeInRange(350, 800);
        data.Pressure.Should().BeInRange(990, 1035);
    }

    [Fact]
    public void GenerateNextReading_Should_ApplyDayNightCycleToLight()
    {
        // Днём (12 часов) - свет должен быть высоким
        _mockTimeProvider.Setup(tp => tp.HourOfDay).Returns(12);
        InvokeGenerateNextReading();
        var dayData = GetCurrentData() as EnvironmentData;
        dayData.LightIntensity.Should().BeGreaterThan(15);

        // Ночью (2 часа) - свет минимальный
        _mockTimeProvider.Setup(tp => tp.HourOfDay).Returns(2);
        InvokeGenerateNextReading();
        var nightData = GetCurrentData() as EnvironmentData;
        nightData.LightIntensity.Should().BeLessThan(1);
    }

    [Fact]
    public void GenerateNextReading_Should_AdjustHumidityInverselyToTemperature()
    {
        // Зафиксируем температуру высокой и низкой через рефлексию
        var dataField = typeof(SensorSimulatorService)
            .GetField("_currentData", BindingFlags.NonPublic | BindingFlags.Instance);
        var current = (EnvironmentData)dataField.GetValue(_service);

        // Устанавливаем высокую температуру
        current.Temperature = 32;
        dataField.SetValue(_service, current);
        InvokeGenerateNextReading();
        var afterHighTemp = (EnvironmentData)dataField.GetValue(_service);
        var humidityHighTemp = afterHighTemp.Humidity;

        // Устанавливаем низкую температуру
        current.Temperature = 18;
        dataField.SetValue(_service, current);
        InvokeGenerateNextReading();
        var afterLowTemp = (EnvironmentData)dataField.GetValue(_service);

        // При низкой температуре влажность должна быть выше
        afterLowTemp.Humidity.Should().BeGreaterThan(humidityHighTemp);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallSendEveryTwoSeconds()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var mockClients = new Mock<IHubClients>();
        var mockSingleClient = new Mock<IClientProxy>();
        mockClients.Setup(c => c.All).Returns(mockSingleClient.Object);
        _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        // Запускаем сервис в фоне
        var executeTask = _service.StartAsync(cts.Token);

        // Ждём немного больше 2 секунд
        await Task.Delay(2500);

        // Останавливаем
        cts.Cancel();
        await executeTask;

        // Проверяем, что SendAsync был вызван хотя бы раз
        mockSingleClient.Verify(
            c => c.SendCoreAsync("ReceiveSensorData", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_Should_NotCrashOnException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        // Симулируем ошибку в методе GenerateNextReading (например, через рефлексию)
        // Но проще: мокируем ITimeProvider так, чтобы он кидал исключение
        _mockTimeProvider.Setup(tp => tp.HourOfDay).Throws(new InvalidOperationException("Simulated failure"));

        var executeTask = _service.StartAsync(cts.Token);

        // Ждём немного
        await Task.Delay(500);
        // Сервис не должен упасть, а должен продолжить (после логирования и ожидания)
        _service.IsRunning.Should().BeTrue(); // добавим свойство IsRunning или проверку, что сервис жив
        cts.Cancel();
        await executeTask;
    }
}
