using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LmStudioController : ControllerBase
    {
        public class LmStudioRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }

        public class LmStudioResponse
        {
            public string Response { get; set; } = string.Empty;
        }

        private readonly string systemInstruction =
            "Je bent een AI-assistent die uitsluitend informatie geeft over de Tweede Kamer der Staten-Generaal van Nederland. " +
            "Je expertise omvat alles wat met de werking, samenstelling, en activiteiten van de Tweede Kamer te maken heeft. " +
            "Je behandelt alleen onderwerpen die direct verband houden met de Tweede Kamer, zoals de rol van Kamerleden, politieke partijen die er zitting in hebben, het wetgevingsproces, interne commissies, debatten, moties, verkiezingen, en relevante actuele gebeurtenissen binnen het parlementaire werk. " +
            "Je beantwoordt geen vragen die buiten dit domein vallen.";

        [HttpPost]
        public async Task<ActionResult<LmStudioResponse>> Post([FromBody] LmStudioRequest request)
        {
            DotNetEnv.Env.Load();
            var host = Environment.GetEnvironmentVariable("HOST");
            var user = Environment.GetEnvironmentVariable("USER");
            var password = Environment.GetEnvironmentVariable("PASSWORD");
            var database = Environment.GetEnvironmentVariable("DATABASE");

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

                var responseText = await SendToLmStudioAsync(fullPrompt);
                return Ok(new LmStudioResponse { Response = responseText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LmStudioResponse { Response = $"Fout: {ex.Message}" });
            }
        }

        private async Task<string> SendToLmStudioAsync(string prompt)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            // Clear any default headers and add Accept only once
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                messages = new[]
                {
            new { role = "system", content = systemInstruction },
            new { role = "user", content = prompt }
        },
                temperature = 0.7,
                max_tokens = 1024,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var response = await httpClient.PostAsync("http://localhost:1234/v1/chat/completions", new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"LM Studio error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("choices", out var choices))
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? "[Geen antwoord]";
            }
            else if (doc.RootElement.TryGetProperty("results", out var results))
            {
                return results[0].GetProperty("text").GetString() ?? "[Geen antwoord]";
            }
            else if (doc.RootElement.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? "[Geen antwoord]";
            }
            else
            {
                return "[Geen antwoord]";
            }
        }

    }
}
