namespace InfotecsTestTask.Models.DataTransferObject
{
    public class CsvRecordDto
    {
        /// <summary>
        /// Время начала ГГГГ-ММ-ДДTчч-мм-сс.ммммZ
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///  время выполнения в секундах
        /// </summary>
        public double ExecutionTime { get; set; }

        /// <summary>
        /// показатель в виде числа с плавающей запятой
        /// </summary>
        public double Value { get; set; }   
    }
}
