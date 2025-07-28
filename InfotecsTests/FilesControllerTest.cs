using FluentAssertions;
using FluentValidation;
using InfotecsTestTask.Controllers;
using InfotecsTestTask.Extensions;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfotecsTests
{
    public class FilesControllerTest
    {
        [Fact]
        public async Task GetLastValuesNoDataTest()
        {
            const string fileName = "test1.csv";

            var timeService = new Mock<ITimeService>(MockBehavior.Strict);
            timeService.Setup(t => t.GetLastRecordsAsync(fileName, 10)).ReturnsAsync(
                DataResultDto<DataGetTopResults>.CreateFromException(new FileNotFoundException())
            );
            

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(timeService.Object, validator.Object, logger.Object);
            var result = await controller.GetLastValues(fileName);

            result.Result.Should().BeEquivalentTo(new
            {
                StatusCode = 404
            });

        }

        [Fact]
        public async Task GetLastValuesDataBaseErrorTest()
        {
            const string fileName = "test1.csv";

            var timeService = new Mock<ITimeService>(MockBehavior.Strict);
            timeService.Setup(t => t.GetLastRecordsAsync(fileName, 10)).ReturnsAsync(
                DataResultDto<DataGetTopResults>.CreateFromException(new Exception("DB error"))
            );

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(timeService.Object, validator.Object, logger.Object);
            var result = await controller.GetLastValues(fileName);

            result.Result.Should().BeEquivalentTo(new
            {
                StatusCode = 500
            });
        }

        /// <summary>
        /// Тестируем корретные данные с получением результата для GetLastValues
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="expectedRecords"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(LastValuesTestData))]
        public async Task GetLastValuesCorrectResultsTest(
            string fileName,List<CsvRecordDto> expectedRecords, int expectedStatusCode)
        {
            var timeService = new Mock<ITimeService>(MockBehavior.Strict);
            timeService.Setup(t => t.GetLastRecordsAsync(fileName, 10)).ReturnsAsync(
                DataResultDto<DataGetTopResults>.CreateFromData(
                    new DataGetTopResults { Data = expectedRecords })
            );

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(timeService.Object, validator.Object, logger.Object);
            var result = await controller.GetLastValues(fileName);

            // проверяем, что результат имеет тип OkObjectResult, затем проверяем само значение (должно быть 200)
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.StatusCode.Should().Be(expectedStatusCode);

            // получаем сами значения и проверяем
            var responseData = okResult.Value as DataGetTopResults;
            responseData.Should().NotBeNull();
            responseData!.Data.Should().BeEquivalentTo(
                expectedRecords, 
                options => options.WithStrictOrdering()
            );
        }

        public static TheoryData<string, List<CsvRecordDto>, int> LastValuesTestData =>
        new()
        {
            // пустой файл
            {
                "empty.csv",
                new List<CsvRecordDto>(),
                200
            },

            // записей меньше 10
            {
                "file1.csv",
                new List<CsvRecordDto>
                {
                    new() { Date = DateTime.UtcNow.AddDays(-4), ExecutionTime = 100, Value = 1.1 },
                    new() { Date = DateTime.UtcNow.AddDays(-3), ExecutionTime = 200, Value = 2.2 },
                    new() { Date = DateTime.UtcNow.AddDays(-2), ExecutionTime = 300, Value = 3.3 },
                    new() { Date = DateTime.UtcNow.AddDays(-1), ExecutionTime = 400, Value = 4.4 },
                    new() { Date = DateTime.UtcNow, ExecutionTime = 500, Value = 5.5 }
                },
                200
            },
        
            // записей больше 10
            {
                "file2.csv",
                new List<CsvRecordDto>
                {
                    new() { Date = DateTime.UtcNow.AddDays(-12), ExecutionTime = 100, Value = 1.1 },
                    new() { Date = DateTime.UtcNow.AddDays(-11), ExecutionTime = 200, Value = 2.2 },
                    new() { Date = DateTime.UtcNow.AddDays(-10), ExecutionTime = 300, Value = 3.3 },
                    new() { Date = DateTime.UtcNow.AddDays(-9), ExecutionTime = 400, Value = 4.4 },
                    new() { Date = DateTime.UtcNow.AddDays(-8), ExecutionTime = 500, Value = 5.5 },
                    new() { Date = DateTime.UtcNow.AddDays(-7), ExecutionTime = 600, Value = 6.6 },
                    new() { Date = DateTime.UtcNow.AddDays(-6), ExecutionTime = 700, Value = 7.7 },
                    new() { Date = DateTime.UtcNow.AddDays(-5), ExecutionTime = 800, Value = 8.8 },
                    new() { Date = DateTime.UtcNow.AddDays(-4), ExecutionTime = 900, Value = 9.9 },
                    new() { Date = DateTime.UtcNow.AddDays(-3), ExecutionTime = 100, Value = 1.1 },
                    new() { Date = DateTime.UtcNow.AddDays(-2), ExecutionTime = 200, Value = 2.2 },
                    new() { Date = DateTime.UtcNow.AddDays(-1), ExecutionTime = 300, Value = 3.3 },
                    new() { Date = DateTime.UtcNow, ExecutionTime = 400, Value = 4.5 }
                },
                200
            }
        };

        /// <summary>
        /// Объединение первых двух тестов с использованием Theory для GetLastValues
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="exception"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(LastValuesErrorTestData))]
        public async Task GetLastValuesWithErrorsTest(
            string fileName,Exception exception,int expectedStatusCode)
        {
            var mockService = new Mock<ITimeService>(MockBehavior.Strict);
            mockService.Setup(t => t.GetLastRecordsAsync(fileName, It.IsAny<int>()))
                .ReturnsAsync(DataResultDto<DataGetTopResults>.CreateFromException(exception));

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(mockService.Object, validator.Object, logger.Object);

            var result = await controller.GetLastValues(fileName);

            result.Result.Should().BeEquivalentTo(new
            {
                StatusCode = expectedStatusCode
            });
        }

        public static TheoryData<string, Exception, int> LastValuesErrorTestData =>
        new()
        {
            { "not_found.csv", new FileNotFoundException(), 404 },
            { "db_error.csv", new Exception("Database error"), 500 },
        };

        /// <summary>
        /// Получение ошибки для GetResultsFilter
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetResultsFilterErrorTest()
        {
            var filters = new ResultFilterDto();

            var timeService = new Mock<ITimeService>(MockBehavior.Strict);
            timeService.Setup(t => t.GetResultsFilterAsync(filters))
                .ReturnsAsync(DataResultDto<DataFilterResults>.CreateFromException(new Exception("DB error"))
            );

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(timeService.Object, validator.Object, logger.Object);
            var result = await controller.GetResultsFilter(filters);

            result.Result.Should().BeEquivalentTo(new
            {
                StatusCode = 500
            });
        }

        /// <summary>
        /// Тестируем корретные данные с получением результата для GetResultsFilter
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="expectedResults"></param>
        /// <param name="expectedStatusCode"></param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(ResultsFilterTestData))]
        public async Task GetResultsFilterCorrectResultsTest(
            ResultFilterDto filters, List<ResultDto> expectedResults, int expectedStatusCode)
        {
            var timeService = new Mock<ITimeService>(MockBehavior.Strict);
            timeService.Setup(t => t.GetResultsFilterAsync(filters)).ReturnsAsync(
                DataResultDto<DataFilterResults>.CreateFromData(
                    new DataFilterResults { Data = expectedResults })
            );

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(timeService.Object, validator.Object, logger.Object);
            var result = await controller.GetResultsFilter(filters);

            // проверяем, что результат имеет тип OkObjectResult, затем проверяем само значение (должно быть 200)
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult.StatusCode.Should().Be(expectedStatusCode);

            // получаем сами значения и проверяем
            var responseData = okResult.Value as DataFilterResults;
            responseData.Should().NotBeNull();
            var responseDataAsResultDto = responseData.Data;
            responseDataAsResultDto.Should().BeEquivalentTo(
                expectedResults,
                options => options.WithStrictOrdering()
            );
        }

        public static TheoryData<ResultFilterDto, List<ResultDto>, int> ResultsFilterTestData =>
        new()
        {
            // фильтры не заполнены
            {
                new ResultFilterDto(),
                new List<ResultDto>(),
                200
            },

            // один результат
            {
                new ResultFilterDto { FileName = "test1.csv" },
                new List<ResultDto>
                {
                    new()
                    {
                        Id = 1,
                        FileId = 1,
                        FileName = "test1.csv",
                        MinDate = DateTime.UtcNow.AddDays(-1),
                        AverageExecutionTime = 100,
                        AverageValue = 1.5,
                        MedianValue = 1.5,
                        MaxValue = 2.0,
                        MinValue = 1.0,
                        LastUpdated = DateTime.UtcNow
                    }
                },
                200
            },
        
            // несколько результатов
            {
                new ResultFilterDto { FileName = "test2.csv" },
                new List<ResultDto>
                {
                    new()
                    {
                        Id = 2,
                        FileId = 2,
                        FileName = "test2.csv",
                        MinDate = DateTime.UtcNow.AddDays(-2),
                        AverageExecutionTime = 200,
                        AverageValue = 2.0,
                        MedianValue = 2.0,
                        MaxValue = 3.0,
                        MinValue = 1.0,
                        LastUpdated = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = 3,
                        FileId = 2,
                        FileName = "test2.csv",
                        MinDate = DateTime.UtcNow.AddDays(-3),
                        AverageExecutionTime = 150,
                        AverageValue = 1.5,
                        MedianValue = 1.5,
                        MaxValue = 2.0,
                        MinValue = 1.0,
                        LastUpdated = DateTime.UtcNow
                    }
                },
                200
            }
        };
    }
}
