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

            if (size % 2 == 0)
            {
                var first = sorted[mid - 1];
                var second = sorted[mid];

                // если одно из значений на порядок больше другого
                if (Math.Abs(first) > Math.Abs(second) * 1E15 || Math.Abs(second) > Math.Abs(first) * 1E15)
                {
                    return Math.Abs(first) > Math.Abs(second) ? first : second;
                }

                // проверка переполнения (MaxValue или MinValue -> при сложении будет бесконечность)
                if (double.IsInfinity(first + second))
                {
                    return Math.Max(first, second);
                }   
                
                return (first + second) / 2;
            }
            
            return sorted[mid];
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
