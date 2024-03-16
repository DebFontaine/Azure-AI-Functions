using System.ComponentModel.DataAnnotations;
using API.Entities;
using CloudinaryDotNet.Actions;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string Username { get; set;}

    [Required] public string KnownAs { get; set; }
    [Required] public string Gender { get; set; }
    [Required] public DateOnly? DateOfBirth { get; set; } //optional to make required work!!
    public string City { get; set; }
    public string Country { get; set; }
    
    [Required] 
    [StringLength(8, MinimumLength = 4)]
    public string Password { get; set;}
    [Required] public int CompanyId {get; set;}
}
public class UpdatePasswordDto
{
    [Required]
    public string Username { get; set;}
    [Required]
    [StringLength(8, MinimumLength = 4)]
    public string Password { get; set;}
}
