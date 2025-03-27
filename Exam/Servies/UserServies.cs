using Exam.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Exam.Servies;

public class UserServies
{
    private readonly TelegramBotClient _bot;
    private readonly ExamContext _context;
    private List<MyUser> _users;
    public UserServies(TelegramBotClient bot,ExamContext context)
    {
        _bot = bot;
        _context = context;
        _users = new List<MyUser>();
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
        if (password.Length < 8 && password.Length > 32)
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
        if(_context.Users.Any(x => x.Login.Equals(login)) || login.Length < 4 || login.Length > 32)
        {
            _bot.SendMessage(id, "Такий логін вже існує, спробуй інший!");
            return false;
        }
        return true;
    }

    public string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input.Substring(0, 1).ToUpper() + input.Substring(1).ToLower();
    }
}
