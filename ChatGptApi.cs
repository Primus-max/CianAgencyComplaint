
using OpenAI_API;
using OpenAI_API.Chat;
using System.Text;

public class ChatGptApi
{
    private readonly OpenAIAPI api;
    private readonly string API_KEY = "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";

    public ChatGptApi()
    {
        api = new OpenAIAPI(API_KEY);
    }

    public class Complaint
    {
        public List<string>? Phrases { get; set; }
        public List<string>? Templates { get; set; }
    }

    public async Task<string> GetChatGptResponse(string prompt)
    {
        ChatRequest chatRequest = new ChatRequest
        {
            Temperature = 0.2,
            MaxTokens = 200,
            Model = "gpt-3.5-turbo-16k"
        };

        var chat = api.Chat.CreateConversation(chatRequest);

        List<Complaint> complaints = new List<Complaint>
    {
        new Complaint
        {
            Phrases = new List<string>
            {
                "Предложение уже неактуально",
                "Вымышленный объект"
            },
            Templates = new List<string>
            {
                "Я нашел объявление {phrase1}, но это не соответствует действительности. Пожалуйста, примите меры и удалите это объявление.",
                "Объявление {phrase2} не является правдивым. Я ожидал другого объекта. Пожалуйста, удалите его с сайта."
            }
        },
        new Complaint
        {
            Phrases = new List<string>
            {
                "По телефону отвечают, что ничего не предлагают",
                "Указан чужой телефон"
            },
            Templates = new List<string>
            {
                "Я пытался связаться по указанному номеру, но мне сказали, что {phrase1}. Пожалуйста, проверьте этот номер и удалите объявление, если оно недействительно.",
                "В объявлении указан чужой номер телефона ({phrase2}). Я никогда не смогу связаться с вами по этому номеру. Пожалуйста, исправьте эту информацию."
            }
        },
        new Complaint
        {
            Phrases = new List<string>
            {
                "Цена завышена",
                "Обман"
            },
            Templates = new List<string>
            {
                "Цена указана слишком высокая для этого объекта ({phrase1}). Я ожидал получить лучшую сделку. Пожалуйста, пересмотрите цену или удалите объявление.",
                "Меня обманули с ценой ({phrase2}). Это нечестно и неправильно. Пожалуйста, примите меры и исправьте эту ситуацию."
            }
        },
        // Добавьте другие варианты жалоб с различными фразами и шаблонами
    };

        Random random = new Random();
        Complaint randomComplaint = complaints[random.Next(complaints.Count)];

        StringBuilder systemMessageBuilder = new StringBuilder();
        systemMessageBuilder.AppendLine(" - Ты пишешь жалобу на сайте по покупке, аренде жилья.");
        systemMessageBuilder.AppendLine(" - Тебе даётся фраза, ты её дополняешь на русском языке.");
        systemMessageBuilder.AppendLine(" - Сообщение должно состоять максимум из 10-15 слов.");
        systemMessageBuilder.AppendLine(" - Ты должна убеждать в своей правде, чтобы сайт отреагировал и заблокировал объявление.");
        systemMessageBuilder.AppendLine(" - Не ссылайся на половые неравенства.");
        systemMessageBuilder.AppendLine(" - Меньше официального тона.");
        systemMessageBuilder.AppendLine(" - Пиши как недовольный клиент, но без оскарблений.");
        systemMessageBuilder.AppendLine();

        string randomPhrase1 = randomComplaint.Phrases[random.Next(randomComplaint.Phrases.Count)];
        string randomPhrase2 = randomComplaint.Phrases[random.Next(randomComplaint.Phrases.Count)];

        string randomTemplate = randomComplaint.Templates[random.Next(randomComplaint.Templates.Count)];
        randomTemplate = randomTemplate.Replace("{phrase1}", randomPhrase1);
        randomTemplate = randomTemplate.Replace("{phrase2}", randomPhrase2);

        systemMessageBuilder.AppendLine("Жалоба:");
        systemMessageBuilder.AppendLine(randomTemplate);

        chat.AppendSystemMessage(systemMessageBuilder.ToString());

        // Остальная часть кода остается без изменений
        chat.AppendUserInput(prompt);
        string response = await chat.GetResponseFromChatbotAsync();

        return response;
    }

}
