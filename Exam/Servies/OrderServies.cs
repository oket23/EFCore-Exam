using Exam.Models;
using Telegram.Bot;

namespace Exam.Servies;

public class OrderServies
{
    private readonly TelegramBotClient _bot;
    private readonly ExamContext _context;
    public OrderServies(TelegramBotClient bot, ExamContext context)
    {
        _bot = bot;
        _context = context;
    }

    public void AddOrder(Order order)
    {
        if (order != null)
        {
            _context.Add(order);
            _context.SaveChanges();
        }
    }
    public void RemoveOrder(Order order)
    {
        if (order != null)
        {
            _context.Remove(order);
            _context.SaveChanges();
        }
    }
    public void UpdateOrder(Order order)
    {
        if (order != null)
        {
            _context.Update(order);
            _context.SaveChanges();
        }
    }

}
