using Application.Interfaces;
using Application.Mappers;
using Application.Models;
using Application.Services;
using Domain.Constants;
using Domain.Entities.Identity;
using Infrastructure;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddSingleton<CategoryMapper>();
builder.Services.AddSingleton<UserMapper>();

builder.Services.AddIdentity<UserEntity, RoleEntity>(options =>
{
    options.Password.RequireDigit = false; //Цифри
    options.Password.RequireNonAlphanumeric = false; //Спеціальні символи
    options.Password.RequireLowercase = false;  //Маленькі літери
    options.Password.RequireUppercase = false; //Великі літери
    options.Password.RequiredLength = 6; //Кількість символів
    options.Password.RequiredUniqueChars = 1;  //Кількість унікальних символів
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    Type[] markerTypes = new[]
    {
        // сюди додавати об'єкти яким потрібна xml документація на swagger
        typeof(RegisterModel),
    };

    foreach (var type in markerTypes)
    {
        var assemblyName = type.Assembly.GetName().Name;
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");

        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
});

string webUrl = builder.Configuration.GetValue<string>("WebUrl") ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(webUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies[CookieConstants.AccessTokenCookieName];
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();
 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var dirImageName = builder.Configuration.GetValue<string>("DirImageName") ?? "images";
var path = Path.Combine(Directory.GetCurrentDirectory(), dirImageName);
Directory.CreateDirectory(path);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(path),
    RequestPath = $"/{dirImageName}"
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<RoleEntity>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        await context.Database.MigrateAsync();
        logger.LogInformation("Міграції успішно застосовані.");

        await DbSeeder.SeedAllAsync(services, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Помилка під час ініціалізації бази даних.");
    }
}

app.Run();
