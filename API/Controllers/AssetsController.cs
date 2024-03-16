using System.Net.Http.Headers;
using API.Controllers;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Dtos;
using Google.Apis.Drive.v3;
using API.Data;

namespace API.Conrollers;
[Authorize]
public class AssetsController : BaseApiController
{
    private readonly IGoogleDriveRepository _driveRepo;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IGoogleDriveRepository driveRepo, IUserRepository userRepository, ILogger<AssetsController> logger)
    {
        _driveRepo = driveRepo;
        _userRepository = userRepository;
        _logger = logger;
    }
    [HttpGet("driveId")]
    public async Task<ActionResult<string>> GetDriveId()
    {
        var id = await _driveRepo.GetDriveId();
        return Ok(id);
    }
    #region files/folders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GoogleDriveFile>>> GetDriveFiles([FromQuery] UserParams userParams)
    {
        var user = User.GetUsername();
        var driveFiles = await _driveRepo.GetDriveFiles(user, userParams);
        if (!driveFiles.IsSuccess)
            return BadRequest($"Failed to retrieve files: {driveFiles.ErrorMessage}");

        return Ok(driveFiles.Data);
    }
    [HttpGet("{folderId}")]
    public async Task<ActionResult<IEnumerable<GoogleDriveFile>>> GetFolderContents(string folderId, [FromQuery] UserParams userParams)
    {
        var user = User.GetUsername();
        var result = await _driveRepo.GetFolderContents(folderId, userParams, user);
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(result.ErrorMessage);

    }
    [HttpGet("search/{folderId}")]
    public async Task<ActionResult<IEnumerable<GoogleDriveFile>>> SearchFolderContents(string folderId, [FromQuery] UserParams userParams)
    {
        var user = User.GetUsername();
        var result = await _driveRepo.GetFolderContentsRecursive(folderId, userParams, user);
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(result.ErrorMessage);

    }
    [HttpPost("add-file/{folderId}")]
    public async Task<ActionResult<string>> UploadFile(string folderId, [FromForm] IFormFile file)
    {
        if (file.ContentType == "application/octet-stream")
            return BadRequest($"Failed to upload: Prohibited filetype {file.ContentType}");
        var user = User.GetUsername();
        var u = await _userRepository.GetUserWithRolesByUsernameAsync(user);
        if (u != null && u.UserRoles != null)
        {
            var roles = u.UserRoles.Select(r => r.RoleId).ToList();
            //if(roles.Contains(2) || roles.Contains(3))
            //{
            var fileResult = await _driveRepo.UploadFileToFolder(folderId, file, user);

            if (fileResult.IsSuccess)
            {
                _logger.LogInformation($"Uploaded file:{fileResult.Data} ");
                return Ok(new { message = fileResult.Data.ToString() });
            }
            else
                return BadRequest(fileResult.ErrorMessage);
            //}
            // else return BadRequest("You do not have permission to upload files");
        }
        return BadRequest();
    }
    [HttpPut("copy-file/{fileId}/{folderId}")]
    public async Task<ActionResult<GoogleDriveFile>> CopyFile(string fileId, string folderId)
    {
        OperationResult<GoogleDriveFile> result = null;
        var user = User.GetUsername();
        var u = await _userRepository.GetUserWithRolesByUsernameAsync(user);
        if (u != null && u.UserRoles != null)
        {
            var roles = u.UserRoles.Select(r => r.RoleId).ToList();
            //if(roles.Contains(2) || roles.Contains(3))
            //{
            result = await _driveRepo.CopyFileToFolder(fileId, folderId, user);
            if (result.IsSuccess)
                return Ok(result.Data);
            else
                BadRequest(result.ErrorMessage);
            //}
            // else return BadRequest("You do not have permission to upload files");
        }
        return BadRequest(result != null ? result.ErrorMessage : "Failed to copy file");
    }
    [HttpPost("add-folder/{folderId}/{folderName}")]
    public async Task<ActionResult<string>> UploadFile(string folderId, string folderName)
    {
        var result = await _driveRepo.CreateRootSubfolder(folderId, folderName);
        if (result.IsSuccess)
            return Ok();

        return BadRequest(result.ErrorMessage);
    }
    [HttpDelete("{fileId}")]
    public async Task<ActionResult<OperationResult<bool>>> DeleteFile(string fileId)
    {
        var result = await _driveRepo.DeleteFile(fileId);
        return result.IsSuccess ? Ok() : BadRequest($"Failed to delete file: {result.ErrorMessage}");
    }
    [HttpGet("downloads/{fileId}")]
    public async Task<ActionResult<IEnumerable<DriveFileDownloadsDto>>> GetFileDownloads(string fileId)
    {
        var downloadsResult = await _driveRepo.GetDownloadsForFile(fileId);
        if (!downloadsResult.IsSuccess)
            return BadRequest($"Failed to retrieve downloads for file");

        return Ok(downloadsResult.Data);
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        // Assuming you have retrieved the file content and other details
        // Replace the following lines with your actual logic to retrieve the file content
        var downloadResult = await _driveRepo.DownloadFile(fileId, User.GetUsername());
        if (!downloadResult.IsSuccess)
            return BadRequest(downloadResult.ErrorMessage);

        byte[] fileContent = downloadResult.Data;
        string fileName = "jackman.jpg";       // Get file name
        string contentType = "image/jpg"; // Get content type

        if (fileContent == null)
        {
            return NotFound(); // Or return an appropriate error response
        }

        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = fileName, // This is for non-ASCII filenames
            FileName = fileName // This is for ASCII filenames
        };

        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
        Response.Headers.Add("Content-Type", contentType); // Set the appropriate content type

        var key = "Downloaded" + DateTime.UtcNow.Ticks;
        await _driveRepo.AddOrCreatePropertyOnFile(fileId, key, User.GetUsername());

        // Return the file content as a file stream
        return File(fileContent, contentType, fileName);
    }
    #endregion

    #region permissions
    [HttpGet("permissions-write/{fileId}")]
    public async Task<ActionResult<string>> GetFileWritePermissions(string fileId)
    {
        var permissions = await _driveRepo.GetWritersForFile(fileId);
        return Ok(permissions);
    }
    #endregion

    #region properties
    [HttpPost]
    [Route("properties/{fileId}")]
    public async Task<ActionResult> AddProperties(string fileId, [FromBody] PropertiesDTO propertiesDto)
    {
        try
        {       
            await _driveRepo.AddOrCreatePropertiesOnFile(fileId, propertiesDto.Properties.ToDictionary(p => p.Key, p => p.Value));
       
            return Ok();
        }
        catch (Exception ex)
        {
            // If an error occurs, return an error response
            return BadRequest($"Error: {ex.Message}");
        }
    }
    [HttpPatch("properties/{fileId}")]
    public async Task CreateOrAddProperty(string fileId)
    {
        var key = "Downloaded" + DateTime.UtcNow.Ticks;
        await _driveRepo.AddOrCreatePropertyOnFile(fileId, key, User.GetUsername());
    }
    [HttpGet("properties/{fileId}")]
    public async Task<ActionResult<IDictionary<string, string>>> GetProperties(string fileId)
    {
        var props = await _driveRepo.GetPropertiesForFile(fileId);
        return Ok(props);
    }
    #endregion

    #region revisions/versions
    [HttpGet("download-version/{fileId}/{versionId}")]
    public async Task<IActionResult> DownloadFileVersion(string fileId, string versionId)
    {
        // Assuming you have retrieved the file content and other details
        // Replace the following lines with your actual logic to retrieve the file content
        var downloadResult = await _driveRepo.DownloadFileVersion(fileId, versionId);
        if (!downloadResult.IsSuccess)
            return BadRequest(downloadResult.ErrorMessage);

        byte[] fileContent = downloadResult.Data;// Get file content
        string fileName = "jackman.jpg";       // Get file name
        string contentType = "image/jpg"; // Get content type

        if (fileContent == null || string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound(); // Or return an appropriate error response
        }

        var contentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = fileName, // This is for non-ASCII filenames
            FileName = fileName // This is for ASCII filenames
        };

        Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
        Response.Headers.Add("Content-Type", contentType); // Set the appropriate content type

        // Return the file content as a file stream
        return File(fileContent, contentType, fileName);
    }
    [HttpPost("add-file-version/{fileId}")]
    public async Task<ActionResult<string>> UploadFileVersion(string fileId, [FromForm] IFormFile file)
    {
        var user = User.GetUsername();
        var u = await _userRepository.GetUserWithRolesByUsernameAsync(user);
        if (u != null && u.UserRoles != null)
        {
            var roles = u.UserRoles.Select(r => r.RoleId).ToList();
            //if(roles.Contains(2) || roles.Contains(3))
            //{
            var updatedResult = await _driveRepo.UploadFileVersionToFolder(fileId, file, user);
            if (updatedResult.IsSuccess)
                return Ok();
            //}
            //else return BadRequest("You do not have permission to upload files");
        }
        return BadRequest("Error uploading file version");
    }
    [HttpGet("revisions/{fileId}")]
    public async Task<ActionResult<IEnumerable<DriveRevisionDto>>> GetFileVersions(string fileId)
    {
        try
        {
            var driveFilesResult = await _driveRepo.GetFileVersions(fileId);
            if (driveFilesResult.IsSuccess)
                return Ok(driveFilesResult.Data);
            else
                return BadRequest(driveFilesResult.ErrorMessage);
        }
        catch (Exception ex)
        {
            // If an error occurs, return an error response
            return BadRequest($"Error: {ex.Message}");
        }
    }
    #endregion

    #region comments
    [HttpGet("comments/{fileId}")]
    public async Task<ActionResult<IEnumerable<DriveCommentDTO>>> GetFileComments(string fileId)
    {
        var commentsResult = await _driveRepo.GetFileComments(fileId);
        if (commentsResult.IsSuccess)
            return Ok(commentsResult.Data);

        return BadRequest(commentsResult.ErrorMessage);
    }

    [HttpPost]
    [Route("comments/{fileId}")]
    public async Task<ActionResult<DriveCommentDTO>> AddComment(string fileId, [FromBody] DriveAddCommentDTO commentDto)
    {
        try
        {
            var commentResult = await _driveRepo.AddComment(fileId, commentDto);
            if (!commentResult.IsSuccess)
                return BadRequest(commentResult.ErrorMessage);

            var comment = commentResult.Data;
            var key = "CreatedBy" + comment.Id;
            await _driveRepo.AddOrCreatePropertyOnFile(fileId, key, User.GetUsername());
            comment.CreatedBy = User.GetUsername();

            return Ok(comment);
        }
        catch (Exception ex)
        {
            // If an error occurs, return an error response
            return BadRequest($"Error: {ex.Message}");
        }
    }
    [HttpPost]
    [Route("resolve-comment/{fileId}/{commentId}")]
    public async Task<ActionResult<DriveReplyDTO>> ResolveComment(string fileId, string commentId, [FromBody] DriveAddCommentDTO commentDto)
    {
        try
        {
            var commentResult = await _driveRepo.ResolveComment(fileId, commentId, commentDto, User.GetUsername());
            if (!commentResult.IsSuccess)
                return BadRequest(commentResult.ErrorMessage);

            var comment = commentResult.Data;
            var key = "ResolvedBy" + comment.Id;
            await _driveRepo.AddOrCreatePropertyOnFile(fileId, key, User.GetUsername());
            comment.CreatedBy = User.GetUsername();

            return Ok(comment);
        }
        catch (Exception ex)
        {
            // If an error occurs, return an error response
            return BadRequest($"Error: {ex.Message}");
        }
    }
    #endregion comments
    [HttpGet]
    [Route("drive-changes")]
    public async Task<ActionResult<IEnumerable<DriveChangesDto>>> GetDriveChanges()
    {
        try
        {
            var results = await _driveRepo.FetchChangesForDrive("");
            return Ok(results);
        }
        catch (Exception ex)
        {
            // If an error occurs, return an error response
            return BadRequest($"Error: {ex.Message}");
        }
    }
}
