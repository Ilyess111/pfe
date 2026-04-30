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

// ─── CONFIGURATION BUSINESS CENTRAL ─────────────────────────────────────────

var bcConfig = builder.Configuration.GetSection("BusinessCentral");
string rawUrl    = bcConfig.GetValue<string>("BaseUrl")     ?? string.Empty;
string companyName = bcConfig.GetValue<string>("CompanyName") ?? "SOROUBATBF-NAV";

string baseUrl = rawUrl.Split("/api/")[0].Split("/ODataV4")[0].TrimEnd('/');

// API Custom (siteManagement) — utilisée par tous les services métier
string apiUri = $"{baseUrl}/api/soroubat/siteManagement/v1.0/companies(name='{Uri.EscapeDataString(companyName)}')/";

// Services Web OData — utilisés uniquement par LookupService
string odataUri = $"{baseUrl}/ODataV4/Company('{Uri.EscapeDataString(companyName)}')/";

// ─── AUTHENTIFICATION JWT ────────────────────────────────────────────────────

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("La clé JWT 'Jwt:Key' est manquante dans la configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ─── BASE DE DONNÉES LOCALE ──────────────────────────────────────────────────

builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlite("Data Source=auth.db")); //data source indique le chemin du fichier de base de données SQLite locale (auth.db à la racine du projet)
// ca veut dire les paramétres sont opt et à l'appel fait : opt.useSqlite("Data Source=auth.db") . opt sera remplacé par DbContextOptions<AuthDbContext> options 
// ─── CONTRÔLEURS & SÉRIALISATION ─────────────────────────────────────────────

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// ─── SWAGGER ─────────────────────────────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title   = "Soroubat API",
        Version = "v1",
        Description = "API de gestion et pilotage des chantiers — intégrée à Business Central."
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

// ─── CONFIGURATION DES CLIENTS HTTP ──────────────────────────────────────────

// Client BC (API Custom) — utilisé par tous les services métier
void ConfigureBCClient(HttpClient client)
{
    client.BaseAddress = new Uri(apiUri);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}

// Client OData — utilisé uniquement par LookupService
void ConfigureODataClient(HttpClient client)
{
    client.BaseAddress = new Uri(odataUri);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}

// Gestionnaire HTTP commun : authentification Windows intégrée (NTLM/Kerberos → BC)
static HttpClientHandler CreateBCHandler() =>
    new HttpClientHandler { UseDefaultCredentials = true };

// ─── INJECTION DES SERVICES ───────────────────────────────────────────────────

builder.Services.AddHttpClient<ISiteManagementService, SiteManagementService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

builder.Services.AddHttpClient<IPurchaseRequestService, PurchaseRequestService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

builder.Services.AddHttpClient<ITransferService, TransferService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

builder.Services.AddHttpClient<IStockService, StockService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

builder.Services.AddHttpClient<IChefChantierService, ChefChantierService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

builder.Services.AddHttpClient<IVehiculeService, VehiculeService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

builder.Services.AddHttpClient<IGasoilService, GasoilService>(ConfigureBCClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

// Lookup : OData uniquement
builder.Services.AddHttpClient<ILookupService, LookupService>(ConfigureODataClient)
    .ConfigurePrimaryHttpMessageHandler(CreateBCHandler);

// Services sans HttpClient propre (Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAlertService, AlertService>();

// ─── CORS ────────────────────────────────────────────────────────────────────

builder.Services.AddCors(opt => opt.AddPolicy("AllowAngular", p =>
    p.WithOrigins("http://localhost:4200")
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

// ─── PIPELINE ────────────────────────────────────────────────────────────────

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

Console.WriteLine("Soroubat API démarrée sur http://localhost:5227");
Console.WriteLine("Swagger disponible sur http://localhost:5227/swagger");

app.Run();