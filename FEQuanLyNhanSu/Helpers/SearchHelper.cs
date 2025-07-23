using FEQuanLyNhanSu.Base;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

namespace FEQuanLyNhanSu.Helpers
{
    public static class SearchHelper
    {
        private static readonly HttpClient _httpClient;

        static SearchHelper()
        {
            var baseUrl = AppsettingConfigHelper.GetBaseUrl();

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("API base URL not found in appsettings.json");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/")
            };
        }

        public static async Task<List<T>> SearchAsync<T>(string endpoint, string keyword, string token)
        {
            try
            {
                var lowerKeyword = keyword.Trim().ToLower();
                var encoded = Uri.EscapeDataString(lowerKeyword);
                var url = $"{endpoint}?Search={encoded}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);

                var status = (int)response.StatusCode;
                var contentType = response.Content.Headers.ContentType?.MediaType;
                //MessageBox.Show($"🔎 Status: {status} | Content-Type: {contentType}");

                var json = await response.Content.ReadAsStringAsync();
                //MessageBox.Show($"📦 Raw Body:\n{json}");
                //if (!response.IsSuccessStatusCode)
                //{
                //    MessageBox.Show($"❌ API error: {response.StatusCode}");
                //    return new List<T>();
                //}

                var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<T>>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result?.Data?.Items?.ToList() ?? new List<T>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search error: {ex.Message}");
                return new List<T>();
            }
        }
    }
}
