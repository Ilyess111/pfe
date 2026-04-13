using Soroubat.Api.Interfaces;
using Soroubat.Api.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var bcConfig = builder.Configuration.GetSection("BusinessCentral");
string rawUrl = bcConfig.GetValue<string>("BaseUrl") ?? "";

string baseUrl = rawUrl.Split("/api/")[0].Split("/ODataV4")[0].TrimEnd('/');
string companyName = bcConfig.GetValue<string>("CompanyName") ?? "SOROUBATBF-NAV";

// Pour les Services de gestion
string apiUri = $"{baseUrl}/api/soroubat/siteManagement/v1.0/companies(name='{Uri.EscapeDataString(companyName)}')/";

// Pour les Lookups (Services Web OData)
string odataUri = $"{baseUrl}/ODataV4/Company('{Uri.EscapeDataString(companyName)}')/";

// --- 3. SERVICES ---
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { 
    UseDefaultCredentials = true 
});

builder.Services.AddCors(opt => opt.AddPolicy("AllowAngular", p => 
    p.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// Afficher l'URL de démarrage
Console.WriteLine("🚀 Backend démarré sur http://localhost:5227");
Console.WriteLine("📚 Swagger disponible sur http://localhost:5227/swagger");


app.Run();