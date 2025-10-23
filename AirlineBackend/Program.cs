using AirlineBackend.Common.Auth;
using AirlineBackend.Infrastructure.Mongo;
using AirlineBackend.Modules.Users;
using AirlineBackend.Modules.Airports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Controlers + Swaggers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Airline API", Version = "v1" });

    // Add JWT bearer auth
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Cors
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:3000","http://localhost:4000",
                                                                                     "http://localhost:5173","http://localhost:5174","http://localhost:5175")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    ));

//Mongo
builder.Services.AddMongo(builder.Configuration);

//JWT

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt");
var keyString = jwt["Key"];
if(string.IsNullOrWhiteSpace(keyString))
    throw new InvalidOperationException("Missing configuration: Jwt:Key");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => 
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, 
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
            
    });

builder.Services.AddAuthorization(o => 
    {
        o.AddPolicy("AdminOnly", p => p.RequireClaim("isAdmin", "true")); 
    });
    
//Users Module
builder.Services.AddUsersModule(builder.Configuration);

//Admin Airports Module
builder.Services.AddAirportsModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler("/error");
app.Map("/error", () => Results.Problem("server error"));

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();




