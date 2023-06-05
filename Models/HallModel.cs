using Microsoft.AspNetCore.Identity;

namespace aspnet_blog_application.Models;

public class HallModel : IdentityUser
{
    public int UserId {get; set;}
    public string Name {get; set;}
    public int NumReports {get; set;}
}
