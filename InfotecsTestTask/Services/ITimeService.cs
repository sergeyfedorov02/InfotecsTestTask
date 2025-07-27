using InfotecsTestTask.Models.DataTransferObject;

namespace InfotecsTestTask.Services
{
    public interface ITimeService
    {
        Task<FileProcessingResult> ProcessFileAsync(
            IFormFile uploadedFile,
            IReadOnlyList<CsvRecordDto> records
        );

        Task<DataGetTopResults<List<CsvRecordDto>>> GetLastRecordsAsync(string fileName, int limit = 10);
    }

    public class FileProcessingResult
    {
        public bool Success { get; set; }
        public long FileId { get; set; }
        public int RecordCount { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DataGetTopResults<T>
    {
        public bool Success { get; set; }
        public List<CsvRecordDto> Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}
