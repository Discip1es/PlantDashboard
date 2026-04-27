namespace PlantDashboard.Services
{
    /// <summary>
    /// Абстракция для получения текущего времени и часа дня.
    /// Используется для тестирования симуляции датчиков.
    /// </summary>
    public interface ITimeProvider
    {
        DateTime Now { get; }
        int HourOfDay { get; }
    }

    /// <summary>
    /// Реальная реализация, возвращающая системное время.
    /// </summary>
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
        public int HourOfDay => DateTime.Now.Hour;
    }
}