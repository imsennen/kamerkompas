using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/gemini")]
    public class GeminiController : ControllerBase
    {
        //incoming
        public class GeminiRequest
        {
            public List<GeminiChatMessage> Messages { get; set; } = new();
        }

        //outgoing
        public class GeminiResponse
        {
            public string Response { get; set; } = string.Empty;
            public List<GeminiChatMessage> Messages { get; set; } = new();
        }

        //chat message with role and content
        public class GeminiChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty; // "user", "system", "assistant"

            [JsonPropertyName("content")] 
            public string Content { get; set; } = string.Empty;
        }

        //system instruction for the ai assistant
        private readonly string systemInstruction =
            "Je bent een AI-assistent die uitsluitend informatie geeft over de Tweede Kamer der Staten-Generaal van Nederland. " +
            "Je expertise omvat alles wat met de werking, samenstelling, en activiteiten van de Tweede Kamer te maken heeft. " +
            "Je behandelt alleen onderwerpen die direct verband houden met de Tweede Kamer, zoals de rol van Kamerleden, politieke partijen die er zitting in hebben, het wetgevingsproces, interne commissies, debatten, moties, verkiezingen, en relevante actuele gebeurtenissen binnen het parlementaire werk. " +
            "Je beantwoordt geen vragen die buiten dit domein vallen.";

        [HttpPost]
        public async Task<ActionResult<GeminiResponse>> Post([FromBody] GeminiRequest request)
        {
            // Create message history: first message is the system instruction only
            var fullMessages = new List<GeminiChatMessage>
            {
                new GeminiChatMessage { Role = "system", Content = systemInstruction }
            };
            fullMessages.AddRange(request.Messages);

            try
            {
                // Send to Gemini
                var assistantReply = await SendToGeminiAsync(fullMessages);

                // Add response to history
                request.Messages.Add(new GeminiChatMessage { Role = "assistant", Content = assistantReply });

                // Return response with messages
                return Ok(new GeminiResponse
                {
                    Response = assistantReply,
                    Messages = request.Messages
                });
            }
            catch (Exception ex)
            {
                // Error handling
                return StatusCode(500, new GeminiResponse
                {
                    Messages = request.Messages.Append(new GeminiChatMessage
                    {
                        Role = "system",
                        Content = $"Er gaat iets fout: {ex.Message}"
                    }).ToList()
                });
            }
        }

        private async Task<string> SendToGeminiAsync(List<GeminiChatMessage> messages)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new { messages = messages };
            var json = JsonSerializer.Serialize(requestBody);

            // Change the URL to your Gemini backend
            var response = await httpClient.PostAsync("http://localhost:5000/chat", new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini AI error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("response").GetString() ?? "[Geen antwoord]";
        }
    }
}