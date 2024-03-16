

using System.ComponentModel.DataAnnotations.Schema;
using System.Formats.Asn1;
using API.Extensions;
using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    public DateOnly DateOfBirth { get; set; }

    public string KnownAs { get; set; }

    public DateTime Created  { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set;}  = DateTime.UtcNow;
    public string Gender { get; set; }

    public string Introduction { get; set; }
    public string LookingFor { get; set; }

    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public List<Photo> Photos { get; set; } = new List<Photo>();
    public List<UserLike> LikedByUsers { get; set; }
    
    public List<UserLike> LikedUsers { get; set; }
    public List<Message> MessagesSent { get; set; }
    public List<Message> MessagesReceived { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; }

}

public class Company
{
    public int CompanyId { get; set;}
    public string Name { get; set;}
    public string Folder { get; set;}
    public string PhotoUrl { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

}

[Table("Photos")]
public class Photo
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }

    public string PublicId { get; set; } 

   public int AppUserId { get; set;}
   public AppUser AppUser { get; set; }
}