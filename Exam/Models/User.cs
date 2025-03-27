using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Exam.Models;

[Table("USERS")]
public class User
{
    public int Id { get; set; }
    [Required]
    [MaxLength(32)]
    public string Name { get; set; }
    [Required]
    [MaxLength(32)]
    public string LastName { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public DateTime BDate { get; set; }
    [Required]
    public DateTime RegDate { get; set; }
    [Required]
    [Column(TypeName = "CHAR(16)")]
    public string CartNumber { get; set; }
    [Required]
    public bool IsAdmin { get; set; }
}
