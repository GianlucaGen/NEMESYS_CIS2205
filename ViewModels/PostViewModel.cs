namespace aspnet_blog_application.Models;

public class PostViewModel{
    public List<PostModel> PostList {get;set;}
    public PostModel Post {get;set;}
    public IFormFile? PhotoImage {get;set;}
    public string UserID {get; set;}

    public PostViewModel(){
	UserID = null;
    }
}