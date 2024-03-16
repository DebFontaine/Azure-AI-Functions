namespace API.Helpers;

public class UserParams : PaginationParams
{
    public string CurrentUsername { get; set; }
    public string Gender { get; set;}
    public string City { get; set;}
    public int CompanyId { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set;} = 100;   
    public string OrderBy { get; set;} = "lastActive";

    public string QueryString {get; set;} = "";
    public string PropertySearch {get; set;} = "";
    public string PropertyValue {get; set;} = "";

    public string FolderId {get;set;} = "";

}
