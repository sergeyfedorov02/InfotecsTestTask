using System.ComponentModel.DataAnnotations.Schema;

namespace InfotecsTestTask.Models.Entities
{
    public class DataCSV
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long FileId { get; set; }
        [ForeignKey("FileId")]

        public FileCSV FileCSV { get; set; }
        public DateTime Date { get; set; }
        public double ExecutionTime { get; set; }
        public double Value { get; set; }
    }
}
