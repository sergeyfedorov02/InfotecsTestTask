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
            {
                [1,2,3,4], 2.5
            }
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
            {
                [ new DateTime(2025,7,27)], 0, new DateTime(2025,7,27)
            }
        };
    }
}
