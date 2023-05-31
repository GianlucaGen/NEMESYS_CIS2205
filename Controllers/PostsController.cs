using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnet_blog_application.Models;
using Microsoft.Data.Sqlite;
using aspnet_blog_application.ViewModels;

namespace aspnet_blog_application.Controllers;

public class PostsController : Controller
{
    private readonly ILogger<PostsController> _logger;

    private readonly IConfiguration _configuration;

    public PostsController(ILogger<PostsController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        var returnModel = new PostListViewModel(){Reports = GetAllPosts()};
        return View(returnModel);
    }

    public IActionResult Details(int id)
    {
        var returnModel = new DetailsViewModel(){Report = GetPostByID(id)[0]};
        return View(returnModel);
    }

     public IActionResult _InsertForm()
    {
        return View();
    }

    internal List<PostModel> GetAllPosts(){
        List<PostModel> postList = new();


        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM post";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
                            postList.Add(
                                new PostModel{
                                    Id=reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Body = reader.GetString(2),
                                    CreatedAt = reader.GetString(3),
                                    UpdatedAt = reader.GetString(4),

                                }
                            );
                            }
                    } else {
                        return postList;
                    }
                }
            }
        }
            return postList;
    }

    internal int GetPostCount(){
	var counter = 0;
        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM post";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
                            counter = counter+1;
                            }
                    } else {
                        return counter;
                    }
                }
            }
        }
            return counter;
    }

    internal int GetFreeID(){
	var returnFlag = 1;
	var counter = 0;
	do{
	returnFlag = 1;
        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM post";
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows){
                        while(reader.Read()){
                            if(reader.GetInt32(0) == counter){
				returnFlag = 0;
			    }
                        }
                    }
                }
            }
        }
	counter = counter+1;
	}while(returnFlag==0);
            return counter-1;
    }

    public ActionResult Insert(PostModel post){
	post.Id = GetFreeID();
        post.CreatedAt = (DateTime.Now).ToString();
        post.UpdatedAt =  (DateTime.Now).ToString();

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO post(id, title, body, createdat,updatedat) VALUES('{post.Id}', '{post.Title}', '{post.Body}', '{post.CreatedAt}', '{post.UpdatedAt}')";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return RedirectToAction("Index", "Posts");
    }


    public ActionResult Delete(int id){
	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM post WHERE id = '{id}'";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
	}
        return RedirectToAction("Index", "Posts");
    }

    public List<PostModel> GetPostByID(int id){
        List<PostModel> postList = new();


        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM post WHERE id = '{id}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
                            postList.Add(
                                new PostModel{
                                    Id=reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Body = reader.GetString(2),
                                    CreatedAt = reader.GetString(3),
                                    UpdatedAt = reader.GetString(4),

                                }
                            );
                            }
                    } else {
                        return postList;
                    }
                }
            }
        }
            return postList;
	}

}

    



