using API.DTOs;
using Google.Apis.Drive.v3.Data;

namespace API.Dtos;
public class DriveChangesDto
{
    public string ChangeType { get; set; }
    public bool Removed { get; set; }
    public FileChangesDTO File { get; set; }
    public string FileId { get; set; }
    public DateTime Time { get; set; }
    public string DriveId { get; set; }
    public string Type { get; set; }
    public string TeamDriveId { get; set; }
    //public DriveDTO Drive { get; set; }
}

public class FileChangesDTO
{
    public string Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public string Description {get; set;}
    public string FileExtension { get; set; }
    public string HeadRevisionId { get; set; }
    public LastModifyingUser LastModifyingUser { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string Name { get; set; }
    public string OriginalFileName { get; set; }
    //public Dictionary<string,string> Properties { get; set; }

}


