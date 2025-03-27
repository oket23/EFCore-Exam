using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;

namespace Exam;

public class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        ConfigServies servies = new ConfigServies();
        var bot_API = servies.GetApi();

        var bot = new TelegramBotClient(bot_API,cancellationToken: cts.Token);
        var me = bot.GetMe();
        bot.OnMessage += OnMessage;
        bot.OnError += OnError;
        bot.OnUpdate += OnUpdate;

        Console.ReadKey();
        cts.Cancel();

        async Task OnUpdate(Update update)
        {
            throw new NotImplementedException();
        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Chat.Type == ChatType.Private)
            {
                switch (msg.Text)
                {
                    case "/start":
                        await bot.SendMessage(msg.Chat.Id, $"Привіт,52");
                        break;
                    case "/login":

                        break;
                    case "/register":

                        break;
                }
            }
        }
    }

   
}
