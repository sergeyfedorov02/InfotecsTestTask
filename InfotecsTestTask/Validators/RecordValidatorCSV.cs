using FluentValidation;
using InfotecsTestTask.Models.DataTransferObject;

namespace InfotecsTestTask.Validators
{
    public class CsvRecordValidator : AbstractValidator<CsvRecordDto>
    {
        private static readonly DateTime MinDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public CsvRecordValidator()
        {
            // для даты
            RuleFor(x => x.Date) // правило для конкретного поля
                .Must(date => date <= DateTime.UtcNow)
                .WithMessage("Дата не может быть позже текущей")
                .Must(date => date >= MinDate)
                .WithMessage("Дата не может быть раньше 01.01.2000");

            // для времени выполнения
            RuleFor(x => x.ExecutionTime)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время выполнения не может быть меньше 0");

            // для показателя
            RuleFor(x => x.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Значение показателя не может быть меньше 0");
        }
    }
}
