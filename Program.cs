using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BluebirdCore.Data;
using BluebirdCore.Services;
using BluebirdCore.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Hangfire;
using Hangfire.SqlServer;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// ===== FIX JWT CLAIM MAPPING =====
// This prevents ASP.NET Core from mapping JWT claims to Microsoft claims
// JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// ===== DATABASE CONFIGURATION =====
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

// ===== CONFIGURATION BINDING =====
builder.Services.Configure<SchoolSettings>(
    builder.Configuration.GetSection(SchoolSettings.SectionName));

// ===== SERVICES REGISTRATION =====
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IReportCardService, ReportCardService>();
builder.Services.AddScoped<ReportCardPdfService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IPdfMergeService, PdfMergeService>();
builder.Services.AddScoped<MarkSchedulePdfService>();
builder.Services.AddScoped<ExamAnalysisPdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IBabyClassSkillService, BabyClassSkillService>();
builder.Services.AddScoped<IHomeroomService, HomeroomService>();

// Add HttpClientFactory for SMS service
builder.Services.AddHttpClient();

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration                                         
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();



// ===== CONTROLLERS =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Add custom DateTime converter to serialize in local time
        options.JsonSerializerOptions.Converters.Add(new LocalDateTimeConverter());
    });

// ===== JWT AUTHENTICATION =====
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Remove the duplicate and fix the structure
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
        };
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                
                // Map the "role" claim to the standard role claim type
                var roleClaim = claimsIdentity?.FindFirst("role");
                if (roleClaim != null)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                }
                
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// ===== SWAGGER CONFIGURATION =====
var schoolSettings = new SchoolSettings();
builder.Configuration.GetSection(SchoolSettings.SectionName).Bind(schoolSettings);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Management System API",
        Version = "v2.0",
        Description = "A comprehensive School Management System API for managing students, teachers, subjects, exams, and report cards",
        Contact = new OpenApiContact
        {
            Name = "School Management System",
            Email = schoolSettings.Email
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // // Include XML comments
    // var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// ===== CORS CONFIGURATION =====
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173", "http://localhost:3000", "http://127.0.0.1:5173", "http://127.0.0.1:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();  // Required for JWT tokens in Authorization header
    });
});

// ===== LOGGING =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// Configure the HTTP request pipeline

// ===== CORS must be one of the first middleware components =====
app.UseCors("AllowAll"); // In production, use a restricted policy (see comment above)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Management System API v1");
        c.RoutePrefix = "";
        c.DisplayRequestDuration();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    
    try
    {
        // Initialize database and run migrations
        await initializer.InitializeAsync();
        
        // Seed initial data
        await seeder.SeedAsync();
        
        Console.WriteLine("Database setup completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during database setup");
        Console.WriteLine($"Error during database setup: {ex.Message}");
        
        // Don't stop the application, but log the error
        Console.WriteLine("Application will continue running. Please check database configuration.");
    }
}

// ===== STARTUP MESSAGE =====
app.Logger.LogInformation("School Management System API started successfully");
app.Logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
app.Logger.LogInformation($"Swagger UI available at: /");


app.Run();

// Custom DateTime converter to serialize in local time
public class LocalDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Serialize DateTime in local time format without timezone indicator
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"));
    }
}