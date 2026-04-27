namespace PlantDashboard.Models;

public class EnvironmentData
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }      // °C
    public double Humidity { get; set; }         // % RH
    public double SoilMoisture { get; set; }     // %
    public double LightIntensity { get; set; }   // lux (x1000)
    public double Co2Level { get; set; }         // ppm
    public double Pressure { get; set; }         // hPa
}