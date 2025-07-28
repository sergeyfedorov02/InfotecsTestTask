namespace InfotecsTestTask.Models.DataTransferObject
{
    /// <summary>
    /// Класс для обработки фильтрации
    /// </summary>
    public class ResultFilterDto
    {
        /// <summary>
        /// по имени файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// начало времени запуска
        /// </summary>
        public DateTime? MinStartDate { get; set; }

        /// <summary>
        /// начало времени запуска
        /// </summary>
        public DateTime? MaxStartDate { get; set; }

        /// <summary>
        /// минимальное среднее значение
        /// </summary>
        public double? MinAverageValue { get; set; }

        /// <summary>
        /// максимальное среднее значение
        /// </summary>
        public double? MaxAverageValue { get; set; }

        /// <summary>
        /// минимальное среднее время выполнения
        /// </summary>
        public double? MinAverageTime { get; set; }

        /// <summary>
        /// максимальное среднее время выполнения
        /// </summary>
        public double? MaxAverageTime { get; set; } 
    }
}
