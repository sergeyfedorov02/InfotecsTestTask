namespace InfotecsTestTask.Models.DataTransferObject
{
    /// <summary>
    /// Класс для обработки фильтрации
    /// </summary>
    public class ResultFilterDto
    {
        public string? FileName { get; set; } // по имени файла
        public DateTime? MinStartDate { get; set; } // начало времени запуска
        public DateTime? MaxStartDate { get; set; } // начало времени запуска
        public double? MinAverageValue { get; set; } // минимальное среднее значение
        public double? MaxAverageValue { get; set; } // максимальное среднее значение
        public double? MinAverageTime { get; set; } // минимальное среднее время выполнения
        public double? MaxAverageTime { get; set; } // максимальное среднее время выполнения
    }
}
