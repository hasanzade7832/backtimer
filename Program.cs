using System.Text.Json.Serialization;
using backtimetracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

/*────────────────────  Services  ────────────────────*/

// کنترلرها
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>
{
    cfg.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BackTimeTracker API",
        Version = "v1"
    });
});

// اتصال به SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


// CORS برای کلاینت Vite
const string FrontPolicy = "Front";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(FrontPolicy, p =>
        p.WithOrigins("http://localhost:5173")   // آدرس فرانت‌اِند
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

/*───────────────────  Middleware  ───────────────────*/

// Swagger UI فقط در حالت Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(FrontPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
