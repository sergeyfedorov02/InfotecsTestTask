using FluentAssertions;
using InfotecsTestTask.Extensions;

namespace InfotecsTests
{
    public class ResultTest
    {
        [Theory]
        [MemberData(nameof(MedianData))]
        public void CalculateMedianTest(List<double> values, double expectedValue)
        {
            var t1 = values.CalculateMedian();
            Assert.Equal(expectedValue, t1, 1e-6);
        }

        public static TheoryData<List<double>, double> MedianData =>
        new()
        {
            { [1,2,3,4], 2.5 },         // четное
            { [1,2,3], 2 },             // нечетное
            { [10,10,20,20], 15 },      // дубликаты четное
            { [10,20,10], 10 },         // дубликаты нечетное
            { [double.MaxValue, double.MaxValue], double.MaxValue },        // MaxValue
            { [double.MinValue, double.MinValue], double.MinValue },        // MinValue
            { [double.MaxValue, 1], double.MaxValue },                      // MaxValue и 1
            { [double.MinValue, -1], double.MinValue },                     // MinValue и -1
            { [10,10,10], 10 },             // одинаковые
            { [2.2,3.3,4.4,5.5], 3.85 }     // дробные
        };

        [Fact]
        public void CalculateMedianEmptyListTest()
        {
            var values = new List<double>();

            Action act = () => values.CalculateMedian();

            act.Should().Throw<ArgumentException>().WithMessage("Список значений должен быть не пустым");
        }
        
        [Theory]
        [MemberData(nameof(TimeData))]
        public void CalculateDateValuesTest(List<DateTime> values, double expectedDelta, DateTime expectedMinDate)
        {
            var calculatedValue = values.CalculateDateValues();
            Assert.Equal(calculatedValue.timeDelta, expectedDelta, 1e-6);
            Assert.Equal(calculatedValue.minValue, expectedMinDate);
        }

        public static TheoryData<List<DateTime>, double, DateTime> TimeData =>
        new()
        {
            // одно значение
            {
                [ new DateTime(2025,7,27, 0, 0, 0, DateTimeKind.Utc)], 
                0, 
                new DateTime(2025,7,27, 0, 0, 0, DateTimeKind.Utc)
            },

            // одинаковые даты
            {
                new List<DateTime> { 
                    new DateTime(2025, 7, 27, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2025, 7, 27, 0, 0, 0, DateTimeKind.Utc) },
                0, 
                new DateTime(2025,7,27, 0, 0, 0, DateTimeKind.Utc)
            },

            // разница в 1 день
            {
                new List<DateTime> { 
                    new DateTime(2025, 7, 27, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2025, 7, 28, 0, 0, 0, DateTimeKind.Utc) },
                86400,
                new DateTime(2025,7,27, 0, 0, 0, DateTimeKind.Utc)
            },

            // несколько дат в прямом порядке
            {
                new List<DateTime> { 
                    new DateTime(2025, 7, 24, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2025, 7, 26, 0, 0, 0, DateTimeKind.Utc) , 
                    new DateTime(2025, 7, 28, 0, 0, 0, DateTimeKind.Utc) },
                345600,
                new DateTime(2025,7,24, 0, 0, 0, DateTimeKind.Utc)
            },

            // несколько дат в смешанном порядке
            {
                new List<DateTime> { 
                    new DateTime(2025, 7, 28, 0, 0, 0, DateTimeKind.Utc), 
                    new DateTime(2025, 7, 24, 0, 0, 0, DateTimeKind.Utc) , 
                    new DateTime(2025, 7, 26, 0, 0, 0, DateTimeKind.Utc) },
                345600,
                new DateTime(2025,7,24, 0, 0, 0, DateTimeKind.Utc)
            },

        };
    }
}
