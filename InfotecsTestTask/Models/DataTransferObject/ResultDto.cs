namespace InfotecsTestTask.Models.DataTransferObject
{
    public class ResultDto
    {
        public long Id { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; }
        public double TimeDelta { get; set; }
        public DateTime MinDate { get; set; }
        public double AverageExecutionTime { get; set; }
        public double AverageValue { get; set; }
        public double MedianValue { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public DateTime LastUpdated { get; set; }
    }    
}
