using InfotecsTestTask.Data;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Models.Entities;
using Microsoft.EntityFrameworkCore;
using InfotecsTestTask.Extensions;

namespace InfotecsTestTask.Services
{
    public class TimeService : ITimeService
    {
        private Func<InfotecsDBContext> ContextProvider { get; }
        private readonly ILogger<TimeService> _logger;

        public TimeService(Func<InfotecsDBContext> contextProvider, ILogger<TimeService> logger)
        {
            ContextProvider = contextProvider;
            _logger = logger;
        }

        public async Task<DataResultDto<FileProcessingResult>> ProcessFileAsync(
            string uploadedFileName,
            IReadOnlyList<CsvRecordDto> records)
        {
            await using var context = ContextProvider();

            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var existingFile = await context.Files                   
                    .FirstOrDefaultAsync(f => f.FileName == uploadedFileName);

                // обновление или создание файла
                FileCSV fileEntity;

                if (existingFile != null)
                {
                    // удаляем старые данные
                    await context.DataCSV
                        .Where(d => d.FileId == existingFile.Id)
                        .ExecuteDeleteAsync();

                    await context.Results
                        .Where(r => r.FileId == existingFile.Id)
                        .ExecuteDeleteAsync();

                    fileEntity = existingFile;
                    fileEntity.UploadTime = DateTime.UtcNow;
                }
                else
                {
                    fileEntity = new FileCSV
                    {
                        FileName = uploadedFileName,
                        UploadTime = DateTime.UtcNow
                    };
                    await context.Files.AddAsync(fileEntity);
                }

                // Добавляем записи
                var dataRecords = records.Select(r => new DataCSV
                {
                    FileCSV = fileEntity,
                    Date = r.Date.ToUniversalTime(),  // преобразование в UTC (postgreSql ожидает UTC)
                    ExecutionTime = r.ExecutionTime,
                    Value = r.Value
                }).ToList();

                await context.DataCSV.AddRangeAsync(dataRecords);

                // Вычисляем агрегаты
                var result = CalculateAggregates(fileEntity, dataRecords);
                await context.Results.AddAsync(result);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return DataResultDto<FileProcessingResult>.CreateFromData(new FileProcessingResult
                {
                    FileId = fileEntity.Id,
                    RecordCount = dataRecords.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return DataResultDto<FileProcessingResult>.CreateFromException(ex);
            }
        }

        public async Task<DataResultDto<DataGetTopResults>> GetLastRecordsAsync(string fileName, int limit = 10)
        {
            await using var context = ContextProvider();

            try
            {
                // Находим файл по имени с включением связанных данных
                var file = await context.Files
                    .Where(f => f.FileName == fileName)  // фильтрация по имени
                    .FirstOrDefaultAsync();

                if (file == null)
                {
                    return DataResultDto<DataGetTopResults>.CreateFromException(
                        new FileNotFoundException()
                    ); 
                }

                // Получаем последние 10 записей, отсортированных по дате
                var lastRecords = await context.DataCSV.Where(d => d.FileId == file.Id)
                    .OrderByDescending(r => r.Date)
                    .Take(limit)
                    .Select(r => new CsvRecordDto
                    {
                        Date = r.Date,
                        ExecutionTime = r.ExecutionTime,
                        Value = r.Value
                    })
                    .ToListAsync();

                return DataResultDto<DataGetTopResults>.CreateFromData(new DataGetTopResults
                {
                    Data = lastRecords
                });
            }
            catch (Exception ex)
            {
                return DataResultDto<DataGetTopResults>.CreateFromException(ex);
            }
        }

        public async Task<DataResultDto<DataFilterResults>> GetResultsFilterAsync(ResultFilterDto filters)
        {
            await using var context = ContextProvider();

            try
            {
                var query = context.Results
                    .Include(r => r.FileCSV)
                    .ApplyFilters(filters)  // применение фильтров
                    .AsQueryable();
 
                // формирование результата
                var results = await query.Select(r => new ResultDto
                {
                    Id = r.Id,
                    FileId = r.FileId,
                    FileName = r.FileCSV.FileName,
                    MinDate = r.MinDate,
                    AverageExecutionTime = r.AverageExecutionTime,
                    AverageValue = r.AverageValue,
                    MedianValue = r.MedianValue,
                    MaxValue = r.MaxValue,
                    MinValue = r.MinValue,
                    LastUpdated = r.LastUpdated
                }).ToListAsync();


                return DataResultDto<DataFilterResults>.CreateFromData( new DataFilterResults
                {
                    Data = results
                });
            }
            catch (Exception ex)
            {
                return DataResultDto<DataFilterResults>.CreateFromException(ex);
            }
        }

        private static Result CalculateAggregates(FileCSV fileEntity, List<DataCSV> dataRecords)
        {
            var values = dataRecords.Select(r => r.Value).ToList();
            var executionTimes = dataRecords.Select(r => r.ExecutionTime).ToList();
            var dates = dataRecords.Select(r => r.Date).ToList();

            var dateValues = dates.CalculateDateValues();

            return new Result
            {
                FileCSV = fileEntity,
                TimeDelta = dateValues.timeDelta,
                MinDate = dateValues.minValue,
                AverageExecutionTime = executionTimes.Average(),
                AverageValue = values.Average(),
                MedianValue = values.CalculateMedian(),
                MaxValue = values.Max(),
                MinValue = values.Min(),
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}
