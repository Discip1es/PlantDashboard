namespace PlantDashboard.Models;

public class PlantRoomConfig
{
    public double TempMin { get; set; } = 18;
    public double TempOptimal { get; set; } = 24;
    public double TempMax { get; set; } = 30;

    public double HumidityMin { get; set; } = 50;
    public double HumidityOptimal { get; set; } = 65;
    public double HumidityMax { get; set; } = 80;

    public double SoilMoistureMin { get; set; } = 40;
    public double SoilMoistureOptimal { get; set; } = 65;
    public double SoilMoistureMax { get; set; } = 85;
}
