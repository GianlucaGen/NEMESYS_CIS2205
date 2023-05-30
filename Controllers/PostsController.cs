using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnet_blog_application.Models;
using Microsoft.Data.Sqlite;
using aspnet_blog_application.Models.ViewModels;

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
        var PostListViewModel = GetAllPosts();
        return View(PostListViewModel);
    }

     public IActionResult NewPost()
    {
        return View();
    }

    internal PostViewModel GetAllPosts(){
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
                                    CreatedAt = reader.GetDateTime(3),
                                    UpdatedAt = reader.GetDateTime(4),

                                }
                            );
                            }
                    } else {
                        return new PostViewModel { PostList = postList};
                    }
                }
            }
        }
            return new PostViewModel{PostList = postList};
    }

    public ActionResult Insert(PostModel post){
        post.CreatedAt = DateTime.Now;
        post.UpdatedAt =  DateTime.Now;

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO post(title, body, createdat,updateat) VALUES('{post.Title}', '{post.Body}', '{post.CreatedAt}', '{post.UpdatedAt}')";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    }

    



