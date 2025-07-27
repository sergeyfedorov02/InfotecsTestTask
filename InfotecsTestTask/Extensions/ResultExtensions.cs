namespace InfotecsTestTask.Extensions
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Метод расширения для вычисления медианы
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double CalculateMedian(this IReadOnlyList<double> values)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentException("Список значений должен быть не пустым");
            }

            var sorted = values.OrderBy(v => v).ToList();
            int size = sorted.Count;
            int mid = size / 2;

            return size % 2 == 0 ?
                (sorted[mid - 1] + sorted[mid]) / 2 :
                sorted[mid];
        }

        public static (DateTime minValue, double timeDelta) CalculateDateValues(this IReadOnlyList<DateTime> values)
        {
            if (values == null ||  !values.Any())
            {
                throw new ArgumentException("Список значений должен быть не пустым");
            }

            var minValue = values.Min();
            return (minValue, timeDelta: (values.Max() - minValue).TotalSeconds);
        }
    }
}
