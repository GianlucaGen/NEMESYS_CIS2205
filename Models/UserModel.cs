using Microsoft.AspNetCore.Identity;

namespace aspnet_blog_application.Models;

public class UserModel : IdentityUser
{
    public int  Id {get; set;}
    public string  Username {get; set;}
    public string  Password {get; set;}
    public string  Email {get; set;}
    public string  PhoneNum {get; set;}
    public int UserType {get; set;}
}
