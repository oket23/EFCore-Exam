using Exam.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Exam.Servies;

public class ProductServies
{
    private readonly TelegramBotClient _bot;
    private readonly ExamContext _context;

    public ProductServies(TelegramBotClient bot, ExamContext context)
    {
        _bot = bot;
        _context = context;
    }

    public void AddProduct(Product product)
    {
        if (product != null)
        {
            _context.Add(product);
            _context.SaveChanges();
        }
    }
    public void UpdateProduct(Product product)
    {
        if (product != null)
        {
            _context.Update(product);
            _context.SaveChanges();
        }
    }
    public void DeleteProduct(Product product)
    {
        if(product != null)
        {
            _context.Remove(product);
            _context.SaveChanges();
        }
    }

    public async Task ShowAllProduct(ChatId id)
    {
        var allProduct = _context.Products
            .OrderBy(x => x.Name)
            .Select(x => $"{x.Id} {x.Name} {x.Description} {x.Price}$ {(x.DiscountPercentage==null?"0": x.DiscountPercentage)}% {x.Category}")
            .ToList();

        if (allProduct.Any())
        {
            string text = string.Join("\n", allProduct);
            await _bot.SendMessage(id, $"Всі продукти:\n{text}");
        }
        else
        {
            await _bot.SendMessage(id, "Продуктів не знайдено!");
        }
    }

    public async Task ShowProductById(ChatId id, int productId)
    {
        var productById = _context.Products.Find(productId);
        if(productById != null)
        {
            await _bot.SendMessage(id, $"Назва: {productById.Name}\nОпис: {productById.Description}\nЦіна: {productById.Price}$\nЗнижка: {((productById.DiscountPercentage == null)?"0":productById.DiscountPercentage)}%\nКатегорія: {productById.Category}");
        }
        else
        {
            await _bot.SendMessage(id, "Продукт з таким ID не знайдено.");
        }
    }

    public bool IsValidName(ChatId id, string productName)
    {
        if(productName.Length < 2 || productName.Length > 64)
        {
            _bot.SendMessage(id, "Назва повинна бути більшою за 2 і меншою за 64 символи!");
            return false;
        }
        return true;
    }

    public bool IsValidDescription(ChatId id, string text)
    {
        if (text.Length < 2 || text.Length > 64)
        {
            _bot.SendMessage(id, "Опис повинен бути більшим за 2 і меншим за 64 символи!");
            return false;
        }
        return true;
    }

    public bool IsValidPrice(ChatId id, decimal price)
    {
        if (price <= 0)
        {
            _bot.SendMessage(id, "Ціна повинна бути більшою за 0");
            return false;
        }
        return true;
    }

    public bool IsValidDiscount(ChatId id, int discount)
    {
        if(discount > 100 || discount <= 0)
        {
            _bot.SendMessage(id, "Знижка повинна бути більшою за 0% і меншою за 100%");
            return false;
        }
        return true;
    }

    public bool IsValidCategory(ChatId id, string category)
    {
        if (category.Length <= 2 || category.Length > 64)
        {
            _bot.SendMessage(id, "Назва категорії повинна бути більшою за 2 і меншою за 64 символи!");
            return false;
        }
        return true;
    }
    
    //ваш код
}
