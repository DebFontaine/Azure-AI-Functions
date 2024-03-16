namespace API.DTOs;

public class CommentDto
{
    public string Content { get; set; }
    // Other comment-related properties, such as the author, date, etc.
}

public class CommentsDto
{
    public string kind { get; set; }
    public List<CommentResponseDto> comments { get; set; }
}

public class CommentResponseDto
{
    public string id { get; set; }
    public string kind { get; set; }
    public DateTime createdTime { get; set; }
    public DateTime modifiedTime { get; set; }
    public List<object> replies { get; set; } // You can create a DTO for replies if needed.
    public Author author { get; set; }
    public bool deleted { get; set; }
    public string htmlContent { get; set; }
    public string content { get; set; }
}

public class Author
{
    public string displayName { get; set; }
    public string kind { get; set; }
    public bool me { get; set; }
    public string photoLink { get; set; }
}

public class DriveCommentListDTO
{
    public string Kind { get; set; }
    public List<DriveCommentDTO> Comments { get; set; }
}
public class DriveAddCommentDTO
{
    public string Content { get; set; }
}
public class DriveCommentDTO
{
    public string Id { get; set; }
    public string Kind { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool Resolved { get; set; }
    public List<DriveReplyDTO> Replies { get; set; }
    public DriveUserDTO Author { get; set; }
    public bool Deleted { get; set; }
    public string HtmlContent { get; set; }
    public string Content { get; set; }
    public string CreatedBy {get; set;}
}

public class DriveReplyDTO
{
    public string Id { get; set; }
    public string Kind { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string Action { get; set; }
    public DriveUserDTO Author { get; set; }
    public bool Deleted { get; set; }
    public string HtmlContent { get; set; }
    public string Content { get; set; }
    public string CreatedBy {get; set;}
}



public class DriveUserDTO
{
    public string DisplayName { get; set; }
    public string Kind { get; set; }
    public bool Me { get; set; }
    public string PhotoLink { get; set; }
}

public class PropertiesDTO
{
    public List<AppProperty> Properties {get; set;}
}

public class AppProperty
{
    public string Key { get; set; }
    public string Value { get; set; }
}
