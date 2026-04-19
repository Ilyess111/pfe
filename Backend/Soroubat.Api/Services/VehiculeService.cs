using Newtonsoft.Json;
using Soroubat.Api.Interfaces;
using Soroubat.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Soroubat.Api.Services
{
    public class VehiculeService : BaseService , IVehiculeService
    {
        private readonly HttpClient _httpClient;

        public VehiculeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public async Task<IEnumerable<VehiculePointageHeader>> GetHeadersByJobAsync(string jobNo)
        {
        var url = $"vehiculePointageHeaders?$filter=jobNo eq '{jobNo}'&$expand=vehiculePointageLines";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<BCResponse<VehiculePointageHeader>>(content);
        
        return result.Value;
        }


        public async Task<VehiculePointageHeader> GetHeaderByIdAsync(Guid id)
        {

            var url = $"vehiculePointageHeaders({id})?$expand=vehiculePointageLines";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VehiculePointageHeader>(content);
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<VehiculePointageHeader> CreateHeaderAsync(VehiculePointageHeader header)
        {
            // On définit les paramètres de sérialisation
            var settings = new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore,
                // On force le format de date attendu par Business Central (Edm.Date)
                DateFormatString = "yyyy-MM-dd" 
            };
            
            string json = JsonConvert.SerializeObject(header, settings);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("vehiculePointageHeaders", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VehiculePointageHeader>(responseBody);
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<VehiculePointageHeader> UpdateHeaderAsync(Guid id, VehiculePointageHeader header)
        {
            var settings = new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd" 
            };

            string json = JsonConvert.SerializeObject(header, settings);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // CRUCIAL : On ajoute l'étoile pour dire à BC d'écraser la donnée
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.PatchAsync($"vehiculePointageHeaders({id})", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VehiculePointageHeader>(responseBody);
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<bool> DeleteHeaderAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"vehiculePointageHeaders({id})");
            return response.IsSuccessStatusCode;
        }

        public async Task<VehiculePointageLine> AddLineAsync(VehiculePointageLine line)
        {
            // On utilise les réglages Newtonsoft pour ignorer l'ID s'il est nul
            var settings = new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore 
            };
            
            string json = JsonConvert.SerializeObject(line, settings);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // On utilise PostAsync au lieu de PostAsJsonAsync
            var response = await _httpClient.PostAsync("vehiculePointageLines", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VehiculePointageLine>(responseBody);
            }

            // Capture l'erreur 400 détaillée de Business Central
            await HandleErrorResponse(response);
            return null;
        }

        public async Task<VehiculePointageLine> UpdateLineAsync(Guid id, VehiculePointageLine line)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string json = JsonConvert.SerializeObject(line, settings);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // On force l'écrasement avec l'étoile
            _httpClient.DefaultRequestHeaders.Remove("If-Match");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*");

            var response = await _httpClient.PatchAsync($"vehiculePointageLines({id})", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VehiculePointageLine>(responseBody);
            }

            await HandleErrorResponse(response);
            return null;
        }

        public async Task<bool> DeleteLineAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"vehiculePointageLines({id})");
            return response.IsSuccessStatusCode;
        }
    }
}