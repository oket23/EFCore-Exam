using Exam.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Exam.Servies;

public class UserServies
{
    private readonly TelegramBotClient _bot;
    private readonly ExamContext _context;
    public UserServies(TelegramBotClient bot,ExamContext context)
    {
        _bot = bot;
        _context = context;
    }
    public bool IsAdmin(string name)
    {
        if (name != null && name == "oket13" || name == "ДОДАТИ USERNAME!!")
        {
            return true;
        }
        return false;
    }
    public bool IsValidPassword(string password,ChatId id)
    {
        if (password.Length < 8 || password.Length > 32)
        {
            _bot.SendMessage(id, "Пароль має бути довшим за 8 символів.");
            return false;
        }
        return true;
    }
    public bool IsValidBDate(DateOnly bDate, ChatId id)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        int age = today.Year - bDate.Year;

        if (age < 3)
        {
            _bot.SendMessage(id, "Щоб зареєструватися, вам має бути не менше 3 років.");
            return false;
        }
        if (age > 100)
        {
            _bot.SendMessage(id, "Введіть дійсну дату!");
            return false;
        }
        return true;
    }
    public bool isValidCardNumber(string number,ChatId id)
    {
        if(number.Length != 16)
        {
            _bot.SendMessage(id, "Номер карти обовязково має мати 16 цифр!");
            return false;
        }
        if (!number.All(char.IsDigit))
        {
            _bot.SendMessage(id, "Номер карти може містити тільки цифри!");
            return false;
        }
        return true;
    }

    public bool IsValidLogin(string login, long id)
    {
        if(_context.Users.Any(x => x.Login.Equals(login)) )
        {
            _bot.SendMessage(id, "Такий логін вже існує, спробуй інший!");
            return false;
        }
        if(login.Length < 4 || login.Length > 32)
        {
            _bot.SendMessage(id, "Логін повинен бути довшим за 4 символи та коротшим за 32!");
            return false;
        }
        return true;
    }

    public string CapitalizeFirstLetter(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
    }

    public bool IsValidLoginAndPassword(string login, string password, ChatId id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Login == login);
        if (user == null)
        {
            _bot.SendMessage(id, "Такого логіна не існує!");
            return false;
        }
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            _bot.SendMessage(id, "Невірний пароль!");
            return false;
        }

        return true;
    }
}
