﻿using System.Text;
using System.Text.Json.Serialization;
using backtimetracker.Data;
using backtimetracker.Hubs;
using backtimetracker.Models.User;
using backtimetracker.Services;
using backtimetracker.Swagger; // اضافه‌کردن namespaceِ DocumentFilter
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

/*────────────────── 1) Database ──────────────────*/
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

/*────────────────── 2) Identity ───────────────────*/
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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

    // پیکربندی برای SignalR: دریافت توکن از access_token کوئری‌استرینگ
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/task"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

/*────────────────── 4) Authorization ─────────────*/
builder.Services.AddAuthorization(opts =>
{
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

    // ثبت DocumentFilter برای نمایش هاب SignalR در Swagger
    cfg.DocumentFilter<SignalRSwaggerDocumentFilter>();
});

/*────────────────── 7) CORS برای React ────────────*/
//const string FrontPolicy = "Front";
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(FrontPolicy, p =>
//        p.WithOrigins("*")
//         .AllowAnyHeader()
//         .AllowAnyMethod()
//         .AllowCredentials() );
//});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

const string FrontPolicy = "Front";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontPolicy, p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
    );
});

/*────────────────── 7) CORS برای React ────────────*/
//const string FrontPolicy = "Front";
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(FrontPolicy, p =>
//        p.AllowAnyOrigin()  // آدرس دقیق فرانت
//         .AllowAnyHeader()
//         .AllowAnyMethod()
//    );
//});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(FrontPolicy, p =>
//        p.WithOrigins("http://localhost:7070")  // آدرس دقیق فرانت
//         .AllowAnyHeader()
//         .AllowAnyMethod()
//         .AllowCredentials()                     // لازم برای ارسال توکن/کوکی
//    );
//});


/*────────────────── 8) SignalR ───────────────────*/
builder.Services.AddSignalR();

/*────────────────── 9) Services & DI ─────────────*/
builder.Services.AddScoped<JwtService>();

builder.Services.Configure<FormOptions>(opts =>
{
    opts.MultipartBodyLengthLimit = 10 * 1024 * 1024;   // ۱۰ مگابایت
});

var app = builder.Build();

/*───── 10) ایجاد نقش‌ها / ادمین اولیه (یک‌بار در استارت) ─────*/
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleInitializer.InitializeAsync(services);
}

/*────────────────── 11) Middleware ───────────────────*/

app.UseSwagger();
app.UseSwaggerUI();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();   // ← این خط حتماً باشد
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
//else
//{
//    app.UseExceptionHandler("/Error");
//}

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


app.UseStaticFiles();

app.UseCors(FrontPolicy);
app.UseAuthentication();
app.UseAuthorization();


app.UseAuthentication();
app.UseAuthorization();

// مسیریابی کنترلرها
app.MapControllers();

// مسیریابی هاب SignalR
app.MapHub<TaskHub>("/hubs/task");

app.Run();
