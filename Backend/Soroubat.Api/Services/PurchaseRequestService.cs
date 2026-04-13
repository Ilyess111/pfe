using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using Microsoft.Extensions.Logging;

namespace Soroubat.Api.Services 
{
    public class PurchaseRequestService :  BaseService, IPurchaseRequestService 
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PurchaseRequestService> _logger;


        public PurchaseRequestService(HttpClient httpClient , ILogger<PurchaseRequestService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // --- MÉTHODES EN-TÊTE (HEADER) ---

        public async Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync()
        {
            var response = await _httpClient.GetAsync("purchaseRequests"); // purchaseRequests est ajouté au baseAddress dans Program.cs
            
            if (response.IsSuccessStatusCode)
            {
                // readFromJsonAsync assure que le json retourné par BC (texte brut) est désérialisé en un objet BCResponse<PurchaseRequestDto>
                var result = await response.Content.ReadFromJsonAsync<BCResponse<PurchaseRequestDto>>(); 
                
                // Contrôle strict sur result : on s'assure que la liste existe bien
                if (result?.Value == null)
                {
                    throw new Exception("Impossible de lire la liste des demandes : format de données BC invalide.");
                }

                return result.Value; // on utilise .Value ici car on s'attend à une collection d'entités, même si elle est vide, et pas à un objet unique
            }

            await HandleErrorResponse(response);
            throw new Exception("Erreur lors de la récupération des demandes.");
        }

        public async Task<PurchaseRequestDto> GetRequestByIdAsync(Guid id)
        {
            // On ajoute ?$expand=purchaseRequestLines à l'URL
            var response = await _httpClient.GetAsync($"purchaseRequests({id})?$expand=purchaseRequestLines");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();
                return result; 
                // Les lignes seront automatiquement remplies dans result.PurchaseRequestLines 
                // grâce au $expand et à la désérialisation JSON.
            }
            
            await HandleErrorResponse(response);
            return null;
        }

// 1. Création du Header uniquement
        public async Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header)
        {
            // On s'assure que les lignes sont nulles pour ne pas déclencher le deep insert de BC
            header.PurchaseRequestLines = null;

            var response = await _httpClient.PostAsJsonAsync("purchaseRequests", header);
            if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
            
            return await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();
        }

        // 2. Création d'une ligne individuelle
        public async Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines)
        {
            if (lines == null || !lines.Any()) return false;

            bool globalSuccess = true;
            
            // ÉTAPE 1 : Récupérer dynamiquement le dernier numéro en base
            string docNo = lines.First().DocumentNo;
            int lastLineNo = await GetLastLineNoAsync(docNo);
            
            // ÉTAPE 2 : Commencer l'incrément après le dernier numéro trouvé
            int nextLineNo = lastLineNo + 10000;

            foreach (var line in lines)
            {
                line.LineNo = nextLineNo;
                
                var response = await _httpClient.PostAsJsonAsync("purchaseRequestLines", line);
                
                if (response.IsSuccessStatusCode)
                {
                    nextLineNo += 10000;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erreur ligne {line.No}: {error}");
                    globalSuccess = false;
                }
            }
            return globalSuccess;
        }

        public async Task<bool> UpdateHeaderAsync(Guid id, object partialUpdate)
        {
            var url = $"purchaseRequests({id})";
            return await SendPatchRequest(url, partialUpdate);
        }
        public async Task<bool> DeleteRequestAsync(Guid id)
        {
            // Cible l'entité purchaseRequests. BC se charge de nettoyer les lignes.
            var response = await _httpClient.DeleteAsync($"purchaseRequests({id})");
            if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
            return response.IsSuccessStatusCode;
        }

        // --- MÉTHODES LIGNES (LINES) ---

        public async Task<bool> UpdateLineAsync(Guid lineId, object partialUpdate)
        {
            var url = $"purchaseRequestLines({lineId})";
            return await SendPatchRequest(url, partialUpdate);
        }

        public async Task<bool> DeleteLineAsync(Guid lineId)
        {
            // Cible l'entité purchaseRequestLines. Seule cette ligne disparaît.
            var response = await _httpClient.DeleteAsync($"purchaseRequestLines({lineId})");
            if (!response.IsSuccessStatusCode) await HandleErrorResponse(response);
            return response.IsSuccessStatusCode;
        }

        // --- OUTILS PRIVÉS ---

        private async Task<bool> SendPatchRequest(string url, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
            
            // Obligatoire pour Business Central (OData) lors d'un PATCH
            request.Headers.Add("If-Match", "*"); 

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode) 
                await HandleErrorResponse(response);
                
            return response.IsSuccessStatusCode;
        }

        // ce que fait exactement cette méthode : elle lit le contenu de la réponse d'erreur de BC, 
        // essaie de le désérialiser en un objet BCResponseError pour extraire le message d'erreur spécifique de BC, et si la désérialisation échoue (par exemple si le format de l'erreur n'est pas celui attendu), elle lance une exception générique avec le code d'état HTTP et le contenu brut de l'erreur
        private new async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            try {
                var bcError = JsonSerializer.Deserialize<BCResponseError>(errorContent);
                throw new Exception(bcError?.Error?.Message ?? errorContent);
            } catch (JsonException) {
                throw new Exception($"Réponse de Business Central illisible (Format JSON invalide). Code HTTP {(int)response.StatusCode}. Contenu brut : {errorContent}");
            }
    
        }

        private async Task<int> GetLastLineNoAsync(string documentNo)
        {
            // On appelle BC pour avoir la dernière ligne de ce document précis
            // On trie par LineNo descendant et on en prend 1 ($top=1)
            var response = await _httpClient.GetAsync($"purchaseRequestLines?$filter=documentNo eq '{documentNo}'&$orderby=lineNo desc&$top=1");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement.GetProperty("value");

                if (root.GetArrayLength() > 0)
                {
                    return root[0].GetProperty("lineNo").GetInt32();
                }
            }
            return 0; // Si aucune ligne n'existe, on commence à 0
        }


    }

}
