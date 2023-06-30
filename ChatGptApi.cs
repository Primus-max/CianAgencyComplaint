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

    // Добавляем поля для прокси-сервера, пароля, порта и сервера
    private readonly string PROXY_SERVER = "168.196.236.11";
    private readonly int PROXY_PORT = 50100;
    private readonly string PROXY_USERNAME = "urkytsk3";
    private readonly string PROXY_PASSWORD = "fNBZjoyVt6";

    public ChatGptApi(string apiKey)
    {
        API_KEY = apiKey;


        // Создаем HttpClientHandler для настройки прокси
        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy(PROXY_SERVER, PROXY_PORT)
            {
                Credentials = new NetworkCredential(PROXY_USERNAME, PROXY_PASSWORD)
            },
            UseProxy = true
        };

        httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.Add("authorization", $"Bearer {API_KEY}");
    }

    public async Task<string> GetChatGptResponse(string prompt)
    {
        try
        {
            var content = new StringContent("{\"model\": \"text-davinci-edit-001\", \"prompt\": \"" + prompt + "\",\"temperature\": 0.3,\"max_tokens\": 25}",
                Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("https://api.openai.com/v1/completions", content);
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

