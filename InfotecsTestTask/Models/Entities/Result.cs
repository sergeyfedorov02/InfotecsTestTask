using System.ComponentModel.DataAnnotations.Schema;

namespace InfotecsTestTask.Models.Entities
{
    public class Result
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long FileId { get; set; }
        [ForeignKey("FileId")]

        public FileCSV FileCSV { get; set; }
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
