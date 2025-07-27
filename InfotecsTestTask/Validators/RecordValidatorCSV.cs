using FluentValidation;
using InfotecsTestTask.Models.DataTransferObject;

namespace InfotecsTestTask.Validators
{
    public class CsvRecordValidator : AbstractValidator<CsvRecordDto>
    {
        public CsvRecordValidator()
        {
            // для даты
            RuleFor(x => x.Date) // правило для конкретного поля
                .NotEmpty()
                .Must(date => date <= DateTime.UtcNow)
                .WithMessage("Дата не может быть позже текущей!")
                .Must(date => date >= new DateTime(2000, 1, 1))
                .WithMessage("Дата не может быть раньше 01.01.2000!");

            // для времени выполнения
            RuleFor(x => x.ExecutionTime)
                .NotEmpty()
                .GreaterThanOrEqualTo(0)
                .WithMessage("Время выполнения не может быть меньше 0!");

            // для показателя
            RuleFor(x => x.Value)
                .NotEmpty()
                .GreaterThanOrEqualTo(0)
                .WithMessage("Значение показателя не может быть меньше 0!");
        }
    }
}
