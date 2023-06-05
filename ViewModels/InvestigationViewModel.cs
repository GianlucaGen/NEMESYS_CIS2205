namespace aspnet_blog_application.Models;

public class InvestigationViewModel{
    public InvestigationModel Invest {get;set;}
    public string UserID {get; set;}
    public string UserType {get; set;}

    public InvestigationViewModel(){
	UserID = "-1";
        UserType = "0";
	Invest = new InvestigationModel();
    }
}