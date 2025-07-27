
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
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddTransient<IValidator<CsvRecordDto>, CsvRecordValidator>();
            builder.Services.AddTransient<ITimeService, TimeService>();

            builder.Services.AddDbContext<InfotecsDBContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("InfotecsConnection"));
            });

            builder.Services.AddTransient<Func<InfotecsDBContext>>(provider =>
               () => provider.CreateScope().ServiceProvider.GetRequiredService<InfotecsDBContext>());

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<InfotecsDBContext>();

            db.Database.SetCommandTimeout(60);
            db.Database.EnsureCreated();
            //db.Database.Migrate();

            app.Run();
        }
    }
}
