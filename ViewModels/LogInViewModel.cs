namespace aspnet_blog_application.ViewModels;

public class LogInViewModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string helpMessage {get; set;}
    public string myUsername {get; set;}

    public LogInViewModel(){
	helpMessage = null;
    }
}
