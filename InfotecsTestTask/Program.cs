
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

            // добавляем сервисы валидации и работы с timescale данными 
            // Transient - экземпляр создается всякий раз при использовании
            // Scoped - создается один раз на Http-запрос
            builder.Services.AddScoped<IValidator<CsvRecordDto>, CsvRecordValidator>();
            builder.Services.AddScoped<ITimeService, TimeService>();

            // настраиваем провайдера для БД (PostgreSQL)
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

            // настройка Swagger, используем для любого окружения
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            // убеждаемся, что база создана (если нет -> создаем)
            var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<InfotecsDBContext>();

            db.Database.SetCommandTimeout(60);
            db.Database.EnsureCreated();
            
            app.Run();
        }
    }
}
