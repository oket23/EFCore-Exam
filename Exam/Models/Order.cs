using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Exam.Models;

[Table("ORDERS")]
public class Order
{
    public int Id { get; set; }
    public User User { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public Product Product { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }
    [Required]
    public DateTime OrderDate { get; set; }
}
