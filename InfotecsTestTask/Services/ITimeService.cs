using InfotecsTestTask.Extensions;
using InfotecsTestTask.Models.DataTransferObject;

namespace InfotecsTestTask.Services
{
    public interface ITimeService
    {
        Task<DataResultDto<FileProcessingResult>> ProcessFileAsync(
            string uploadedFileName,
            IReadOnlyList<CsvRecordDto> records
        );

        Task<DataResultDto<DataGetTopResults>> GetLastRecordsAsync(string fileName, int limit = 10);

        Task<DataResultDto<DataFilterResults>> GetResultsFilterAsync(ResultFilterDto filters);
    }

    public class FileProcessingResult
    {
        public long FileId { get; set; }
        public int RecordCount { get; set; }
    }

    public class DataGetTopResults
    {
        public List<CsvRecordDto> Data { get; set; }
    }

    public class DataFilterResults
    {
        public List<ResultDto> Data { get; set; }
    }
}
