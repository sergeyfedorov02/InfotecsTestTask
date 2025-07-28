using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Models.Entities;
using InfotecsTestTask.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;



namespace InfotecsTestTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ITimeService _recordService;  // доступ к базе данных
        private readonly IValidator<CsvRecordDto> _validator; // валидатор - для проверки данных
        private ILogger<FilesController> Logger { get; }

        public FilesController(
            ITimeService recordService,  // для связи БД и объектной модели C# (в папке Entities)
            IValidator<CsvRecordDto> validator,
            ILogger<FilesController> logger)
        {
            _recordService = recordService;
            _validator = validator;
            Logger = logger;
        }

        [HttpGet("filterResults")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ResultDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ResultDto>>> GetResultsFilter([FromQuery] ResultFilterDto filters)
        {
            var result = await _recordService.GetResultsFilterAsync(filters);

            if (!result.Success)
            {
                Logger.LogError(result.Exception, "Ошибка при фильтрации результатов");
                return StatusCode(500, "Ошибка при получении отфильтрованных данных");
                
            }
            return Ok(result.Data);
        }

        [HttpGet("lastValues")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CsvRecordDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CsvRecordDto>>> GetLastValues(string fileName)
        {
            var result = await _recordService.GetLastRecordsAsync(fileName);

            if (!result.Success)
            {
                if (result.Exception is FileNotFoundException)
                {
                    Logger.LogError(result.Exception, $"Файл с именем '{fileName}' не найден");
                    return NotFound("Файл не найден");
                }

                Logger.LogError(result.Exception, $"Ошибка при получении значений для файла {fileName}");
                return StatusCode(500, $"Ошибка при получении значений для файла {fileName}");
            }

            return Ok(result.Data);
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadCsv(IFormFile uploadedFile)
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
                Logger.LogError(result.Exception, "Ошибка при загрузке файла csv");
                return StatusCode(500, "Ошибка при обработке файла");
            }

            return Ok(new { result.Data.FileId, result.Data.RecordCount });
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
