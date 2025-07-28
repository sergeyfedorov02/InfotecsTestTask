
using FluentValidation;
using InfotecsTestTask.Data;
using InfotecsTestTask.Models.DataTransferObject;
using InfotecsTestTask.Services;
using InfotecsTestTask.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace InfotecsTestTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            
            // добавл€ем сервисы валидации и работы с timescale данными 
            builder.Services.AddTransient<IValidator<CsvRecordDto>, CsvRecordValidator>();
            builder.Services.AddTransient<ITimeService, TimeService>();

            // настраиваем провайдера дл€ Ѕƒ (PostgreSQL)
            builder.Services.AddDbContext<InfotecsDBContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("InfotecsConnection"));
            });

            builder.Services.AddTransient<Func<InfotecsDBContext>>(provider =>
               () => provider.CreateScope().ServiceProvider.GetRequiredService<InfotecsDBContext>());

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // настройка Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            // убеждаемс€, что база создана (если нет -> создаем)
            var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<InfotecsDBContext>();

            db.Database.SetCommandTimeout(60);
            db.Database.EnsureCreated();
            
            app.Run();
        }
    }
}
