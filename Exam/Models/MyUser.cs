using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Exam.Models;

[Table("USERS")]
public class MyUser
{
    public int Id { get; set; }
    [Required]
    [MaxLength(32)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string LastName { get; set; }
    [Required]
    [MaxLength(32)]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    [Column(TypeName = "DateTime")]
    public DateOnly BDate { get; set; }
    [Required]
    [Column(TypeName = "DateTime")]
    public DateOnly RegDate { get; set; }
    [Required]
    [Column(TypeName = "CHAR(16)")]
    public string CartNumber { get; set; }
    [Required]
    public bool IsAdmin { get; set; }
}
