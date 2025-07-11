﻿using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/lmstudio")]
    public class LmStudioController : ControllerBase
    {
        //incoming
        public class LmStudioRequest
        {
            public List<LMStudioChatMessage> Messages { get; set; } = new();
        }

        //outgoing
        public class LmStudioResponse
        {
            public string Response { get; set; } = string.Empty;
            public List<LMStudioChatMessage> Messages { get; set; } = new();
        }

        //chat message with role and content
        public class LMStudioChatMessage
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
        public async Task<ActionResult<LmStudioResponse>> Post([FromBody] LmStudioRequest request)
        {
            try
            {
                // create message history, first message is role and instruction (no database data)
                var fullMessages = new List<LMStudioChatMessage>
                {
                    new LMStudioChatMessage { Role = "system", Content = systemInstruction }
                };
                fullMessages.AddRange(request.Messages);

                // Send to LMStudio
                var assistantReply = await SendToLmStudioAsync(fullMessages);

                // Add response to history
                request.Messages.Add(new LMStudioChatMessage { Role = "assistant", Content = assistantReply });

                // return response with messages
                return Ok(new LmStudioResponse
                {
                    Response = assistantReply,
                    Messages = request.Messages
                });
            }
            catch (Exception ex)
            {
                // error handling
                return StatusCode(500, new LmStudioResponse
                {
                    Messages = request.Messages.Append(new LMStudioChatMessage
                    {
                        Role = "system",
                        Content = $"Er gaat iets fout: {ex.Message}"
                    }).ToList()
                });
            }
        }

        private async Task<string> SendToLmStudioAsync(List<LMStudioChatMessage> messages)
        {
            // to prevent timeout
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // create requestbody
            var requestBody = new
            {
                messages = messages,
                temperature = 0.7,
                max_tokens = 1024,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);

            // connect to LMStudio API
            var response = await httpClient.PostAsync("http://localhost:1234/v1/chat/completions", new StringContent(json, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            // error handling
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