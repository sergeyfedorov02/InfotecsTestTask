using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfotecsTestTask.Models.Entities
{
    public class FileCSV
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string FileName { get; set; }         
        public DateTime UploadTime { get; set; }      
        
        // свойства для навигации
        public virtual ICollection<DataCSV> DataRecords { get; set; }
        public virtual Result Result { get; set; }
    }
}
