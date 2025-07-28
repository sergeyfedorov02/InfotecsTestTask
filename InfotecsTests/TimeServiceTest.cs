using FluentAssertions;
using InfotecsTestTask.Data;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Models.Entities;
using InfotecsTestTask.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace InfotecsTests
{
    public class TimeServiceTest
    {
        public static DbContextOptions<InfotecsDBContext> GetSqliteInMemoryProviderOptions(SqliteConnection connection)
        {
            return new DbContextOptionsBuilder<InfotecsDBContext>().UseSqlite(connection).Options;
        }

        [Fact]
        public async Task GetLastRecordsAsyncTest()
        {
            var logger = new Mock<ILogger<TimeService>>();

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = GetSqliteInMemoryProviderOptions(connection);

            var testDate = new DateTime(2025, 7, 28, 0, 0, 0, DateTimeKind.Utc);

            var testFileName1 = "test.csv";
            var testFileName2 = "test2.csv";

            using (var ctx = new InfotecsDBContext(options))
            {
                await ctx.Database.EnsureCreatedAsync();

                var file1 = new FileCSV
                {
                    FileName = testFileName1,
                    UploadTime = DateTime.UtcNow
                };

                var file2 = new FileCSV
                {
                    FileName = testFileName2,
                    UploadTime = DateTime.UtcNow
                };

                ctx.Files.AddRange(file1, file2);

                ctx.DataCSV.AddRange(new DataCSV
                {
                    ExecutionTime = 10,
                    Value = 100,
                    Date = testDate,
                    FileCSV = file1
                }, new DataCSV
                {
                    ExecutionTime = 80,
                    Value = 50.2,
                    FileCSV = file2,
                    Date = testDate
                });

                ctx.SaveChanges();
            }

            var service = new TimeService(() => new InfotecsDBContext(options), logger.Object);

            var result = await service.GetLastRecordsAsync(testFileName1);

            result.Success.Should().BeTrue();
            result.Data.Data.Should().BeEquivalentTo(new[]
            {
                new
                {
                    ExecutionTime = 10,
                    Value = 100,
                    Date = testDate
                }
            }
            );
        }

        [Fact]
        public async Task GetResultsFilterAsyncTest()
        {
            var logger = new Mock<ILogger<TimeService>>();

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = GetSqliteInMemoryProviderOptions(connection);

            var testFileName1 = "test1.csv";
            var testFileName2 = "test2.csv";

            var testDate = new DateTime(2025, 7, 28, 0, 0, 0, DateTimeKind.Utc);

            var testFilter = new ResultFilterDto { FileName = testFileName1 };

            using (var ctx = new InfotecsDBContext(options))
            {
                await ctx.Database.EnsureCreatedAsync();

                var file1 = new FileCSV
                {
                    FileName = testFileName1,
                    UploadTime = DateTime.UtcNow
                };

                var file2 = new FileCSV
                {
                    FileName = testFileName2,
                    UploadTime = DateTime.UtcNow
                };

                ctx.Files.AddRange(file1, file2);

                ctx.Results.AddRange(new Result
                {
                    FileCSV = file1,
                    TimeDelta = 120000,
                    MinDate = testDate,
                    AverageExecutionTime = 100,
                    AverageValue = 20,
                    MedianValue = 15,
                    MaxValue = 200,
                    MinValue = 100,
                    LastUpdated = testDate

                }, new Result
                {
                    FileCSV = file2,
                    TimeDelta = 120000,
                    MinDate = testDate,
                    AverageExecutionTime = 200,
                    AverageValue = 15,
                    MedianValue = 10,
                    MaxValue = 100,
                    MinValue = 50,
                    LastUpdated = testDate
                });

                ctx.SaveChanges();
            }

            var service = new TimeService(() => new InfotecsDBContext(options), logger.Object);

            var result = await service.GetResultsFilterAsync(testFilter);

            result.Success.Should().BeTrue();
            var x = result.Data.Data;
            result.Data.Data.Should().BeEquivalentTo(new[]
            {
                new
                {
                    FileName = testFileName1
                }
            }
            );
        }

        [Fact]
        public async Task ProcessFileAsyncTest()
        {
            var logger = new Mock<ILogger<TimeService>>();

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = GetSqliteInMemoryProviderOptions(connection);

            var testFileName1 = "test1.csv";

            var testDate = new DateTime(2025, 7, 28, 0, 0, 0, DateTimeKind.Utc);

            var testFilter = new ResultFilterDto { FileName = testFileName1 };

            using (var ctx = new InfotecsDBContext(options))
            {
                await ctx.Database.EnsureCreatedAsync();
            }

            var service = new TimeService(() => new InfotecsDBContext(options), logger.Object);

            var result = await service.ProcessFileAsync(testFileName1, [ 
                new CsvRecordDto 
                {
                    Date = testDate,
                    ExecutionTime = 100,
                    Value = 15.3
                },
                new CsvRecordDto
                {
                    Date = testDate,
                    ExecutionTime = 200,
                    Value = 37.8
                }
                ]);

            result.Success.Should().BeTrue();

            using (var ctx = new InfotecsDBContext(options))
            {
                ctx.Files.ToList().Should().BeEquivalentTo(
                [
                    new
                    {
                        FileName = testFileName1
                    }
                ]);

                ctx.DataCSV.ToList().Should().BeEquivalentTo(
                    [
                    new 
                    {
                        Date = testDate,
                        ExecutionTime = 100,
                        Value = 15.3
                    },
                    new 
                    {
                        Date = testDate,
                        ExecutionTime = 200,
                        Value = 37.8
                    }
                    ]
                );

                ctx.Results.ToList().Should().BeEquivalentTo(
                [
                    new
                    {
                        MinValue = 15.3,
                        MaxValue = 37.8,
                        MinDate = testDate,
                        AverageExecutionTime = 150
                    }
                ]);
            }
        }
    }
}
