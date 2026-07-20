using AI.TestCaseGenerator.API.Configuration;
using AI.TestCaseGenerator.API.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AI.TestCaseGenerator.API.Interfaces;
using AI.TestCaseGenerator.API.Services;
using AI.TestCaseGenerator.API.DTOs;
using AI.TestCaseGenerator.API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System;



var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey configuration is missing")))
        };

        // Add event logging to capture authentication failures and successful validations
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception != null)
                {
                    Console.WriteLine($"[JWT AUTH ERROR] {context.Exception.GetType().Name}: {context.Exception.Message}");
                    if (context.Exception.InnerException != null)
                    {
                        Console.WriteLine($"[JWT AUTH ERROR] Inner Exception: {context.Exception.InnerException.Message}");
                    }
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var principal = context.Principal;
                if (principal != null)
                {
                    var claims = string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}"));
                    Console.WriteLine($"[JWT TOKEN VALID] Claims: {claims}");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IAuthService, AuthService>();

var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
});
IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>();
builder.Services.AddHttpClient<IClaudeService, ClaudeService>();
builder.Services.AddHttpClient<IChromaDbService, ChromaDbService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddHttpClient<IOllamaChatService, OllamaChatService>(client => client.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddHttpClient<IOllamaEmbeddingService, OllamaEmbeddingService>(client => client.Timeout = TimeSpan.FromMinutes(5));
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));
builder.Services.AddScoped<IAIChatService, AIChatService>();
builder.Services.AddScoped<ITestCaseService, TestCaseService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

