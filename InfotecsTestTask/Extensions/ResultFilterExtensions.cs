using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Models.Entities;

namespace InfotecsTestTask.Extensions
{
    public static class ResultFilterExtensions
    {
        /// <summary>
        /// Применение фильтров
        /// </summary>
        /// <param name="query"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static IQueryable<Result> ApplyFilters(this IQueryable<Result> query, ResultFilterDto filters)
        {
            if (filters == null)
                return query;

            if (!string.IsNullOrEmpty(filters.FileName))
            {
                query = query.Where(r => r.FileCSV.FileName.Contains(filters.FileName));
            }

            if (filters.MinStartDate.HasValue)
            {
                query = query.Where(r => r.FileCSV.UploadTime >= filters.MinStartDate.Value);
            }

            if (filters.MaxStartDate.HasValue)
            {
                query = query.Where(r => r.FileCSV.UploadTime <= filters.MaxStartDate.Value);
            }

            if (filters.MinAverageValue.HasValue)
            {
                query = query.Where(r => r.AverageValue >= filters.MinAverageValue.Value);
            }

            if (filters.MaxAverageValue.HasValue)
            {
                query = query.Where(r => r.AverageValue <= filters.MaxAverageValue.Value);
            }

            if (filters.MinAverageTime.HasValue)
            {
                query = query.Where(r => r.AverageExecutionTime >= filters.MinAverageTime.Value);
            }

            if (filters.MaxAverageTime.HasValue)
            {
                query = query.Where(r => r.AverageExecutionTime <= filters.MaxAverageTime.Value);
            }

            return query;
        }
    }
}
