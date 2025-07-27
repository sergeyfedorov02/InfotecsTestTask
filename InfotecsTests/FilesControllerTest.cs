using FluentAssertions;
using FluentValidation;
using InfotecsTestTask.Controllers;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Services;
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
            timeService.Setup(t => t.GetLastRecordsAsync(fileName, 10)).ReturnsAsync(new DataGetTopResults<List<CsvRecordDto>>
            {
                Success = false
            });

            var validator = new Mock<IValidator<CsvRecordDto>>(MockBehavior.Strict);
            var logger = new Mock<ILogger<FilesController>>();

            var controller = new FilesController(timeService.Object, validator.Object, logger.Object);
            var result = await controller.GetLastValues(fileName);

            result.Result.Should().BeEquivalentTo(new
            {
                StatusCode = 500
            });

        }
    }
}
