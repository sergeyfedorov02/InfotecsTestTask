using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using InfotecsTestTask.Data;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Models.Entities;
using Microsoft.EntityFrameworkCore;
using InfotecsTestTask.Services;



namespace InfotecsTestTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ITimeService _recordService;  // доступ к базе данных
        private readonly IValidator<CsvRecordDto> _validator; // валидатор - для проверки данных
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            ITimeService recordService,  // для связи БД и объектной модели C# (в папке Entities)
            IValidator<CsvRecordDto> validator,
            ILogger<FilesController> logger)
        {
            _recordService = recordService;
            _validator = validator;
            _logger = logger;
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadCsv(IFormFile uploadedFile)
        {
            try
            {
                // Проверка, что файл был загружен и не превышает допустимый размер
                if (uploadedFile == null || uploadedFile.Length == 0)
                    return BadRequest("Файл не был загружен");

                if (uploadedFile.Length > 10 * 1024 * 1024) // 10MB
                    return BadRequest("Файл слишком большой");

                // парсинг и валидация
                var parseResult = await ParseAndValidateCsv(uploadedFile);
                if (!parseResult.IsValid)
                    return BadRequest(parseResult.Errors);

                // проверка количества записей
                if (parseResult.Records.Count < 1 || parseResult.Records.Count > 10000)
                    return BadRequest($"Количество строк должно быть от 1 до 10000. Получено: {parseResult.Records.Count}");

                // обработка в транзакции
                var result = await _recordService.ProcessFileAsync(uploadedFile, parseResult.Records);

                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(new { result.FileId, result.RecordCount });


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        private async Task<(bool IsValid, List<CsvRecordDto> Records, List<string> Errors)> ParseAndValidateCsv(IFormFile file)
        {
            var records = new List<CsvRecordDto>();
            var errors = new List<string>();

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    MissingFieldFound = null,
                    BadDataFound = context => errors.Add($"Неверные данные в строке: {context.RawRecord}")
                });

                csv.Context.RegisterClassMap<CsvRecordMap>();

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var record = csv.GetRecord<CsvRecordDto>();
                        var validationResult = await _validator.ValidateAsync(record);

                        if (!validationResult.IsValid)
                        {
                            errors.AddRange(validationResult.Errors.Select(e => $"Строка {csv.Parser.Row}: {e.ErrorMessage}"));
                        }
                        else
                        {
                            records.Add(record);
                        }
                    }
                    catch (CsvHelperException ex)
                    {
                        errors.Add($"Ошибка парсинга строки {csv.Parser.Row}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Ошибка чтения файла: {ex.Message}");
            }

            return (!errors.Any(), records, errors);
        }
    }
}
