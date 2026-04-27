using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PlantDashboard.Hubs;
using PlantDashboard.Models;

namespace PlantDashboard.Services
{
    public class SensorSimulatorService : BackgroundService
    {
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly ILogger<SensorSimulatorService> _logger;
        private readonly ITimeProvider _timeProvider;
        private readonly Random _random;
        private EnvironmentData _currentData;
        private readonly PlantRoomConfig _config;

        private double _trendTemperature;
        private double _trendHumidity;
        private double _trendSoilMoisture;

        /// <summary>
        /// Указывает, работает ли основной цикл симуляции.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        public SensorSimulatorService(
            IHubContext<SensorHub> hubContext,
            ILogger<SensorSimulatorService> logger,
            ITimeProvider timeProvider)
        {
            _hubContext = hubContext;
            _logger = logger;
            _timeProvider = timeProvider;
            _random = new Random();
            _config = new PlantRoomConfig();

            _currentData = new EnvironmentData
            {
                Timestamp = _timeProvider.Now,
                Temperature = 23.5,
                Humidity = 60.0,
                SoilMoisture = 65.0,
                LightIntensity = 5.0,
                Co2Level = 420,
                Pressure = 1013.0
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IsRunning = true;
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        GenerateNextReading();
                        await _hubContext.Clients.All.SendCoreAsync(
                            "ReceiveSensorData",
                            new object[] { _currentData },
                            stoppingToken);

                        _logger.LogDebug("Sent sensor update: Temp={Temperature}°C, Humidity={Humidity}%",
                            _currentData.Temperature, _currentData.Humidity);

                        await Task.Delay(2000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in sensor simulation loop");
                        await Task.Delay(5000, stoppingToken);
                    }
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        internal void GenerateNextReading()
        {
            int hourOfDay = _timeProvider.HourOfDay;

            _trendTemperature += (_random.NextDouble() - 0.5) * 0.3;
            _trendHumidity += (_random.NextDouble() - 0.5) * 0.4;
            _trendSoilMoisture += (_random.NextDouble() - 0.5) * 0.2;

            _trendTemperature = Math.Clamp(_trendTemperature, -2.0, 2.0);
            _trendHumidity = Math.Clamp(_trendHumidity, -3.0, 3.0);
            _trendSoilMoisture = Math.Clamp(_trendSoilMoisture, -1.5, 1.5);

            double dayTempCycle = Math.Sin((hourOfDay - 6) * Math.PI / 12) * 3.0;
            double newTemp = _config.TempOptimal + dayTempCycle + _trendTemperature;
            newTemp += (_random.NextDouble() - 0.5) * 0.5;
            _currentData.Temperature = Math.Round(Math.Clamp(newTemp, 16.0, 34.0), 1);

            double humidityBase = 70.0 - (_currentData.Temperature - _config.TempOptimal) * 1.2;
            double newHumidity = humidityBase + _trendHumidity + (_random.NextDouble() - 0.5) * 2.0;
            _currentData.Humidity = Math.Round(Math.Clamp(newHumidity, 30.0, 95.0), 1);

            double soilDrift = -0.05 + (_random.NextDouble() - 0.5) * 0.1;
            double newSoil = _currentData.SoilMoisture + soilDrift + _trendSoilMoisture * 0.1;

            if (_random.NextDouble() < 0.02 || newSoil < 35.0)
            {
                newSoil += 15.0 + _random.NextDouble() * 10.0;
                _logger.LogInformation("💧 Irrigation event simulated. Soil moisture increased.");
            }
            _currentData.SoilMoisture = Math.Round(Math.Clamp(newSoil, 20.0, 90.0), 1);

            double light;
            if (hourOfDay >= 6 && hourOfDay <= 20)
            {
                light = 50_000.0 * Math.Sin((hourOfDay - 6) * Math.PI / 14.0);
                light += (_random.NextDouble() - 0.5) * 3000.0;
                light = Math.Max(0, light);
            }
            else
            {
                light = _random.NextDouble() * 200.0;
            }
            _currentData.LightIntensity = Math.Round(Math.Clamp(light / 1000.0, 0.0, 45.0), 1);

            double co2Base = 410.0;
            if (hourOfDay >= 8 && hourOfDay <= 18)
                co2Base += 30.0 + _random.NextDouble() * 20.0;
            if (hourOfDay >= 19 && hourOfDay <= 22)
                co2Base += 50.0 + _random.NextDouble() * 30.0;
            _currentData.Co2Level = Math.Round(co2Base + (_random.NextDouble() - 0.5) * 15.0, 0);

            double pressureTrend = Math.Sin(DateTime.UtcNow.Ticks / 10_000_000_000.0) * 5.0;
            _currentData.Pressure = Math.Round(1013.0 + pressureTrend + (_random.NextDouble() - 0.5) * 2.0, 1);

            _currentData.Timestamp = _timeProvider.Now;
        }
    }
}