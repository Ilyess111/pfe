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

        public async Task<IEnumerable<PurchaseRequestDto>> GetAllRequestsAsync(string projectNo)
        {
            // On ajoute le filtre pour ne récupérer que les demandes du projet concerné
            var url = $"purchaseRequests?$filter=jobNo eq '{projectNo}'";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BCResponse<PurchaseRequestDto>>();
                
                if (result?.Value == null)
                {
                    return new List<PurchaseRequestDto>();
                }
                
                return result.Value;
            }
            
            // Si BC répond avec une erreur, on utilise votre gestionnaire centralisé
            await HandleErrorResponse(response);
            return null;
        }

        public async Task<PurchaseRequestDto> GetRequestByIdAsync(Guid id, string projectNo)
        {
            // On ajoute $expand=purchaseRequestLines pour forcer BC à envoyer les lignes
            var url = $"purchaseRequests({id})?$expand=purchaseRequestLines";
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var request = await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();
                
                // Vérification de sécurité (déjà en place)
                if (request != null && request.JobNo != projectNo)
                {
                    throw new UnauthorizedAccessException("Accès refusé à ce projet.");
                }
                
                return request;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<PurchaseRequestDto> CreateHeaderAsync(PurchaseRequestDto header, string projectNo)
        {
            // SÉCURITÉ : On force le numéro de projet extrait du JWT.
            // Même si un utilisateur modifie le JSON côté client, le backend écrasera la valeur.
            header.JobNo = projectNo;

            // On prépare le contenu pour Business Central
            var content = new StringContent(JsonSerializer.Serialize(header), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("purchaseRequests", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PurchaseRequestDto>();
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<bool> CreateLinesAsync(List<PurchaseRequestLineDto> lines, string projectNo)
        {
            foreach (var line in lines)
            {
                // SÉCURITÉ : On force le projet sur chaque ligne
                line.JobNo = projectNo;

                var content = new StringContent(JsonSerializer.Serialize(line), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("purchaseRequestLines", content);

                if (!response.IsSuccessStatusCode)
                {
                    // Si une ligne échoue, on log l'erreur et on peut choisir d'arrêter ou de continuer
                    await HandleErrorResponse(response);
                    return false; 
                }
            }
            return true;
        }

        public async Task<bool> PatchHeaderAsync(Guid id, PurchaseRequestDto header, string projectNo)
        {
            // 1. Récupérer l'existant pour avoir l'ETag
            var responseGet = await _httpClient.GetAsync($"purchaseRequests({id})");
            if (!responseGet.IsSuccessStatusCode) return false;

            // Extraire l'ETag des en-têtes de la réponse
            var etag = responseGet.Headers.ETag?.ToString();

            // 2. Préparer la requête PATCH
            var json = JsonSerializer.Serialize(header, new JsonSerializerOptions { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"purchaseRequests({id})")
            {
                Content = content
            };

            // 3. Ajouter le jeton de concurrence (ETag)
            // "*" signifie "forcer la mise à jour peu importe la version" (Pratique pour les tests)
            // Pour être rigoureux, utilisez la variable 'etag' récupérée plus haut.
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var responsePatch = await _httpClient.SendAsync(request);

            if (!responsePatch.IsSuccessStatusCode)
            {
                await HandleErrorResponse(responsePatch);
            }

            return responsePatch.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRequestAsync(Guid id, string projectNo)
        {
            // 1. VÉRIFICATION : On récupère la demande pour vérifier le JobNo
            var getResponse = await _httpClient.GetAsync($"purchaseRequests({id})");
            if (!getResponse.IsSuccessStatusCode) return false;

            var request = await getResponse.Content.ReadFromJsonAsync<PurchaseRequestDto>();

            // Sécurité : Vérifier si le projet correspond
            if (request != null && request.JobNo != projectNo)
            {
                throw new UnauthorizedAccessException("Suppression refusée : Cette demande n'appartient pas à votre projet.");
            }

            // 2. SUPPRESSION : Si la vérification passe, on supprime
            var response = await _httpClient.DeleteAsync($"purchaseRequests({id})");

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorResponse(response);
            }

            return response.IsSuccessStatusCode;
        }

        // --- MÉTHODES LIGNES (LINES) ---

        public async Task<bool> PatchLineAsync(Guid lineId, PurchaseRequestLineDto lineDto, string projectNo)
        {
            // 1. Récupération de la ligne pour vérifier l'appartenance au projet et obtenir l'ETag
            var getResponse = await _httpClient.GetAsync($"purchaseRequestLines({lineId})");
            if (!getResponse.IsSuccessStatusCode) return false;

            var existingLine = await getResponse.Content.ReadFromJsonAsync<PurchaseRequestLineDto>();
            var etag = getResponse.Headers.ETag?.ToString();

            // SÉCURITÉ : On vérifie que la ligne appartient bien au projet du chef de chantier connecté
            if (existingLine == null || !existingLine.JobNo.Equals(projectNo, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Vous n'avez pas l'autorisation de modifier cette ligne.");
            }

            // 2. Préparation du PATCH
            // On force le JobNo au cas où il aurait été modifié dans le DTO
            lineDto.JobNo = projectNo; 
            
            var json = JsonSerializer.Serialize(lineDto, new JsonSerializerOptions { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"purchaseRequestLines({lineId})")
            {
                Content = content
            };

            // 3. Gestion de la concurrence (Indispensable pour BC)
            request.Headers.TryAddWithoutValidation("If-Match", etag ?? "*");

            var patchResponse = await _httpClient.SendAsync(request);

            if (!patchResponse.IsSuccessStatusCode)
            {
                await HandleErrorResponse(patchResponse);
            }

            return patchResponse.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteLineAsync(Guid lineId, string projectNo)
        {
            // 1. VÉRIFICATION : On récupère la ligne pour vérifier son appartenance
            var responseGet = await _httpClient.GetAsync($"purchaseRequestLines({lineId})");
            
            if (!responseGet.IsSuccessStatusCode) return false;

            var line = await responseGet.Content.ReadFromJsonAsync<PurchaseRequestLineDto>();

            // SÉCURITÉ : On vérifie si le JobNo de la ligne correspond au projet du chef
            if (line != null && line.JobNo != projectNo)
            {
                throw new UnauthorizedAccessException("Action refusée : Cette ligne appartient à un projet qui ne vous est pas assigné.");
            }

            // 2. SUPPRESSION : Si c'est valide, on procède au DELETE
            var responseDelete = await _httpClient.DeleteAsync($"purchaseRequestLines({lineId})");

            if (!responseDelete.IsSuccessStatusCode)
            {
                await HandleErrorResponse(responseDelete);
            }

            return responseDelete.IsSuccessStatusCode;
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
