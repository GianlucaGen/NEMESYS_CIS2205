using aspnet_blog_application.Models;

namespace aspnet_blog_application.ViewModels
{
    public class DetailsViewModel
    {
        public PostModel Report {get; set;}
	public UserModel Poster {get; set;}
	public UserModel CurrentUser {get; set;}
	public InvestigationModel Investigation {get; set;}
	public UserModel Investigator {get; set;}
	public int CanUpvote {get; set;}

	public DetailsViewModel(){
	    CurrentUser = new UserModel(){Id = -1, UserType = -1};
	}
    }
}
