
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
        chat.AppendSystemMessage("Ты пишешь жалобу на сайте по покупке, аренде жилья. Тебе даётся фраза, ты её дополняешь на русском языке 10-15 слов. " +
            "Ты должен убеждать в своей правде, чтобы сайт отреагировал и заблокировал объявление");

        // Добавьте необходимые сообщения пользователя и ассистента
        chat.AppendUserInput("На что жалуетесь?");
        chat.AppendExampleChatbotOutput("Предложение уже неактуально или вымышленный объект");

        // Задайте вопрос и получите ответ
        chat.AppendUserInput(prompt);
        string response = await chat.GetResponseFromChatbotAsync();

        // Выводим все сообщения из диалога
        foreach (ChatMessage msg in chat.Messages)
        {
            Console.WriteLine($"{msg.Role}: {msg.Content}");
        }

        return response;
    }
}
