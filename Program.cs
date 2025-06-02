using System.Text;
using System.Text.Json.Serialization;
using backtimetracker.Data;
using backtimetracker.Models;
using backtimetracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

/*────────────────── 1) Database ──────────────────*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

/*────────────────── 2) Identity ───────────────────*/
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();   // ← شامل RoleManager هم می‌شود

/*────────────────── 3) JWT Auth ───────────────────*/
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    var key = builder.Configuration["Jwt:Key"];
    var issuer = builder.Configuration["Jwt:Issuer"];
    var audience = builder.Configuration["Jwt:Audience"];

    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

/*────────────────── 4) Authorization ─────────────*/
builder.Services.AddAuthorization(opts =>
{
    // سیاست اختیاری؛ Attribute هم کفایت می‌کند
    opts.AddPolicy("Admin", p => p.RequireRole("Admin"));
});

/*────────────────── 5) Controllers / JSON ─────────*/
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

/*────────────────── 6) Swagger ───────────────────*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>
{
    cfg.SwaggerDoc("v1", new OpenApiInfo { Title = "BackTimeTracker API", Version = "v1" });

    cfg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "برای استفاده از توکن: 'Bearer {token}' را وارد کنید."
    });

    cfg.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

/*────────────────── 7) CORS برای React ────────────*/
const string FrontPolicy = "Front";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontPolicy, p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

//const string FrontPolicy = "Front";
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(FrontPolicy, p =>
//        p.WithOrigins("http://localhost:5173")
//         .AllowAnyHeader()
//         .AllowAnyMethod());
//});

/*────────────────── 8) Services & DI ─────────────*/
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

/*───── 9) ایجاد نقش‌ها / ادمین اولیه (یک‌بار در استارت) ─────*/
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleInitializer.InitializeAsync(services);
}

/*────────────────── 10) Middleware pipeline ──────*/
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GTC API v1");
//});

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

//app.UseHttpsRedirection();

app.UseCors(FrontPolicy);
app.UseAuthentication();   // ← قبل از UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
