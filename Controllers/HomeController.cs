using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnet_blog_application.Models;
using Microsoft.Data.Sqlite;
using aspnet_blog_application.ViewModels;

namespace aspnet_blog_application.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login()
    {
	var username = HttpContext.Session.GetString("Username");
	var returnModel = new LogInViewModel(){myUsername = username};
        return View(returnModel);
    }

    public IActionResult UserEdit()
    {
	UserModel currentUser = new UserModel();
	currentUser.Id = -1;
	if(HttpContext.Session.GetString("Username")!=null){
	    currentUser.Id = (int)HttpContext.Session.GetInt32("ID");
	    currentUser.Username = HttpContext.Session.GetString("Username");
	    currentUser.Password = HttpContext.Session.GetString("Password");
	    currentUser.Email = HttpContext.Session.GetString("Email");
	    currentUser.PhoneNum = HttpContext.Session.GetString("PhoneNum");
	    currentUser.UserType = (int)HttpContext.Session.GetInt32("UserType");
	}
	var returnModel = new UserEditViewModel(){ThisUser = currentUser};
        return View(returnModel);
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Garbage(){
        return View();
    }

    internal void GetUserByID(int id){
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
                        HttpContext.Session.SetInt32("ID", userList[0].Id);
			HttpContext.Session.SetString("Username", userList[0].Username);
			HttpContext.Session.SetString("Password", userList[0].Password);
			HttpContext.Session.SetString("Email", userList[0].Email);
			HttpContext.Session.SetString("PhoneNum", userList[0].PhoneNum);
			HttpContext.Session.SetInt32("UserType", userList[0].UserType);
                    }
                }
            }
        }
                HttpContext.Session.SetInt32("ID", userList[0].Id);
		HttpContext.Session.SetString("Username", userList[0].Username);
		HttpContext.Session.SetString("Password", userList[0].Password);
		HttpContext.Session.SetString("Email", userList[0].Email);
		HttpContext.Session.SetString("PhoneNum", userList[0].PhoneNum);
		HttpContext.Session.SetInt32("UserType", userList[0].UserType);
	}

    internal int findUserID(string Username, string Password){
	int returnID = -1;

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
			    if(reader.GetString(1)==Username && reader.GetString(2)==Password){
				returnID = reader.GetInt32(0);
			    }
                        }
                    } else {
			return returnID;
                    }
                }
            }
        }
	return returnID;
    }

    [HttpPost]
    public ActionResult TryLogin(string Username, string Password){
	int myID = findUserID(Username, Password);
	if(myID!=-1){
	    GetUserByID(myID);
	    return RedirectToAction("Login", "Home");
	}else{
	    var returnModel = new LogInViewModel(){helpMessage = "Invalid username or password."};
	    return View("Login", returnModel);
	}
    }

    [HttpPost]
    public ActionResult Logout(){
	HttpContext.Session.Clear();
	return RedirectToAction("Login", "Home");
    }

    [HttpPost]
    public ActionResult UpdateInfo(UserModel Update){
	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE user SET password = '{Update.Password}', email = '{Update.Email}', phonenum = '{Update.PhoneNum}' WHERE id = '{Update.Id}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
				
                            }
                    } else {

                    }
                }
            }
        }
	HttpContext.Session.SetString("Password", Update.Password);
	HttpContext.Session.SetString("Email", Update.Email);
	if(Update.PhoneNum==null){
	    HttpContext.Session.SetString("PhoneNum", "");
	}else{
	    HttpContext.Session.SetString("PhoneNum", Update.PhoneNum);
	}
	return RedirectToAction("Login", "Home");
    }

    [HttpPost]
    public ActionResult RegisterAccount(UserModel Update){
	Update.Id = GetFreeID();
	int IDread = -1;
	if(Update.PhoneNum==null){
	    Update.PhoneNum = "";
	}

	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM user WHERE username = '{Update.Username}'";
                
                using (var reader = command.ExecuteReader())
                {

                    if (reader.HasRows){
                        while(reader.Read()){
				IDread=reader.GetInt32(0);
                            }
                    } else {

                    }
                }
            }
        }

	if(IDread==-1){

	using(SqliteConnection connection = new SqliteConnection(_configuration.GetConnectionString("BlogDataContext")))
        {
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO user (id, username, password, email, phonenum, usertype) VALUES ('{Update.Id}', '{Update.Username}', '{Update.Password}', '{Update.Email}', '{Update.PhoneNum}', '{Update.UserType}')";
                
                try{
                    command.ExecuteNonQuery();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
	
	    	var returnModel = new LogInViewModel(){helpMessage = "Account registered successfully!"};
		return View("Login", returnModel);
	}else{
	    	var returnModel = new LogInViewModel(){helpMessage = "That username is already taken. Please use a different username."};
		return View("Login", returnModel);
	}

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
                command.CommandText = $"SELECT * FROM user";
                
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
}