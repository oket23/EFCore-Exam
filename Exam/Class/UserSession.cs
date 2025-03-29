using Exam.Models;

namespace Exam.Class;

public class UserSession
{
    public UserSession()
    {
        TempUser = new MyUser();
    }
    public MyUser TempUser { get; set; } = new MyUser();
    public string UserStatus { get; set; } = "null";
    public string Password { get; set; } = "";
}
