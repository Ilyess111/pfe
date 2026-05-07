using Soroubat.Api.Interfaces;
using Soroubat.Api.Services;
using Soroubat.Api.Data;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. CONFIGURATION BUSINESS CENTRAL ────────────────────────────────────────

var bcConfig = builder.Configuration.GetSection("BusinessCentral");

string rawUrl      = bcConfig.GetValue<string>("BaseUrl")      ?? throw new InvalidOperationException("'BusinessCentral:BaseUrl' est absent de appsettings.json.");
string companyName = bcConfig.GetValue<string>("CompanyName")  ?? throw new InvalidOperationException("'BusinessCentral:CompanyName' est absent de appsettings.json.");

// On extrait la racine (avant /api/ ou /ODataV4) pour construire les deux URIs
string baseUrl = rawUrl.Split("/api/")[0].Split("/ODataV4")[0].TrimEnd('/');

// URI pour les Custom APIs (siteManagement)
string apiUri   = $"{baseUrl}/api/soroubat/siteManagement/v1.0/companies(name='{Uri.EscapeDataString(companyName)}')/";

// URI pour les Lookups (OData Web Services)
string odataUri = $"{baseUrl}/ODataV4/Company('{Uri.EscapeDataString(companyName)}')/";

// ── 2. AUTHENTIFICATION JWT ───────────────────────────────────────────────────

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clé JWT 'Jwt:Key' est absente de appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            // On utilise UtcNow pour la cohérence avec la génération du token
            ClockSkew                = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ── 3. BASE DE DONNÉES LOCALE (SQLite) ────────────────────────────────────────

builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=auth.db"));

// ── 4. CONTROLLERS + JSON ─────────────────────────────────────────────────────

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// ── 5. SWAGGER ────────────────────────────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title   = "Soroubat API",
        Version = "v1",
        Description = "API de pilotage des chantiers — Intégrée à Microsoft Dynamics 365 Business Central"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name        = "Authorization",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme      = "Bearer",
        BearerFormat = "JWT",
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez 'Bearer' suivi d'un espace et de votre jeton JWT.\n\nExemple : \"Bearer eyJhbGci...\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── 6. INJECTION DE DÉPENDANCES — CLIENTS HTTP BC ────────────────────────────

// Fabrique partagée pour les deux configurations HttpClient
static HttpClientHandler WindowsAuthHandler() =>
    new HttpClientHandler { UseDefaultCredentials = true };

void ConfigureBCClient(HttpClient client)
{
    client.BaseAddress = new Uri(apiUri);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
}

void ConfigureODataClient(HttpClient client)
{
    client.BaseAddress = new Uri(odataUri);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
}

builder.Services
    .AddHttpClient<ISiteManagementService, SiteManagementService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IPurchaseRequestService, PurchaseRequestService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<ITransferService, TransferService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<ILookupService, LookupService>(ConfigureODataClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IStockService, StockService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IChefChantierService, ChefChantierService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IVehiculeService, VehiculeService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IGasoilService, GasoilService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IEmpAttendanceService, AttendanceService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services
    .AddHttpClient<IEmployeeService, EmployeeService>(client => 
    {
        // On construit l'URL spécifiquement pour le groupe 'lookups'
        string baseUrl = builder.Configuration.GetValue<string>("BusinessCentral:BaseUrl").Split("/api/")[0].TrimEnd('/');
        string companyName = builder.Configuration.GetValue<string>("BusinessCentral:CompanyName");
        
        // URL Cible : /api/soroubat/lookups/v1.0/companies(...)
        client.BaseAddress = new Uri($"{baseUrl}/api/soroubat/lookups/v1.0/companies(name='{Uri.EscapeDataString(companyName)}')/");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .ConfigurePrimaryHttpMessageHandler(WindowsAuthHandler);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAlertService, AlertService>();

// ── 7. CORS ───────────────────────────────────────────────────────────────────

builder.Services.AddCors(opt => opt.AddPolicy("AllowAngular", p =>
    p.WithOrigins("http://localhost:4200")
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

// ── 8. PIPELINE HTTP ──────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Backend démarré sur http://localhost:5227");
Console.WriteLine("📚 Swagger disponible sur http://localhost:5227/swagger");

app.Run();