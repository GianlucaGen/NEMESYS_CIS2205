namespace aspnet_blog_application.Models;

public class PostModel
{
    public int Id {get; set;}
    public string  Title {get; set;}
    public string  Description {get; set;}
    public string  CreatedAt {get; set;}
    public string  UpdatedAt {get; set;}
    public string  Location {get; set;}
    public string  SpotTime {get; set;}
    public string  HazardType {get; set;}
    public string  UserID {get; set;}
    public string  Photo {get; set;}
    public int Upvotes {get; set;}
    public string  Status {get; set;}
}
