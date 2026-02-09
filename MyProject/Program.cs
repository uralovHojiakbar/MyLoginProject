using Application.Common.Interfaces;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Channels;
using Application.Common.Interfaces;
using Infrastructure.Emailing;
using MyProject.Middleware;
using Application.Services;
using Application.Common.Interfaces;
using Infrastructure.Persistence;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("UserDbConnection")
    )
);

builder.Services.AddScoped<IUserAdminRepository, UserAdminRepository>();
builder.Services.AddScoped<UsersAdminService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<AuthService>();
// Email async queue
builder.Services.AddSingleton(Channel.CreateUnbounded<EmailMessage>());
builder.Services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
builder.Services.AddHostedService<EmailBackgroundService>();

// Middleware
builder.Services.AddScoped<UserStatusMiddleware>();

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyProject API v1");
        c.RoutePrefix = "swagger"; // swagger ui path: /swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserStatusMiddleware>();




app.MapControllers();

app.Run();
