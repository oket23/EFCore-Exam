using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Exam.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Exam.Servies;

namespace Exam;

public class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        ConfigServies configServies = new ConfigServies();
        var bot_API = configServies.GetApi();
        InlineKeyboardMarkup firstKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Увійти", "log"), InlineKeyboardButton.WithCallbackData("Зареєструватися", "reg") } });
        InlineKeyboardMarkup secondKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Так", "yes"), InlineKeyboardButton.WithCallbackData("Ні", "no") } });     

        var bot = new TelegramBotClient(bot_API,cancellationToken: cts.Token);
        var me = bot.GetMe();
        MyUser tempUser = new MyUser();
        string userStatus = "";
        var context = new ExamContext();
        var userServies = new UserServies(bot, context);

        bot.OnMessage += OnMessage;
        bot.OnError += OnError;
        bot.OnUpdate += OnUpdate;

        Console.WriteLine("Я живий!");
        Console.ReadKey();
        Console.WriteLine("Всім удачі, всім пака!");


        cts.Cancel();

        async Task OnUpdate(Update update)
        {
            if (update is { CallbackQuery: { } query })
            {
                await bot.AnswerCallbackQuery(query.Id);
                switch (query.Data)
                {
                    case "log":
                        await bot.DeleteMessage(query.Message.Chat, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat, "Ви обрали увійти!");
                        Console.WriteLine(query.From.Username);
                        Console.WriteLine(query.From.FirstName);
                        break;
                    case "reg":
                        await bot.DeleteMessage(query.Message.Chat, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat, "Давайте познайомимося!");
                        await bot.SendMessage(query.Message.Chat, "Як вас звати?");
                        userStatus = "name";
                        break;
                    case "no":
                        await bot.DeleteMessage(query.Message.Chat, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat, "Ви успішно зареєструвалися!");
                        context.Add(tempUser);
                        context.SaveChanges();
                        break;
                    case "yes":
                        await bot.DeleteMessage(query.Message.Chat, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat, "Тоді проходимо реєстрацію заново!");
                        await bot.SendMessage(query.Message.Chat, "Як вас звати?");
                        userStatus = "name";
                        break;
                }
            }
                

        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message msg, UpdateType type)
        {
            
            if (msg.Chat.Type == ChatType.Private)
            { 
                if(msg.Text == "/start")
                {
                    await bot.SendMessage(msg.Chat.Id, $"Привіт, обирай дію:", replyMarkup: firstKeyboard);
                }

                switch (userStatus)
                {
                    case "name":
                        var name = msg.Text.Trim();
                        name = userServies.CapitalizeFirstLetter(name);
                        if (!string.IsNullOrEmpty(name))
                        {
                            tempUser.Name = name;
                            tempUser.IsAdmin = userServies.IsAdmin(msg.From.Username.Trim());
                            userStatus = "lastname";
                            await bot.SendMessage(msg.From.Id, "Імя успішно додано!");
                            await bot.SendMessage(msg.Chat.Id, "Введіть ваше прізвище:");
                        }
                        break;
                    case "lastname":
                        string lastName = msg.Text.Trim();
                        lastName = userServies.CapitalizeFirstLetter(lastName);
                        if (!string.IsNullOrEmpty(lastName))
                        {
                            userStatus = "login";
                            tempUser.LastName = lastName;
                            await bot.SendMessage(msg.From.Id, "Прізвище успішно додано!");
                            await bot.SendMessage(msg.Chat.Id, "Придумайте свій унікальний логін!");
                        }
                        break;
                    case "login":
                        string login = msg.Text.Trim();
                        if (userServies.IsValidLogin(login, msg.Chat.Id))
                        {
                            userStatus = "password";
                            tempUser.Login = login;
                            await bot.SendMessage(msg.From.Id, "Логін успішно додано!");
                            await bot.SendMessage(msg.Chat.Id, "Придумайте пароль і відправте мені!");
                        }
                        break;
                    case "password":
                        var password = msg.Text.Trim();
                        if (userServies.IsValidPassword(password, msg.Chat.Id))
                        {
                            await bot.SendMessage(msg.From.Id, "Пароль успішно доданий!");
                            await bot.SendMessage(msg.From.Id, "Тепер напишіть вашу дату народження у форматі yyyy-mm-dd:");
                            userStatus = "bdate";
                            tempUser.Password = password;
                        }
                        break;
                    case "bdate":
                        var bdate = DateOnly.Parse(msg.Text.Trim());
                        if (userServies.IsValidBDate(bdate, msg.Chat.Id))
                        {
                            await bot.SendMessage(msg.From.Id, "Дата народження успішно додана!");
                            await bot.SendMessage(msg.From.Id, "Залишилося тільки написати номер вашої картки:");
                            userStatus = "card";
                            tempUser.BDate = bdate;
                        }
                        break;
                    case "card":
                        string cardNumber = msg.Text.Trim();
                        if (userServies.isValidCardNumber(cardNumber, msg.Chat.Id))
                        {
                            await bot.SendMessage(msg.From.Id, "Картку успішно додано!");

                            tempUser.CartNumber = cardNumber;
                            tempUser.RegDate = DateOnly.FromDateTime(DateTime.Now);

                            await bot.SendMessage(msg.Chat.Id, "Реєстрацію успішно завершено!");
                            
                            await bot.SendMessage(msg.From.Id, $"Ось як виглядають ваші дані:\nІм'я: {tempUser.Name}\nПрізвище: {tempUser.LastName}\nЛогін: {tempUser.Login}\nПароль: {tempUser.Password}\nДата народження: {tempUser.BDate}\nНомер карти: {tempUser.CartNumber}\nДата реєстрації: {tempUser.RegDate}");
                            await bot.SendMessage(msg.From.Id, "Бажаєте щось змінити?", replyMarkup: secondKeyboard);
                            userStatus = "null";
                        }
                        break;
                    
                        
                }
            }
        }
    }

}
