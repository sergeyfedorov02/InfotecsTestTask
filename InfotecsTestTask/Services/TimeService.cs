using InfotecsTestTask.Data;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Models.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using InfotecsTestTask.Extensions;

namespace InfotecsTestTask.Services
{
    public class TimeService : ITimeService
    {
        private readonly InfotecsDBContext _context;
        private readonly ILogger<TimeService> _logger;

        public TimeService(InfotecsDBContext context, ILogger<TimeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FileProcessingResult> ProcessFileAsync(
            IFormFile uploadedFile,
            IReadOnlyList<CsvRecordDto> records)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingFile = await _context.Files                   
                    .FirstOrDefaultAsync(f => f.FileName == uploadedFile.FileName);

                // обновление или создание файла
                FileCSV fileEntity;

                if (existingFile != null)
                {
                    // удаляем старые данные
                    await _context.DataCSV
                        .Where(d => d.FileId == existingFile.Id)
                        .ExecuteDeleteAsync();

                    await _context.Results
                        .Where(r => r.FileId == existingFile.Id)
                        .ExecuteDeleteAsync();

                    fileEntity = existingFile;
                    fileEntity.UploadTime = DateTime.UtcNow;
                }
                else
                {
                    fileEntity = new FileCSV
                    {
                        FileName = uploadedFile.FileName,
                        UploadTime = DateTime.UtcNow
                    };
                    await _context.Files.AddAsync(fileEntity);
                }

                // Добавляем записи
                var dataRecords = records.Select(r => new DataCSV
                {
                    FileCSV = fileEntity,
                    Date = r.Date.ToUniversalTime(),  // преобразование в UTC (postgreSql ожидает UTC)
                    ExecutionTime = r.ExecutionTime,
                    Value = r.Value
                }).ToList();

                await _context.DataCSV.AddRangeAsync(dataRecords);

                // Вычисляем агрегаты
                var result = CalculateAggregates(fileEntity, dataRecords);
                await _context.Results.AddAsync(result);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new FileProcessingResult
                {
                    Success = true,
                    FileId = fileEntity.Id,
                    RecordCount = dataRecords.Count
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при сохранении файла");
                return new FileProcessingResult
                {
                    Success = false,
                    ErrorMessage = "Ошибка при обработке файла"
                };
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
