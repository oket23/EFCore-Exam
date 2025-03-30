using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Exam.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Exam.Servies;
using System.Text.RegularExpressions;
using Exam.Class;

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
                new[] {InlineKeyboardButton.WithCallbackData("Робота з товаром", "productServies")},
                new[] {InlineKeyboardButton.WithCallbackData("Переглянути усі товари", "allProduct")},
                new[] {InlineKeyboardButton.WithCallbackData("Вибрати товар по id", "productById")},
                new[] {InlineKeyboardButton.WithCallbackData("Переглянути статистику", "stats")},
                new[] {InlineKeyboardButton.WithCallbackData("Вийти з акаунта", "sign out") }
                });
        var userKeyboard = new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Переглянути усі товари", "allProduct") },
                new[] {InlineKeyboardButton.WithCallbackData("Вибрати товар по id", "productById") },
                new[] {InlineKeyboardButton.WithCallbackData("Оформити замовлення", "order")},
                new[] {InlineKeyboardButton.WithCallbackData("Вийти з акаунта", "sign out") }
                });
        var productKeyboard = new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Додати товар", "addProd") },
                new[] {InlineKeyboardButton.WithCallbackData("Оновити товар", "updateProd") },
                new[] {InlineKeyboardButton.WithCallbackData("Видалити", "deleteProd") },
                new[] {InlineKeyboardButton.WithCallbackData("Повернутися", "back") }
                });
        var productUpdateKeyboard = new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Ім'я", "newName"), InlineKeyboardButton.WithCallbackData("Опис", "newDescription") },
                new[] {InlineKeyboardButton.WithCallbackData("Ціна", "newPrice"),InlineKeyboardButton.WithCallbackData("Знижка", "newDiscount") },
                new[] {InlineKeyboardButton.WithCallbackData("Категорія", "newCategory") },
                new[] {InlineKeyboardButton.WithCallbackData("Повернутися", "newBack") }
                });

        var bot = new TelegramBotClient(bot_API,cancellationToken: cts.Token);
        var me = bot.GetMe();
        var userSessions = new Dictionary<long, UserSession>();
        var context = new ExamContext();
        var userServies = new UserServies(bot, context);
        var productServies = new ProductServies(bot, context);
        var orderServies = new OrderServies(bot, context);
        var emojiPattern = @"[\u1F600-\u1F64F\u2702\u2705\u2615\u2764\u1F4A9]+";
        var tempProduct = new Product();
        var producStatus = "null";
        var updateStatus = "null";


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
                        await bot.SendMessage(query.Message.Chat.Id, $"Привіт, виберіть дію:", replyMarkup: logRegKeyboard);
                        session.UserStatus = "null";
                        break;
                    case "yes":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Тоді проходимо реєстрацію заново!");
                        await bot.SendMessage(query.Message.Chat.Id, "Як вас звати?");
                        session.UserStatus = "name";
                        break;
                    #endregion
                    case "productServies":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Що саме ви хочете зробити з товаром?", replyMarkup:productKeyboard);
                        break;
                    case "allProduct":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await productServies.ShowAllProduct(query.Message.Chat.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Виберіть дію:", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);
                        break;
                    case "productById":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Напишіть ID продукта для пошуку:");
                        producStatus = "wait id";                       
                        break;
                    case "stats":
                        await bot.SendMessage(query.Message.Chat.Id, "Статистик по замовленням:");
                        var orderStats = context.Orders
                            .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Price, x.Product.DiscountPercentage })
                            .Select(x => new
                            {
                                ProductName = x.Key.Name,
                                OrderCount = x.Count(),
                                TotalPrice = x.Count() * x.Key.Price * (1 - (x.Key.DiscountPercentage ?? 0) / 100m)
                            })
                            .ToList();
                        string result = string.Join("\n", orderStats.Select(item => $"Товар: {item.ProductName}, Кількість замовлень: {item.OrderCount}, Сумарна ціна: {item.TotalPrice:F3}"));
                        await bot.SendMessage(query.Message.Chat.Id, result);
                        await bot.SendMessage(query.Message.Chat.Id, "Виберіть дію:", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);
                        
                        break;
                    case "order":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        productServies.ShowAllProduct(query.Message.Chat.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Напишіть ID продукту покупки:");
                        session.UserStatus = "order";
                        break;
                    case "sign out":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Привіт, обирай дію:", replyMarkup: logRegKeyboard);
                        session.UserStatus = "null";
                        session.TempUser = new MyUser();
                        tempProduct = new Product();
                        
                        break;
                    #region product
                    case "addProd":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть назву продукту:");
                        producStatus = "productName";
                        break;
                    case "updateProd":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть Id продукту для оновлення:");
                        producStatus = "update";
                        break;
                    case "deleteProd":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть Id продукту для видалення:");
                        producStatus = "delete";
                        break;
                    case "back":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Виберіть дію:", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);
                        break;
                    case "newName":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть нове ім'я:");
                        updateStatus = "newName";
                        break;
                    case "newDescription":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть новий опис:");
                        updateStatus = "newDescription";
                        break;
                    case "newPrice":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть нову ціну:");
                        updateStatus = "newPrice";
                        break;
                    case "newCategory":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть нову категорію:");
                        updateStatus = "newCategory";
                        break;
                    case "newDiscount":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, $"Введіть нову знижку:");
                        updateStatus = "newDiscount";
                        break;
                    case "newBack":
                        await bot.DeleteMessage(query.Message.Chat.Id, query.Message.Id);
                        await bot.SendMessage(query.Message.Chat.Id, "Що саме ви хочете зробити з товаром?", replyMarkup: productKeyboard);
                        updateStatus = "null";
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
                    tempProduct = new Product();
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
                            session.UserStatus = "verify";
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
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Як вас коректно звати?");
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
                            await bot.SendMessage(msg.Chat.Id, "Придумайте свій унікальний логін:");
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть ваше коректне прізвище:");
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
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Придумайте коректний унікальний логін:");
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
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Придумайте коректний пароль і відправте мені!");
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
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Залишилося тільки написати номер вашої картки:");
                        }
                        break;
                    #endregion
                    case "order":
                        if(int.TryParse(msg.Text.Trim(), out int id))
                        {
                            var productToOrder = context.Products.Find(id);
                            if (productToOrder == null)
                            {
                                await bot.SendMessage(msg.Chat.Id, "Продукта з таким ID не знайдено!");
                                await bot.SendMessage(msg.Chat.Id, "Введіть коректний ID продукта для купівлі:");
                                break;
                            }
                            else
                            {
                                var order = new Order()
                                {
                                    UserId = session.TempUser.Id,
                                    ProductId = id,
                                    OrderDate = DateTime.Now,
                                    Product = productToOrder,
                                    User = session.TempUser
                                };
                                orderServies.AddOrder(order);
                                session.UserStatus = "null";
                                await bot.SendMessage(msg.Chat.Id, $"Ви оформили:");
                                await productServies.ShowProductById(msg.Chat.Id, id);
                                await bot.SendMessage(msg.Chat.Id, "Замовлення успішно оформлено!", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);                              
                            }
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть коректний ID продукта для купівлі:");
                        }                                                                 
                        break;
                }
                switch (producStatus)
                {
                    case "wait id":
                        var id = int.Parse(msg.Text.Trim());
                        await productServies.ShowProductById(msg.Chat.Id, id);
                        await bot.SendMessage(msg.Chat.Id, $"Виберіть дію:", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);
                        producStatus = "null";
                        break;
                    case "productName":
                        tempProduct = new Product();
                        var name = msg.Text.Trim();

                        if(productServies.IsValidName(msg.Chat.Id, name))
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть опис для продукта:");
                            producStatus = "description";
                            tempProduct.Name = name;
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, $"Введіть назву продукту:");
                        }
                        break;
                    case "description":
                        var description = msg.Text.Trim();
                        if(productServies.IsValidDescription(msg.Chat.Id, description))
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть ціну продукта:");
                            producStatus = "price";
                            tempProduct.Description = description;
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть опис для продукта:");
                        }
                        break;
                    case "price":
                        if (decimal.TryParse(msg.Text.Trim(), out decimal price))
                        {
                            if(productServies.IsValidPrice(msg.Chat.Id, price))
                            {
                                await bot.SendMessage(msg.Chat.Id, "Введіть знижку (якщо немає напишіть 0):");
                                producStatus = "discount";
                                tempProduct.Price = price;
                            }
                            else
                            {
                                await bot.SendMessage(msg.Chat.Id, "Введіть коректну ціну продукта (наприклад, 100,50):");
                                break;
                            }
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть коректну ціну продукта (наприклад, 100,50):");
                        }
                        break;
                    case "discount":
                        if (msg.Text.Trim() == "0")
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть категорію продукта:");
                            producStatus = "category";
                        }
                        else
                        {
                            if(int.TryParse(msg.Text.Trim(),out int discount))
                            {
                               if(!productServies.IsValidDiscount(msg.Chat.Id, discount))
                               {
                                    tempProduct.DiscountPercentage = discount;
                                    await bot.SendMessage(msg.Chat.Id, "Введіть категорію продукта:");
                                    producStatus = "category";                    
                               }
                               else
                               {
                                   await bot.SendMessage(msg.Chat.Id, "Введіть знижку(якщо немає напишіть 0):");
                                   break;
                               }
                            }
                            else
                            {
                                await bot.SendMessage(msg.Chat.Id, "Введіть знижку(якщо немає напишіть 0):");
                            }  
                        }
                        break;
                    case "category":
                        var category = msg.Text.Trim();
                        if(productServies.IsValidCategory(msg.Chat.Id, category))
                        {
                            tempProduct.Category = category;
                            productServies.AddProduct(tempProduct);
                            await bot.SendMessage(msg.Chat.Id, "Продукт успішно додано!", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);
                            producStatus = "null";
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть категорію продукта:");
                        }
                        break;
                    case "delete":
                        if(int.TryParse(msg.Text.Trim(),out int productId))
                        {
                            var product = context.Products.Find(productId);
                            if(product == null)
                            {
                                await bot.SendMessage(msg.Chat.Id, "Продукт з таким ID не знайдено!");
                                await bot.SendMessage(msg.Chat.Id, "Введіть коректне ID продукта для видалення:");
                            }
                            else
                            {
                                productServies.DeleteProduct(product);
                                await bot.SendMessage(msg.Chat.Id, "Продукт успішно видалено!", replyMarkup: (session.TempUser.IsAdmin) ? adminKeyboard : userKeyboard);
                            }  
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть коректне ID продукта для видалення:");
                        }
                        
                        break;
                    case "update":
                        if (int.TryParse(msg.Text.Trim(), out int prodId))
                        {
                            tempProduct = context.Products.Find(prodId);
                            if(tempProduct != null)
                            {
                                producStatus = "null";
                                await bot.SendMessage(msg.Chat.Id, "Виберіть що ви хочете оновити:",replyMarkup:productUpdateKeyboard);
                            }
                            else
                            {
                                await bot.SendMessage(msg.Chat.Id, "Продукт з таким ID не знайдено!");
                                await bot.SendMessage(msg.Chat.Id, "Введіть коректне ID продукта для оновлення:");
                            }
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть коректне ID продукта для оновлення:");
                        }
                        break;
                }
                switch (updateStatus)
                {
                    case "newName":
                        var name = msg.Text.Trim();
                        if (productServies.IsValidName(msg.Chat.Id, name))
                        {
                            tempProduct.Name = name;
                            productServies.UpdateProduct(tempProduct);
                            updateStatus = "null";
                            await bot.SendMessage(msg.Chat.Id, "Зміну успішно виконано!", replyMarkup: productUpdateKeyboard);
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, $"Введіть нове коректне ім'я:");
                        }
                        break;
                    case "newDescription":
                        var description = msg.Text.Trim();
                        if (productServies.IsValidDescription(msg.Chat.Id, description))
                        {
                            tempProduct.Description = description;
                            productServies.UpdateProduct(tempProduct);
                            updateStatus = "null";
                            await bot.SendMessage(msg.Chat.Id, "Зміну успішно виконано!", replyMarkup: productUpdateKeyboard);
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, $"Введіть новий коректний опис:");
                        }
                        break;
                    case "newPrice":
                        if (decimal.TryParse(msg.Text.Trim(), out decimal price))
                        {
                            if (productServies.IsValidPrice(msg.Chat.Id, price))
                            {
                                tempProduct.Price = price;
                                productServies.UpdateProduct(tempProduct);
                                updateStatus = "null";
                                await bot.SendMessage(msg.Chat.Id, "Зміну успішно виконано!", replyMarkup: productUpdateKeyboard);
                            }
                            else
                            {
                                await bot.SendMessage(msg.Chat.Id, "Введіть коректну нову ціну продукта (наприклад, 100,50):");
                                break;
                            }
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть коректну ціну продукта (наприклад, 100,50):");
                        }
                        break;
                    case "newCategory":
                        var category = msg.Text.Trim();
                        if (productServies.IsValidCategory(msg.Chat.Id, category))
                        {
                            tempProduct.Category = category;
                            productServies.UpdateProduct(tempProduct);
                            updateStatus = "null";
                            await bot.SendMessage(msg.Chat.Id, "Зміну успішно виконано!", replyMarkup: productUpdateKeyboard);
                        }
                        else
                        {
                            await bot.SendMessage(msg.Chat.Id, "Введіть коректну нову категорію продукта:");
                        }
                        break;
                    case "newDiscount":
                        if (msg.Text.Trim() == "0")
                        {
                            tempProduct.DiscountPercentage = null;
                            productServies.UpdateProduct(tempProduct);
                            updateStatus = "null";
                            await bot.SendMessage(msg.Chat.Id, "Зміну успішно виконано!", replyMarkup: productUpdateKeyboard);
                        }
                        else
                        {
                            if (int.TryParse(msg.Text.Trim(), out int discount))
                            {
                                if (productServies.IsValidDiscount(msg.Chat.Id, discount))
                                {
                                    tempProduct.DiscountPercentage = discount;
                                    productServies.UpdateProduct(tempProduct);
                                    updateStatus = "null";
                                    await bot.SendMessage(msg.Chat.Id, "Зміну успішно виконано!", replyMarkup: productUpdateKeyboard);
                                }
                                else
                                {
                                    await bot.SendMessage(msg.Chat.Id, "Введіть коректну нову знижку(якщо немає напишіть 0):");
                                    break;
                                }
                            }
                            else
                            {
                                await bot.SendMessage(msg.Chat.Id, "Введіть коректну нову знижку(якщо немає напишіть 0):");
                            }
                        }
                        break;
                }
            }
            else if (msg.Animation != null)
            {
                string animation = msg.Animation.FileId;
                await bot.SendAnimation(msg.Chat.Id, animation);
            }
            else if (msg.Sticker != null)
            {
                string sticker = msg.Sticker.FileId;
                await bot.SendStickerAsync(msg.Chat.Id, sticker);
            }
            else
            {
                await bot.SendMessage(msg.Chat.Id, "Я приймаю тільки текст!");
            }
            
            
        }
      
    }

}
