using Soroubat.Api.Interfaces;
using Soroubat.Api.Services;
using Soroubat.Api.Data;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var bcConfig = builder.Configuration.GetSection("BusinessCentral");
string rawUrl = bcConfig.GetValue<string>("BaseUrl") ?? "";

string baseUrl = rawUrl.Split("/api/")[0].Split("/ODataV4")[0].TrimEnd('/');
string companyName = bcConfig.GetValue<string>("CompanyName") ?? "SOROUBATBF-NAV";

// Pour les Services de gestion
string apiUri = $"{baseUrl}/api/soroubat/siteManagement/v1.0/companies(name='{Uri.EscapeDataString(companyName)}')/";

// Pour les Lookups (Services Web OData)
string odataUri = $"{baseUrl}/ODataV4/Company('{Uri.EscapeDataString(companyName)}')/";

var jwtKey = builder.Configuration["Jwt:Key"];
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AuthDbContext>(opt => opt.UseSqlite("Data Source=auth.db"));

// --- 3. SERVICES ---
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Soroubat API", Version = "v1" });

    // Configuration de la définition de sécurité pour JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez 'Bearer' suivi d'un espace et de votre jeton JWT.\n\nExemple: \"Bearer eyJhbGci...\""
    });

    // Activation de la sécurité globalement dans Swagger
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddHttpClient<ISiteManagementService, SiteManagementService>(client => {
    client.BaseAddress = new Uri(apiUri);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

builder.Services.AddHttpClient<IPurchaseRequestService, PurchaseRequestService>(client => {
    client.BaseAddress = new Uri(apiUri);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

builder.Services.AddHttpClient<ITransferService, TransferService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

// ON FORCE UN CLIENT DIFFÉRENT POUR LE LOOKUP
builder.Services.AddHttpClient<ILookupService, LookupService>(client => {
    client.BaseAddress = new Uri(odataUri); 
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

builder.Services.AddHttpClient<IStockService, StockService>(client => {
    client.BaseAddress = new Uri(apiUri); 
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddCors(opt => opt.AddPolicy("AllowAngular", p => 
    p.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

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

// Afficher l'URL de démarrage
Console.WriteLine("🚀 Backend démarré sur http://localhost:5227");
Console.WriteLine("📚 Swagger disponible sur http://localhost:5227/swagger");


app.Run();