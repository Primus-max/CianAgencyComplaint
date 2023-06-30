using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

public class ChatGptApi
{
    private readonly string API_KEY;
    private readonly HttpClient httpClient;

    public ChatGptApi(string apiKey)
    {
        API_KEY = apiKey;
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("authorization", $"Bearer {API_KEY}");
    }

    public async Task<string> GetChatGptResponse(string prompt)
    {
        try
        {
            var requestModel = new GptApiRequest
            {
                Model = "text-davinci-003",
                Prompt = prompt,
                Temperature = 0.3,
                MaxTokens = 25,
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestModel),
                Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            var dyData = JsonConvert.DeserializeObject<dynamic>(responseString);
            return dyData!.choices[0].text;
        }
        catch (Exception ex)
        {
            return $"ОШИБКА! Не удалось получить ответ от API: {ex.Message}";
        }
    }
}

public class GptApiRequest
{
    public string? Model { get; set; }
    public string? Prompt { get; set; }
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }

}