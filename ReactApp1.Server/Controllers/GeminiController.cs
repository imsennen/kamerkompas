using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        public class GeminiRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }

        public class GeminiResponse
        {
            public string Response { get; set; } = string.Empty;
        }

        private readonly string systemInstruction =
            "Je bent een AI-assistent die uitsluitend informatie geeft over de Tweede Kamer der Staten-Generaal van Nederland. " +
            "Je expertise omvat alles wat met de werking, samenstelling, en activiteiten van de Tweede Kamer te maken heeft. " +
            "Je behandelt alleen onderwerpen die direct verband houden met de Tweede Kamer, zoals de rol van Kamerleden, politieke partijen die er zitting in hebben, het wetgevingsproces, interne commissies, debatten, moties, verkiezingen, en relevante actuele gebeurtenissen binnen het parlementaire werk. " +
            "Je beantwoordt geen vragen die buiten dit domein vallen.";

        [HttpPost]
        public async Task<ActionResult<GeminiResponse>> Post([FromBody] GeminiRequest request)
        {
            DotNetEnv.Env.Load();
            var host = Environment.GetEnvironmentVariable("HOST");
            var user = Environment.GetEnvironmentVariable("USER");
            var password = Environment.GetEnvironmentVariable("PASSWORD");
            var database = Environment.GetEnvironmentVariable("DATABASE");
            var projectId = Environment.GetEnvironmentVariable("PROJECT_ID");

            string connectionString = $"Server={host};Database={database};User ID={user};Password={password};";
            string kamerledenText;

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using var cmd = new MySqlCommand("SELECT * FROM tweede_kamer2.kamerleden_2;", conn);
                    using var reader = await cmd.ExecuteReaderAsync();

                    var sb = new StringBuilder();
                    while (await reader.ReadAsync())
                    {
                        var fields = new object[reader.FieldCount];
                        reader.GetValues(fields);
                        sb.AppendLine(string.Join(", ", fields));
                    }
                    kamerledenText = sb.ToString();
                }

                var fullPrompt = $"{systemInstruction}\n\n{kamerledenText}\n\nVraag: {request.Prompt}";

                var geminiResponse = await SendToGeminiAsync(fullPrompt, projectId);
                return Ok(new GeminiResponse { Response = geminiResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GeminiResponse { Response = $"Fout: {ex.Message}" });
            }
        }

        private async Task<string> SendToGeminiAsync(string prompt, string projectId)
        {
            // Load service account and get OAuth token
            var credential = await GoogleCredential.FromFile("service-account.json")
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform")
                .UnderlyingCredential
                .GetAccessTokenForRequestAsync();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", credential);

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var endpoint = $"https://us-central1-aiplatform.googleapis.com/v1/projects/{projectId}/locations/us-central1/publishers/google/models/gemini-1.5-flash:generateContent";

            var response = await httpClient.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString() ?? "[Geen antwoord]";
        }
    }
}
