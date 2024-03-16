using API.DTOs;

namespace API.Dtos;


    public class LastModifyingUser
    {
        public string DisplayName { get; set; }
        public string Kind { get; set; }
        public bool Me { get; set; }
        public string PermissionId { get; set; }
        public string EmailAddress { get; set; }
        public string PhotoLink { get; set; }
    }

    public class DriveRevisionDto
    {
        public string Id { get; set; }
        public string MimeType { get; set; }
        public string Kind { get; set; }
        public bool Published { get; set; }
        public bool KeepForever { get; set; }
        public string Md5Checksum { get; set; }
        public DateTime ModifiedTime { get; set; }
        public long Size { get; set; }
        public string OriginalFilename { get; set; }
        public DriveUserDTO LastModifyingUser { get; set; }
        public string UploadedBy {get; set;}
    }

    public class DriveRevisionListDTO
    {
        public string Kind { get; set; }
        public List<DriveRevisionDto> Revisions { get; set; }
    }


