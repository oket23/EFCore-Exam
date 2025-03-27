using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Exam.Models;

[Table("PRODUCTS")]
public class Product
{
    public int Id { get; set; }
    [Required]
    [MaxLength(64)]
    public string Name { get; set; }
    [Required]
    [MaxLength(64)]
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    public int? DiscountPercentage { get; set; }
    [Required]
    [MaxLength(64)]
    public string Category { get; set; }

}
