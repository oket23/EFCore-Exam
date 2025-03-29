using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Exam.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Exam.Servies;
using System.Text.RegularExpressions;
using BCrypt.Net;
using Exam.Class;
using Microsoft.AspNetCore.Http;

namespace Exam;

public class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var configServies = new ConfigServies();
        var bot_API = configServies.GetApi();
        var logRegKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Увійти", "log"), InlineKeyboardButton.WithCallbackData("Зареєструватися", "reg") } });
        var logYesOrNoKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("Так", "yes"), InlineKeyboardButton.WithCallbackData("Ні", "no") } });         
        var adminKeyboard = new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Робота з товаром", "admin1")},
                new[]{ InlineKeyboardButton.WithCallbackData("Переглянути усі товари", "admin2")},
                new[] {InlineKeyboardButton.WithCallbackData("Вибрати товар по id", "admin3")},
                new[] {InlineKeyboardButton.WithCallbackData("Переглянути статистику", "admin4")},
                });
        var userKeyboard = new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Переглянути усі товари", "user1") },
                new[] {InlineKeyboardButton.WithCallbackData("Вибрати товар по id", "user2") },
                new[] {InlineKeyboardButton.WithCallbackData("Оформити замовлення", "user3")}
                });

        var bot = new TelegramBotClient(bot_API,cancellationToken: cts.Token);
        var me = bot.GetMe();
        var userSessions = new Dictionary<long, UserSession>();
        var context = new ExamContext();
        var userServies = new UserServies(bot, context);
        var emojiPattern = @"[\u1F600-\u1F64F\u2702\u2705\u2615\u2764\u1F4A9]+";


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
                if (!userSessions.ContainsKey(query.Message.Chat.Id))
                {
                    userSessions[query.Message.Chat.Id] = new UserSession();
                }

                var session = userSessions[query.Message.Chat.Id];

                await bot.AnswerCallbackQuery(query.Id);
                switch (query.Data)
                {
                    #region log reg
                    case "log":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Напишіть ваш логін:");
                        session.UserStatus = "log in";
                        break;
                    case "reg":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Давайте познайомимося!");
                        await bot.SendMessage(query.Message.Chat.Id, "Як вас звати?");
                        session.UserStatus = "name";
                        break;
                    #endregion
                    #region yes no
                    case "no":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Ви успішно зареєструвалися!");
                        context.Add(session.TempUser);
                        context.SaveChanges();
                        await bot.SendMessage(query.Message.Chat.Id, $"Привіт, обирай дію:", replyMarkup: logRegKeyboard);
                        session.UserStatus = "null";
                        break;
                    case "yes":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Тоді проходимо реєстрацію заново!");
                        await bot.SendMessage(query.Message.Chat.Id, "Як вас звати?");
                        session.UserStatus = "name";
                        break;
                    #endregion
                    #region admin
                    case "admin1":

                        break;


                    #endregion
                }
            }
        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message msg, UpdateType type)
        {
            Console.WriteLine($"{msg.From.Username}: {msg.Text}");

            if (msg.Chat.Type == ChatType.Group || msg.Chat.Type == ChatType.Supergroup)
            {
                return;
            }

            if (!userSessions.ContainsKey(msg.Chat.Id))
            {
                userSessions[msg.Chat.Id] = new UserSession();
            }

            var session = userSessions[msg.Chat.Id];

            if (msg.Chat.Type == ChatType.Private && msg.Text != null && Regex.IsMatch(msg.Text, emojiPattern))
            {
               
                if (msg.Text == "/start")
                {
                    await bot.SendMessage(msg.Chat.Id, $"Привіт, обирай дію:", replyMarkup: logRegKeyboard);
                    session.UserStatus = "null";
                    session.TempUser = new MyUser();
                }

                switch (session.UserStatus)
                {
                    #region log
                    case "log in":
                        var _login = msg.Text.Trim();
                        await bot.SendMessage(msg.Chat.Id, "Введіть пароль:");
                        session.TempUser.Login = _login;
                        session.UserStatus = "logPassword";
                        break;
                    case "logPassword":
                        var login_ = session.TempUser.Login;
                        var _password = msg.Text.Trim();
                        if (userServies.IsValidLoginAndPassword(session.TempUser.Login,_password, msg.Chat.Id))
                        {
                            session.TempUser = context.Users.FirstOrDefault(x => x.Login == login_);
                            await bot.SendMessage(msg.Chat.Id, "Ви успішно увійшли!");
                            await bot.SendMessage(msg.Chat.Id, "Виберіть, що будемо робити:", replyMarkup:(session.TempUser.IsAdmin)?adminKeyboard:userKeyboard);
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Давайте ще разок!\nНапишіть ваш логін:");
                            session.UserStatus = "log in";
                        }
                        break;
                    #endregion
                    #region reg
                    case "name":
                        var name = msg.Text.Trim();
                        name = userServies.CapitalizeFirstLetter(name);
                        if (!string.IsNullOrEmpty(name))
                        {
                            session.TempUser.Name = name;
                            session.UserStatus = "lastname";
                            await bot.SendMessage(msg.Chat.Id, "Імя успішно додано!");
                            await bot.SendMessage(msg.Chat.Id, "Введіть ваше прізвище:");
                        }
                        break;
                    case "lastname":
                        string lastName = msg.Text.Trim();
                        lastName = userServies.CapitalizeFirstLetter(lastName);
                        if (!string.IsNullOrEmpty(lastName))
                        {
                            session.UserStatus = "login";
                            session.TempUser.LastName = lastName;
                            await bot.SendMessage(msg.Chat.Id, "Прізвище успішно додано!");
                            await bot.SendMessage(msg.Chat.Id, "Придумайте свій унікальний логін!");
                        }
                        break;
                    case "login":
                        string login = msg.Text.Trim();
                        if (userServies.IsValidLogin(login, msg.Chat.Id))
                        {
                            session.TempUser.IsAdmin = userServies.IsAdmin(login);
                            session.UserStatus = "password";
                            session.TempUser.Login = login;
                            await bot.SendMessage(msg.Chat.Id, "Логін успішно додано!");
                            await bot.SendMessage(msg.Chat.Id, "Придумайте пароль і відправте мені!");
                        }
                        break;
                    case "password":
                        session.Password = msg.Text.Trim();
                        if (userServies.IsValidPassword(session.Password, msg.Chat.Id))
                        {
                            await bot.SendMessage(msg.Chat.Id, "Пароль успішно доданий!");
                            await bot.SendMessage(msg.Chat.Id, "Тепер напишіть вашу дату народження у форматі yyyy-mm-dd:");
                            session.UserStatus = "bdate";
                            var passwordHash = BCrypt.Net.BCrypt.HashPassword(session.Password);
                            session.TempUser.Password = passwordHash;
                        }
                        break;
                    case "bdate":
                        DateOnly bdate;
                        if (DateOnly.TryParse(msg.Text.Trim(), out bdate))
                        {
                            if (userServies.IsValidBDate(bdate, msg.Chat.Id))
                            {
                                await bot.SendMessage(msg.Chat.Id, "Дата народження успішно додана!");
                                await bot.SendMessage(msg.Chat.Id, "Залишилося тільки написати номер вашої картки:");
                                session.UserStatus = "card";
                                session.TempUser.BDate = bdate;
                            }
                        }
                        else
                        {
                            await bot.SendMessage(msg.From.Id, "Будь ласка, введіть дійсну дату!");
                            await bot.SendMessage(msg.Chat.Id, "Тепер напишіть вашу дату народження у форматі yyyy-mm-dd:");
                        }
                        break;
                    case "card":
                        string cardNumber = msg.Text.Trim();
                        if (userServies.isValidCardNumber(cardNumber, msg.Chat.Id))
                        {
                            await bot.SendMessage(msg.From.Id, "Картку успішно додано!");

                            session.TempUser.CartNumber = cardNumber;
                            session.TempUser.RegDate = DateOnly.FromDateTime(DateTime.Now);

                            await bot.SendMessage(msg.Chat.Id, "Реєстрацію успішно завершено!");
                            
                            await bot.SendMessage(msg.Chat.Id, $"Ось як виглядають ваші дані:\nІм'я: {session.TempUser.Name}\nПрізвище: {session.TempUser.LastName}\nЛогін: {session.TempUser.Login}\nПароль: {session.Password}\nДата народження: {session.TempUser.BDate}\nНомер карти: {session.TempUser.CartNumber}\nДата реєстрації: {session.TempUser.RegDate}");
                            await bot.SendMessage(msg.Chat.Id, "Бажаєте щось змінити?", replyMarkup: logYesOrNoKeyboard);
                            session.UserStatus = "null";
                        }
                        break;
                        #endregion
                }
            }
            else
            {
                await bot.SendMessage(msg.Chat.Id, "Я приймаю тільки текст!");
            }
        }
    }

}
