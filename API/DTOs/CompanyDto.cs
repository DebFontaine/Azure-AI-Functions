using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace API;

public class CompanyDto
{
    public string Id { get; set;}
    public string Name { get; set;}
    public string Folder { get; set;}
    public string PhotoUrl { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}
public class AddCompanyDto
{
    public string Name { get; set;}
    public string Folder { get; set;}
    public string PhotoUrl { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}
public class CompanyUpdateDto
{
    public string City { get; set; }
    public string Country { get; set; }
}
public class CompanyPhotoUpdateDto
{
    public string PhotoUrl { get; set; }
}
