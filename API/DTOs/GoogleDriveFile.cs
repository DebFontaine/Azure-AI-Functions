namespace API.DTOs;

public class GoogleDriveFile
{
    public string Id { get; set; }
    public string Name { get; set; }
    public long? Size { get; set; }
    public long? Version { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
    public IList<string> Parents { get; set; }
    public string MimeType { get; set; }
    public string UploadedBy {get; set;}
    public string DownloadedBy {get; set;}
    public string PreviewLink { get; set; }
}

public class DriveFileDownloadsDto
{
    public string DownloadedBy { get; set; }
    public DateTime? ModifiedTime { get; set; }
}

