using FaceRecognitionDotNet;
using System.Text.Json;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System.IO;
using System.Drawing;

namespace Soroubat.Api.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmployeeService> _logger;
        private readonly string _modelsPath;

        public EmployeeService(HttpClient httpClient, ILogger<EmployeeService> logger, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _logger = logger;
            // Chemin vers les modèles de reconnaissance (à télécharger et placer dans wwwroot/models)
            _modelsPath = Path.Combine(env.ContentRootPath, "Models", "FaceRef");
        }

        public async Task<bool> VerifyFaceAsync(FaceVerificationRequest request, string? numProjet)
        {
            try
            {
                // 1. Récupérer l'employé depuis BC (avec le champ imageBase64 que vous avez créé en AL)
                var filter = $"matricule eq '{request.Matricule}'";
                var employeeData = await GetEmployeesAsync(numProjet, filter, top: 1);

                if (!employeeData.TryGetProperty("value", out var employees) || employees.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Employé non trouvé pour le matricule {Matricule}", request.Matricule);
                    return false;
                }

                // Extraction du Base64 de BC
                var bcBase64 = employees[0].GetProperty("imageBase64").GetString();
                if (string.IsNullOrEmpty(bcBase64))
                {
                    _logger.LogWarning("L'employé {Matricule} n'a pas de photo de référence dans BC", request.Matricule);
                    return false;
                }

                // 2. Préparation de la comparaison
                // Nettoyage des chaînes Base64 (au cas où elles contiennent des headers data:image/...)
                var cleanCaptured = CleanBase64(request.CapturedImageBase64);
                var cleanReference = CleanBase64(bcBase64);

                return CompareImages(cleanCaptured, cleanReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification faciale");
                return false;
            }
        }


private bool CompareImages(string capturedBase64, string referenceBase64)
{
    // 1. Initialisation du moteur avec le chemin des modèles (.dat)
    using var fr = FaceRecognition.Create(_modelsPath);
    
    // 2. Décodage des chaînes Base64 en tableaux d'octets
    byte[] capturedBytes = Convert.FromBase64String(capturedBase64);
    byte[] referenceBytes = Convert.FromBase64String(referenceBase64);

    // 3. Conversion byte[] -> MemoryStream -> Bitmap -> FaceRecognition Image
    // Cette étape résout l'erreur CS1503 en fournissant le type Bitmap attendu
    using var msCaptured = new MemoryStream(capturedBytes);
    using var bitmapCaptured = new Bitmap(msCaptured);
    using var imgCaptured = FaceRecognition.LoadImage(bitmapCaptured);

    using var msReference = new MemoryStream(referenceBytes);
    using var bitmapReference = new Bitmap(msReference);
    using var imgReference = FaceRecognition.LoadImage(bitmapReference);

    // 4. Extraction des caractéristiques (encodings)
    var encodingCaptured = fr.FaceEncodings(imgCaptured).FirstOrDefault();
    var encodingReference = fr.FaceEncodings(imgReference).FirstOrDefault();

    if (encodingCaptured == null || encodingReference == null)
    {
        _logger.LogWarning("Visage non détecté sur l'une des photos.");
        return false;
    }

    // 5. Comparaison par distance euclidienne
    double distance = FaceRecognition.FaceDistance(encodingCaptured, encodingReference);
    _logger.LogInformation("Distance calculée : {Distance}", distance);

    // Seuil de 0.6 : une distance inférieure signifie qu'il s'agit de la même personne
    return distance < 0.6;
}
private string CleanBase64(string base64)
{
    if (string.IsNullOrEmpty(base64)) return "";
    return base64.Contains(",") ? base64.Split(',')[1] : base64;
}

            public async Task<JsonElement> GetEmployeesAsync(string? numProjet = null, string? filter = null, int? top = null)
    {
        var queryParams = new List<string>();

        // Filtres existants
        var filters = new List<string>();
        if (!string.IsNullOrEmpty(numProjet)) filters.Add($"chantier eq '{numProjet}'");
        if (!string.IsNullOrEmpty(filter)) filters.Add(filter);
        
        if (filters.Any()) 
            queryParams.Add($"$filter={string.Join(" and ", filters)}");

        // LIMITATION : On ajoute le paramètre $top
        if (top.HasValue)
            queryParams.Add($"$top={top.Value}");

        // OPTIMISATION : On ne demande pas l'image si on veut juste tester la liste
        // queryParams.Add("$select=id,matricule,firstName,lastName,fonction"); 

        var requestUri = "employees";
        if (queryParams.Any())
            requestUri += "?" + string.Join("&", queryParams);

        _logger.LogInformation($"[BC Call] URL: {requestUri}");

        var response = await _httpClient.GetAsync(requestUri);
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"BC Error: {response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<JsonElement>();    }


    }
}