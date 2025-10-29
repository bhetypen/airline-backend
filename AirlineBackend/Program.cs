using AirlineBackend.Common.Auth;
using AirlineBackend.Infrastructure.Mongo;
using AirlineBackend.Modules.Users;
using AirlineBackend.Modules.Airports;
using AirlineBackend.Modules.Routes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Controllers + Swagger Configuration ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Airline API", Version = "v1" });

    // Add JWT bearer authentication scheme to Swagger
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


// --- 2. CORS Configuration ---
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:3000","http://localhost:4000",
                                                                                     "http://localhost:5173","http://localhost:5174","http://localhost:5175")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    ));

// --- 3. MongoDB Configuration ---
// Requires Infrastructure/Mongo/MongoExtensions.cs
builder.Services.AddMongo(builder.Configuration);

// --- 4. JWT Authentication and Authorization Configuration ---

// Binds the "Jwt" configuration section to the JwtOptions class (requires Common/Auth/JwtOptions.cs)
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt");
var keyString = jwt["Key"];

// CRITICAL: This is where the application fails if "Jwt:Key" in appsettings.json is empty.
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
        // Policy requires the custom 'isAdmin' claim, which is set in AuthService.cs
        o.AddPolicy("AdminOnly", p => p.RequireClaim("isAdmin", "true")); 
    });
    
// --- 5. Module Integration ---
builder.Services
    .AddUsersModule(builder.Configuration)
    .AddAirportsModule(builder.Configuration)
    .AddRoutesModule(builder.Configuration);

var app = builder.Build();

// --- 6. Middleware Pipeline ---

// Custom error handling
app.UseExceptionHandler("/error");
app.Map("/error", () => Results.Problem("server error"));

app.UseHttpsRedirection();
app.UseCors(); // Apply CORS policy

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Swagger UI should be run after controllers are mapped
app.UseSwagger();
app.UseSwaggerUI();

app.Run();





