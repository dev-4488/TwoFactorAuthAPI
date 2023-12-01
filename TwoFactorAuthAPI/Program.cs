using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using TwoFactorAuthAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetSection("RedisConnectionString").Get<string>()));

// Register cleanup service with Redis cache
builder.Services.AddHostedService<CodeCleanupService>();

builder.Services.Configure<TwoFactorAuthConfig>(builder.Configuration.GetSection("TwoFactorAuthConfig"));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7109"; // Replace with your IdentityServer4 base URL
        options.Audience = "api1"; // Should match the API scope name

        // Add additional validation parameters if required
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:7109", // Replace with your IdentityServer4 base URL
            ValidAudience = "api1" // Should match the API scope name
        };
    });


//CORS Set-up
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("*") //Change this to allow traffic only from specific applications
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddIdentityServer()
       .AddInMemoryClients(IdentityServerConfig.GetClients())
       .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
       .AddDeveloperSigningCredential(); // For development, using this insecure method

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // Map attribute-routed API controllers
});

app.Run();
