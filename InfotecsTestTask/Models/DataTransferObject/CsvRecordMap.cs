using CsvHelper.Configuration;

namespace InfotecsTestTask.Models.DataTransferObject
{
    /// <summary>
    /// Сопоставление столбцов со свойствами
    /// </summary>
    public sealed class CsvRecordMap : ClassMap<CsvRecordDto>
    {
        public CsvRecordMap()
        {
            Map(m => m.Date)
                .Name("Date")
                .TypeConverterOption.Format("yyyy-MM-ddTHH-mm-ss.ffffZ");

            Map(m => m.ExecutionTime)
                .Name("Execution Time")
                .Default(0);

            Map(m => m.Value)
                .Name("Value")
                .Default(0);
        }
    }
}
