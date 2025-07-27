using InfotecsTestTask.Models.DataTransferObject;

namespace InfotecsTestTask.Services
{
    public interface ITimeService
    {
        Task<FileProcessingResult> ProcessFileAsync(
            IFormFile uploadedFile,
            IReadOnlyList<CsvRecordDto> records
        );
    }

    public class FileProcessingResult
    {
        public bool Success { get; set; }
        public long FileId { get; set; }
        public int RecordCount { get; set; }
        public string ErrorMessage { get; set; }
    }
}
