using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnet_blog_application.Models;
using Microsoft.Data.Sqlite;
using aspnet_blog_application.ViewModels;
using System.Drawing;

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

    public IActionResult Hall()
    {
	var returnModel = new HallViewModel(){Entries = GetHallData().OrderByDescending(p=>p.NumReports).ToList()};
        return View(returnModel);
    }

    public IActionResult Index()
    {
	var allPosts = GetAllPosts().OrderByDescending(p=>p.Upvotes).ToList();

	for(int i=0;i<allPosts.Count;i++){

	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT status FROM investigation WHERE id = '{allPosts[i].Id}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
                            allPosts[i].Status = reader.GetString(0);
                            }
                    } else {

                    }
                }
            }
        }

	}
        var returnModel = new PostListViewModel(){Reports = allPosts};
        return View(returnModel);
    }

    public IActionResult Details(int id)
    {
	PostModel thisPost = GetPostByID(id)[0];
	var returnModel = new DetailsViewModel();
	returnModel.CanUpvote = -1;
	if(HttpContext.Session.GetString("Username")!=null){
	    UserModel thisUser = GetUserByID(HttpContext.Session.GetInt32("ID").ToString())[0];
	    returnModel.CurrentUser = thisUser;
	    returnModel.CanUpvote = CheckCanUpvote(id);
	}
        returnModel.Report = thisPost;
	InvestigationModel myInvestigation = GetInvestigationByID(id)[0];
        returnModel.Investigation = myInvestigation;
	if(thisPost.Id!=-1){
	    returnModel.Poster = GetUserByID(thisPost.UserID)[0];
	    if(myInvestigation.UserID!="-1"){
	        returnModel.Investigator = GetUserByID(myInvestigation.UserID)[0];
	    }
	}
        return View(returnModel);
    }

    public IActionResult Edit(int id)
    {
	var userID = HttpContext.Session.GetInt32("ID");
        var returnModel = new PostViewModel(){UserID = userID.ToString(), Post = GetPostByID(id)[0]};
        return View(returnModel);
    }

    public IActionResult ChangeInvestigation(int id)
    {
	var returnModel = new InvestigationViewModel();
	if(HttpContext.Session.GetString("Username")!=null){
	    var userID = (int)HttpContext.Session.GetInt32("ID");
	    var userType = (int)HttpContext.Session.GetInt32("UserType");
	    returnModel = new InvestigationViewModel(){UserID = userID.ToString(), UserType = userType.ToString(), Invest = GetInvestigationByID(id)[0]};
	}
        return View(returnModel);
    }

     public IActionResult _InsertForm()
    {
	var userID = HttpContext.Session.GetInt32("ID").ToString();
	var returnModel = new PostViewModel(){UserID = userID};
        return View(returnModel);
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
                                    Description = reader.GetString(2),
                                    CreatedAt = reader.GetString(3),
                                    UpdatedAt = reader.GetString(4),
                                    Location = reader.GetString(5),
                                    SpotTime = reader.GetString(6),
                                    HazardType = reader.GetString(7),
                                    UserID = reader.GetString(8),
                                    Photo = reader.GetString(9),
                                    Upvotes = reader.GetInt32(10)
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

    [HttpPost]
    public ActionResult Insert(IFormFile? PhotoImage, PostModel post){
	post.Id = GetFreeID();
        post.CreatedAt = (DateTime.Now).ToString();
        post.UpdatedAt = (DateTime.Now).ToString();
	post.Upvotes = 0;
	post.Photo = null;

	string fileName = "";
	var path = "";
	if(PhotoImage != null){
	    var extension = "." + PhotoImage.FileName.Split('.')[PhotoImage.FileName.Split('.').Length - 1];
	    fileName = Guid.NewGuid().ToString() + extension;
	    path = Directory.GetCurrentDirectory() + "\\wwwroot\\images\\" + fileName;
	    using (var imageFile = new FileStream(path, FileMode.Create)){
                PhotoImage.CopyTo(imageFile);
            }
	    post.Photo = "/images/" + fileName;
	}

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO post(id, title, body, createdat, updatedat, location, spottime, hazardtype, userid, photo, upvotes) VALUES('{post.Id}', '{post.Title}', '{post.Description}', '{post.CreatedAt}', '{post.UpdatedAt}', '{post.Location}', '{post.SpotTime}', '{post.HazardType}', '{post.UserID}', '{post.Photo}', '{post.Upvotes}')";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO investigation(id, description, dateofaction, userid, status) VALUES('{post.Id}', '', '', '-1', 'Open')";
                
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

    [HttpPost]
    public ActionResult EditSubmit(IFormFile? PhotoImage, PostModel post){
        post.UpdatedAt = (DateTime.Now).ToString();

	string fileName = "";
	var path = "";
	if(PhotoImage != null){
	    var extension = "." + PhotoImage.FileName.Split('.')[PhotoImage.FileName.Split('.').Length - 1];
	    fileName = Guid.NewGuid().ToString() + extension;
	    path = Directory.GetCurrentDirectory() + "\\wwwroot\\images\\" + fileName;
	    using (var imageFile = new FileStream(path, FileMode.Create)){
                PhotoImage.CopyTo(imageFile);
            }
	    post.Photo = "/images/" + fileName;
	}

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE post SET title = '{post.Title}', body = '{post.Description}', updatedat = '{post.UpdatedAt}', location = '{post.Location}', spottime = '{post.SpotTime}', hazardtype = '{post.HazardType}', photo = '{post.Photo}' WHERE id='{post.Id}';";
                
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

    [HttpPost]
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

	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM investigation WHERE id = '{id}'";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
	}

	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM upvote WHERE postid = '{id}'";
                
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

    internal List<PostModel> GetPostByID(int id){
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
                                    Description = reader.GetString(2),
                                    CreatedAt = reader.GetString(3),
                                    UpdatedAt = reader.GetString(4),
                                    Location = reader.GetString(5),
                                    SpotTime = reader.GetString(6),
                                    HazardType = reader.GetString(7),
                                    UserID = (reader.GetInt32(8)).ToString(),
                                    Photo = reader.GetString(9),
                                    Upvotes = reader.GetInt32(10)
                                }
                            );
                            }
                    } else {
			if(postList.Count == 0){
			    postList.Add(
                                new PostModel{
                                    Id=-1,
                                    Title = null,
                                    Description = null,
                                    CreatedAt = null,
                                    UpdatedAt = null,
                                    Location = null,
                                    SpotTime = null,
                                    HazardType = null,
                                    UserID = "-1",
                                    Photo = null,
                                    Upvotes = 0
                                }
                            );
			}
                        return postList;
                    }
                }
            }
        }
	    if(postList.Count == 0){
		postList.Add(
                    new PostModel{
                    	Id=-1,
                        Title = null,
                        Description = null,
                        CreatedAt = null,
                        UpdatedAt = null,
                        Location = null,
                        SpotTime = null,
                        HazardType = null,
                        UserID = "-1",
                        Photo = null,
                        Upvotes = 0
                    }
                );
	}
            return postList;
	}


    internal List<UserModel> GetUserByID(String id){
        List<UserModel> userList = new();


        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM user WHERE id = '{id}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
                            userList.Add(
                                new UserModel{
                                    Id=reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    Password = reader.GetString(2),
                                    Email = reader.GetString(3),
                                    PhoneNum = reader.GetString(4),
                                    UserType = reader.GetInt32(5)
                                }
                            );
                            }
                    } else {
                        return userList;
                    }
                }
            }
        }
            return userList;
	}

    internal List<InvestigationModel> GetInvestigationByID(int id){
        List<InvestigationModel> investigationList = new();


        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM investigation WHERE id = '{id}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
                            investigationList.Add(
                                new InvestigationModel{
                                    Id=reader.GetInt32(0),
				    Description = reader.GetString(1),
				    DateOfAction = reader.GetString(2),
                                    UserID = reader.GetString(3),
                                    Status = reader.GetString(4)
                                }
                            );
                            }
                    } else {
			if(investigationList.Count == 0){
			    investigationList.Add(
                                new InvestigationModel{
                                    Id=-1,
                                    Description = null,
                                    DateOfAction = null,
                                    UserID = "-1",
                                    Status = "ERROR"
                                }
                            );
			}
                        return investigationList;
                    }
                }
            }
        }
	    if(investigationList.Count == 0){
		investigationList.Add(
                    new InvestigationModel{
                        Id=-1,
                        Description = null,
                        DateOfAction = null,
                        UserID = "-1",
                        Status = "ERROR"
                    }
                );
	}
            return investigationList;
	}

    [HttpPost]
    public ActionResult EditInvestigation(InvestigationModel invest){
        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE investigation SET description = '{invest.Description}', dateofaction = '{invest.DateOfAction}', userid = '{invest.UserID}', status = '{invest.Status}' WHERE id='{invest.Id}';";
                
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

    [HttpPost]
    public ActionResult Upvote(int id, int userid){
        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO upvote (userid, postid) VALUES ('{userid}', '{id}')";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE post SET upvotes = upvotes+1 WHERE id = '{id}'";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return RedirectToAction("Details", "Posts", new RouteValueDictionary(new { id = id }) );
    }

    internal int CheckCanUpvote(int id){
	String userId = HttpContext.Session.GetInt32("ID").ToString();
	int returnFlag = 1;

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT * FROM upvote WHERE postid = "+id+" AND userid = "+userId;
                
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows){
                        while(reader.Read()){
                            returnFlag = 0;
                        }
                    }
                }
            }
        }

	return returnFlag;
    }

    [HttpPost]
    public ActionResult RemoveUpvote(int id, int userid){
        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM upvote WHERE userid = '{userid}' AND postid = '{id}'";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE post SET upvotes = upvotes-1 WHERE id = '{id}';";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        return RedirectToAction("Details", "Posts", new RouteValueDictionary(new { id = id }) );
    }

    internal List<HallModel> GetHallData(){
        List<HallModel> hallList = new();
	char[] delimiters = {' ', '/'};

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM user";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
			    hallList.Add(
                                new HallModel{
				    UserId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
				    NumReports = 0
                                }
                            );
                        }
                    }
                }
            }
        }

	for(int i=0; i<hallList.Count; i++){

        using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT createdat FROM post WHERE userid = '{hallList[i].UserId}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
			    if( (reader.GetString(0).Split(delimiters))[2] == ((DateTime.Now.ToString()).Split(delimiters))[2] ){
				hallList[i].NumReports = hallList[i].NumReports+1;
			    }
                        }
                    }
                }
            }
        }

	}

            return hallList;
    }

}