
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
        chat.AppendSystemMessage("Говори только от женского пола. Ты пишешь жалобу на сайте по покупке, аренде жилья. Тебе даётся фраза, ты её дополняешь на русском языке 10-15 слов. " +
            "Ты должен убеждать в своей правде, чтобы сайт отреагировал и заблокировал объявление. Не ссылайся на половые неравенства.");

        // Добавьте необходимые сообщения пользователя и ассистента
        chat.AppendUserInput("На что жалуетесь?");
        chat.AppendExampleChatbotOutput("Предложение уже неактуально или вымышленный объект");

        // Задайте вопрос и получите ответ
        chat.AppendUserInput(prompt);
        string response = await chat.GetResponseFromChatbotAsync();

        return response;
    }
}
