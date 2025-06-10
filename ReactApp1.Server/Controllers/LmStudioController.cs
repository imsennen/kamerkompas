using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LmStudioController : ControllerBase
    {
        //incoming
        public class LmStudioRequest
        {
            public List<ChatMessage> Messages { get; set; } = new();
        }

        //outgoing
        public class LmStudioResponse
        {
            public string Response { get; set; } = string.Empty;
            public List<ChatMessage> Messages { get; set; } = new();
        }

        public class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty; // "user", "system", "assistant"

            [JsonPropertyName("content")] 
            public string Content { get; set; } = string.Empty;
        }

        //system instruction
        private readonly string systemInstruction =
            "Je bent een AI-assistent die uitsluitend informatie geeft over de Tweede Kamer der Staten-Generaal van Nederland. " +
            "Je expertise omvat alles wat met de werking, samenstelling, en activiteiten van de Tweede Kamer te maken heeft. " +
            "Je behandelt alleen onderwerpen die direct verband houden met de Tweede Kamer, zoals de rol van Kamerleden, politieke partijen die er zitting in hebben, het wetgevingsproces, interne commissies, debatten, moties, verkiezingen, en relevante actuele gebeurtenissen binnen het parlementaire werk. " +
            "Je beantwoordt geen vragen die buiten dit domein vallen.";

        [HttpPost]
        public async Task<ActionResult<LmStudioResponse>> Post([FromBody] LmStudioRequest request)
        {
            DotNetEnv.Env.Load();       //load database credentials         todo: add more data en in andere file
            var host = Environment.GetEnvironmentVariable("HOST");
            var user = Environment.GetEnvironmentVariable("USER");
            var password = Environment.GetEnvironmentVariable("PASSWORD");
            var database = Environment.GetEnvironmentVariable("DATABASE");

            string connectionString = $"Server={host};Database={database};User ID={user};Password={password};";
            string kamerledenText;

            try //get all kamerleden_2 data in kamerledenText string
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

                // create message history
                var fullMessages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "system", Content = $"{systemInstruction}\n\n{kamerledenText}" }
                };
                fullMessages.AddRange(request.Messages);

                // Send to LMStudio
                var assistantReply = await SendToLmStudioAsync(fullMessages);

                // Add response to history
                request.Messages.Add(new ChatMessage { Role = "assistant", Content = assistantReply });

                return Ok(new LmStudioResponse { Messages = request.Messages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LmStudioResponse
                {
                    Messages = request.Messages.Append(new ChatMessage
                    {
                        Role = "system",
                        Content = $"Er gaat iets fout: {ex.Message}"
                    }).ToList()
                });
            }
        }

        private async Task<string> SendToLmStudioAsync(List<ChatMessage> messages)
        {

            //to prevent timeout
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //create requestbody
            var requestBody = new
            {
                messages = messages,
                temperature = 0.7,
                max_tokens = 1024,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);

            //connect to api
            var response = await httpClient.PostAsync("http://localhost:1234/v1/chat/completions", new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"LM Studio error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("choices", out var choices))
            {
                return choices[0].GetProperty("message").GetProperty("content").GetString() ?? "[Geen antwoord]";
            }

            return "[Geen antwoord]";
        }

    }
}
