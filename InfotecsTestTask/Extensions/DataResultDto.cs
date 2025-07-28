namespace InfotecsTestTask.Extensions
{
    public sealed class DataResultDto<T> where T : class
    {
        private DataResultDto()
        {

        }

        public static DataResultDto<T> CreateFromException(Exception ex)
        {
            return new DataResultDto<T>
            {
                Success = false,
                Exception = ex
            };
        }

        public static DataResultDto<T> CreateFromData(T data)
        {
            return new DataResultDto<T>
            {
                Success = true,
                Data = data
            };
        }

        public bool Success { get; init; }
        public Exception Exception { get; init; }

        public T Data { get; init; }
    }
}
