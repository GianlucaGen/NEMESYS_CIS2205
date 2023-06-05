using aspnet_blog_application.Models;
namespace aspnet_blog_application.ViewModels;

public class UserEditViewModel
{
    public UserModel ThisUser { get; set; }
    public UserModel Update { get; set; }

    public UserEditViewModel(){
	ThisUser = new UserModel(){Id = -1};
    }
}
