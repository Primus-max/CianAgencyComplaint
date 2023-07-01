
using OpenAI_API;
using OpenAI_API.Chat;

public class ChatGptApi
{
    private readonly OpenAIAPI api;
    private readonly string API_KEY = "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";

    public ChatGptApi()
    {
        api = new OpenAIAPI(API_KEY);
    }

    public async Task<string> GetChatGptResponse(string prompt)
    {
        var chat = api.Chat.CreateConversation();
        chat.AppendSystemMessage(
            "- Ты женщина 25-25 лет. " +
            " - Ты пишешь жалобу на сайте по покупке, аренде жилья. " +
            " - Тебе даётся фраза, ты её дополняешь на русском языке." +
            " - Сообщение должно состоять максимум из 10-15 слов." +
            " - Ты должна убеждать в своей правде, чтобы сайт отреагировал и заблокировал объявление. " +
            " - Не ссылайся на половые неравенства. " +
            " - Меньше официального тона" +
            " - В цонце желобы можешь представиться именем, но это не обязательно" +
            " - Не в коем случае не вставляй такое - [Ваше имя]!");

        #region Модель обучения        
        // Кейс
        chat.AppendUserInput("На что жалуетесь?");
        // Овтет
        chat.AppendExampleChatbotOutput("Предложение уже неактуально или вымышленный объект");
        // Кейс
        chat.AppendUserInput("На что жалуетесь?");
        // Овтет
        chat.AppendExampleChatbotOutput("По телефону отвечают, что ничего не предлагают — указан чужой телефон");
        #endregion

        // Задайте вопрос и получите ответ
        chat.AppendUserInput(prompt);
        string response = await chat.GetResponseFromChatbotAsync();

        return response;
    }
}
