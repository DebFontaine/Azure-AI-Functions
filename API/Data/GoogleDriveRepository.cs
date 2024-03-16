using System.Linq;
using System.Text;
using API.Dtos;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace API.Data;

public class GoogleDriveRepository : IGoogleDriveRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<GoogleDriveRepository> _logger;

    public GoogleDriveRepository(IMapper mapper, ILogger<GoogleDriveRepository> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }
    #region Files/Folders
    public async Task<string> GetDriveId()
    {
        DriveService service = GetService();

        var request = service.Files.Get("root");

        var file = await request.ExecuteAsync();

        return file.Id;

    }
    public async Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetDriveFiles(string userName, UserParams userParams = null)
    {
        List<GoogleDriveFile> fileList = new List<GoogleDriveFile>();
        var fileResult = new OperationResult<IEnumerable<GoogleDriveFile>> { IsSuccess = false, Data = fileList };
        bool bPropertySearch = false;
        try
        {
            DriveService service = GetService();

            // Define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();
            // for getting folders only.
            if (userParams != null)
                if (!string.IsNullOrWhiteSpace(userParams.QueryString))
                {
                    FileListRequest.Q = userParams.QueryString;
                }
                else if (!string.IsNullOrWhiteSpace(userParams.PropertySearch) && !string.IsNullOrWhiteSpace(userParams.PropertyValue))
                {
                    bPropertySearch = true;
                    FileListRequest.Q = $"modifiedTime > '{userParams.PropertyValue}'";
                }

            FileListRequest.Fields = "nextPageToken, files(*)";

            // List file
            var result = await FileListRequest.ExecuteAsync();
            if (result.Files.Any())
            {
                foreach (var file in result.Files)
                {
                    GoogleDriveFile File = new GoogleDriveFile
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTimeDateTimeOffset != null ?
                            DateTimeOffset.Parse(file.CreatedTimeDateTimeOffset?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).DateTime.ToLocalTime()
                            : DateTime.MinValue,
                        ModifiedTime = file.ModifiedTimeDateTimeOffset != null ?
                            DateTimeOffset.Parse(file.ModifiedTimeDateTimeOffset?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).DateTime.ToLocalTime()
                            : DateTime.MinValue,
                        Parents = file.Parents,
                        MimeType = file.MimeType,
                        UploadedBy = GetUploadedBy(file),
                        DownloadedBy = GetDownloadedBy(file),
                        PreviewLink = file.ThumbnailLink
                    };
                    if (!bPropertySearch || File.DownloadedBy.Contains(userName))
                        fileList.Add(File);
                }
            }
            // For getting only folders
            // files = files.Where(x => x.MimeType == "application/vnd.google-apps.folder").ToList();
            fileResult.IsSuccess = true;
            return fileResult;
        }
        catch (Exception e)
        {
            _logger.LogError("Error retirving files for drive", e);
            fileResult.ErrorMessage = e.Message;
        }
        return fileResult;
    }
    public async Task<string> GetWritersForFile(string fileId)
    {
        DriveService service = GetService();
        var permissionsListReq = service.Permissions.List(fileId);
        permissionsListReq.Fields = "*";
        var result = await permissionsListReq.ExecuteAsync();
        IList<Permission> permissions = result.Permissions;

        // Filter permissions to get writers
        IEnumerable<Permission> writers = permissions.Where(p => p.Role == "writer").ToList();
        StringBuilder b = new StringBuilder();
        foreach (Permission p in writers)
            b.Append(p.EmailAddress);

        return b.ToString();
    }
    private string GetUploadedBy(Google.Apis.Drive.v3.Data.File file)
    {
        if (file.MimeType.Contains("application/vnd.google-apps.folder")) return "";
        var properties = file.AppProperties?.FirstOrDefault(p => p.Key == "UploadedBy");
        return properties != null ? properties.Value.Value : "System";
    }
    private string GetDownloadedBy(Google.Apis.Drive.v3.Data.File file)
    {
        if (file.MimeType.Contains("application/vnd.google-apps.folder")) return "";
        var properties = file.AppProperties?.Where(p => p.Key.StartsWith("Downloaded"));
        string concatenatedValues = "";
        if (properties != null)
        {
            // Extract and concatenate the values of key-value pairs starting with "DownloadedBy"
            concatenatedValues = string.Join(", ", properties
                .Select(p => p.Value));

            // concatenatedValues now contains the concatenated values of matching key-value pairs
        }
        return concatenatedValues;
    }

    public async Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetDriveFolders()
    {
        // List files.
        List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
        var fileResult = new OperationResult<IEnumerable<GoogleDriveFile>> { IsSuccess = false, Data = FileList };
        try
        {
            DriveService service = GetService();

            // Define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();
            // for getting folders only.
            FileListRequest.Q = "mimeType in 'application/vnd.google-apps.folder'";
            //FileListRequest.Q = "name contains 'Example'";
            //if(!string.IsNullOrWhiteSpace(userParams.QueryString))
            //    FileListRequest.Q = FileListRequest.Q + " and " + userParams.QueryString;
            FileListRequest.Fields = "nextPageToken, files(*)";


            var result = await FileListRequest.ExecuteAsync();
            if (result.Files.Any())
            {
                foreach (var file in result.Files)
                {
                    GoogleDriveFile File = new GoogleDriveFile
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        ModifiedTime = file.ModifiedTimeDateTimeOffset != null ?
                            DateTimeOffset.Parse(file.ModifiedTimeDateTimeOffset?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).DateTime
                            : DateTime.MinValue,
                        Parents = file.Parents,
                        MimeType = file.MimeType
                    };
                    FileList.Add(File);
                }
            }
            fileResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folders from drive");
            fileResult.ErrorMessage = "Error retrieving folders";
        }

        return fileResult;
    }

    public async Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetFolderContents(string folderId, UserParams userParams, string userName)
    {
        // List files.
        List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
        var fileResult = new OperationResult<IEnumerable<GoogleDriveFile>> { IsSuccess = false, Data = FileList };
        bool bPropertySearch = false;
        try
        {
            DriveService service = GetService();

            // Define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();
            // for getting folders only.
            //FileListRequest.Q = $"parents in '{folderId}'";
            FileListRequest.Q = $"'{folderId}' in parents";
            if (!string.IsNullOrWhiteSpace(userParams.QueryString))
            {
                FileListRequest.Q = FileListRequest.Q + " and " + userParams.QueryString;
            }
            else if (!string.IsNullOrWhiteSpace(userParams.PropertySearch) && !string.IsNullOrWhiteSpace(userParams.PropertyValue))
            {
                bPropertySearch = true;
                FileListRequest.Q = FileListRequest.Q + $" and modifiedTime > '{userParams.PropertyValue}'";
            }


            if (userParams.PageSize > 0)
                FileListRequest.PageSize = userParams.PageSize;

            FileListRequest.Fields = "nextPageToken, *";


            var result = await FileListRequest.ExecuteAsync();
            if (result.Files.Any())
            {
                foreach (var file in result.Files)
                {
                    GoogleDriveFile File = new GoogleDriveFile
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        ModifiedTime = file.ModifiedTimeDateTimeOffset != null ?
                            DateTimeOffset.Parse(file.ModifiedTimeDateTimeOffset?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).DateTime.ToLocalTime()
                            : DateTime.MinValue,
                        Parents = file.Parents,
                        MimeType = file.MimeType,
                        UploadedBy = GetUploadedBy(file),
                        DownloadedBy = GetDownloadedBy(file),
                        PreviewLink = file.ThumbnailLink
                    };
                    if (!bPropertySearch || File.DownloadedBy.Contains(userName))
                        FileList.Add(File);
                }
            }
            fileResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving contents for folder with id: {folderId}");
            fileResult.ErrorMessage = "Error retrieving contents for folder";
        }

        return fileResult;
    }


    public async Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetFolderContentsRecursive(string folderId, UserParams userParams, string userName)
    {
        // List files.
        List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
        var fileResult = new OperationResult<IEnumerable<GoogleDriveFile>> { IsSuccess = false, Data = FileList };
        bool bPropertySearch = false;
        try
        {
            DriveService service = GetService();

            //get all folders
            IEnumerable<string> folderIds = await GetFolders(service, folderId);

            StringBuilder sb = new StringBuilder();

            sb.Append($"('{folderId}' in parents");

            foreach (string id in folderIds)
            {
                sb.Append($" or '{id}' in parents");
            }
            sb.Append(")");
            // Define parameters of request.
            FilesResource.ListRequest FileListRequest = service.Files.List();

            FileListRequest.Q = sb.ToString();

            _logger.LogInformation($"folder query string: {FileListRequest.Q}");

            // for getting folders only.
            if (userParams != null)
            {
                if (!string.IsNullOrWhiteSpace(userParams.QueryString))
                {
                    FileListRequest.Q = FileListRequest.Q + " and " + userParams.QueryString;
                }
                else if (!string.IsNullOrWhiteSpace(userParams.PropertySearch) && !string.IsNullOrWhiteSpace(userParams.PropertyValue))
                {
                    bPropertySearch = true;
                    FileListRequest.Q = $"modifiedTime > '{userParams.PropertyValue}'";
                }
            }
            _logger.LogInformation($"full query string: {FileListRequest.Q}");
            var result = await FileListRequest.ExecuteAsync();
            if (result.Files.Any())
            {
                foreach (var file in result.Files)
                {
                    GoogleDriveFile File = new GoogleDriveFile
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        ModifiedTime = file.ModifiedTimeDateTimeOffset != null ?
                            DateTimeOffset.Parse(file.ModifiedTimeDateTimeOffset?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).DateTime.ToLocalTime()
                            : DateTime.MinValue,
                        Parents = file.Parents,
                        MimeType = file.MimeType,
                        UploadedBy = GetUploadedBy(file),
                        DownloadedBy = GetDownloadedBy(file),
                        PreviewLink = file.ThumbnailLink
                    };
                    if (!bPropertySearch || File.DownloadedBy.Contains(userName))
                        FileList.Add(File);
                }
            }
            fileResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving contents for folder with id: {folderId}");
            fileResult.ErrorMessage = "Error retrieving contents for folder";
        }

        return fileResult;
    }
    private async Task<IEnumerable<string>> GetFolders(DriveService service, string folderId)
    {
        List<string> folderList = new List<string>();

        // Define the query to retrieve folders
        string query = $"'{folderId}' in parents and mimeType = 'application/vnd.google-apps.folder'";

        // Define fields to retrieve only necessary information
        string fields = "files(id, name)";

        // Execute the request
        var request = service.Files.List();
        request.Q = query;
        request.Fields = fields;

        var result = await request.ExecuteAsync();
        var folders = result.Files;

        // Iterate through folders
        foreach (var folder in folders)
        {
            folderList.Add(folder.Id);

            // Recursively get subfolders
            folderList.AddRange(await GetFolders(service, folder.Id));
        }

        return folderList;
    }

    public async Task<OperationResult<string>> UploadFileToFolder(string folderId, IFormFile file, string userName)
    {
        var fileResult = new OperationResult<string> { IsSuccess = false, Data = "" };

        try
        {
            DriveService service = GetService();

            if (file != null && file.Length > 0)
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = file.FileName,
                    Parents = new List<string> { folderId },
                    AppProperties = new Dictionary<string, string>
                {
                    { "UploadedBy", userName }
                }
                };

                FilesResource.CreateMediaUpload request;

                using (var stream = file.OpenReadStream())
                {
                    request = service.Files.Create(fileMetadata, stream, file.ContentType);
                    request.Fields = "id";
                    await request.UploadAsync();
                }

                var uploadedFile = request.ResponseBody;

                Permission permission = new Permission
                {
                    Type = "user",
                    Role = "writer", // Set the desired access level (e.g., "reader" or "writer").
                    EmailAddress = "admin@synovisionsolutions.com",

                };

                await service.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();
                fileResult.IsSuccess = true;
                fileResult.Data = uploadedFile.Id;
            }

        }
        catch (Exception e)
        {
            _logger.LogError("Error uploading file", e);
            fileResult.ErrorMessage = e.Message;
        }

        return fileResult;
    }
    public async Task<OperationResult<byte[]>> DownloadFile(string fileId, string userName)
    {
        var downloadResult = new OperationResult<byte[]> { IsSuccess = false };
        DriveService service = GetService();

        try
        {
            var request = service.Files.Get(fileId);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
                progress =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                break;
                            }
                    }
                };
            await request.DownloadAsync(stream);

            downloadResult.IsSuccess = true;
            downloadResult.Data = stream.ToArray();

            return downloadResult;
        }
        catch (Exception e)
        {
            downloadResult.ErrorMessage = e.Message;
            if (e is AggregateException)
            {
                _logger.LogError(e.Message, e.InnerException);
            }
            else
            {
                _logger.LogError(e.Message, "Failed to download file");
            }
        }
        return downloadResult;
    }

    public async Task<OperationResult<GoogleDriveFile>> CopyFileToFolder(string fileId, string folderId, string userName)
    {
        var fileResult = new OperationResult<GoogleDriveFile> { IsSuccess = false };
        try
        {
            DriveService service = GetService();
            // Retrieve the existing parents to remove
            var getRequest = service.Files.Get(fileId);
            getRequest.Fields = "id, name, parents";
            var file = await getRequest.ExecuteAsync();
            if (file.Parents.Contains(folderId))
            {
                fileResult.ErrorMessage = "The file is already in the specified folder";
                GoogleDriveFile origFile = new GoogleDriveFile
                {
                    Id = file.Id,
                    Name = file.Name,
                    Parents = file.Parents,

                };
                fileResult.Data = origFile;
                return fileResult;
            }
            var previousParents = String.Join(",", file.Parents);
            var copy = new Google.Apis.Drive.v3.Data.File()
            {
                Parents = new List<string>() { folderId },
                AppProperties = new Dictionary<string, string>
                {
                    { "UploadedBy", userName }
                }
            };

            var copyRequest = service.Files.Copy(copy, fileId);
            copyRequest.Fields = "id, name, size, parents,kind, mimeType,appProperties";
            // Copy the file to the new folder
            file = await copyRequest.ExecuteAsync();

            //var updateRequest = service.Files.Update(copy, fileId);
            //updateRequest.Fields = "id, name, size, parents,kind, mimeType,appProperties";
            //updateRequest.AddParents = folderId;

            //updateRequest.RemoveParents = previousParents;
            //file = await updateRequest.ExecuteAsync();
            GoogleDriveFile googleFile = new GoogleDriveFile
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                Version = file.Version,
                ModifiedTime = file.ModifiedTimeDateTimeOffset != null ?
                                DateTimeOffset.Parse(file.ModifiedTimeDateTimeOffset?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind).DateTime
                                : DateTime.MinValue,
                Parents = file.Parents,
                MimeType = file.MimeType,

            };
            fileResult.IsSuccess = true;
            fileResult.Data = googleFile;
        }
        catch (Exception e)
        {
            _logger.LogError("Error copying file", e);
            fileResult.ErrorMessage = $"Failed to copy file: {e.Message}";
        }

        return fileResult;
    }

    public async Task<OperationResult<bool>> DeleteFile(string fileId)
    {
        var deleteResult = new OperationResult<bool> { IsSuccess = false };
        try
        {
            DriveService service = GetService();
            var response = await service.Files.Delete(fileId).ExecuteAsync();

            if (response != null)
                deleteResult.IsSuccess = true;
        }
        catch (Google.GoogleApiException ge)
        {
            _logger.LogError(ge, ge.Message);
            if (ge.Error.Errors != null && ge.Error.Errors.Any())
                deleteResult.ErrorMessage = ge.Error.Errors[0].Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            deleteResult.ErrorMessage = "Failed to delete file";
        }
        return deleteResult;
    }
    public Task<OperationResult<bool>> AddFolder(string folderId)
    {

        throw new NotImplementedException();

    }
    public async Task<OperationResult<string>> CreateRootSubfolder(string folderId, string folderName)
    {
        var fileResult = new OperationResult<string> { IsSuccess = false, Data = "" };
        try
        {
            DriveService service = GetService();
            Google.Apis.Drive.v3.Data.File folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            if (!string.IsNullOrWhiteSpace(folderId))
                folderMetadata.Parents = new List<string>() { folderId };

            var request = service.Files.Create(folderMetadata);
            request.Fields = "id";

            var folder = await request.ExecuteAsync();

            if (folder != null)
            {
                fileResult.IsSuccess = true;
                fileResult.Data = folder.Id;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            fileResult.ErrorMessage = ex.Message;
        }

        return fileResult;
    }
    #endregion Files/Folders
    #region Comments
    public async Task<OperationResult<IEnumerable<DriveCommentDTO>>> GetFileComments(string fileId)
    {
        // List files.
        List<DriveCommentDTO> driveComments = new List<DriveCommentDTO>();
        var commentResult = new OperationResult<IEnumerable<DriveCommentDTO>> { IsSuccess = false, Data = driveComments };

        try
        {
            DriveService service = GetService();

            CommentsResource.ListRequest request = service.Comments.List(fileId);
            request.Fields = "*";

            CommentList comments = await request.ExecuteAsync();
            driveComments = _mapper.Map<List<DriveCommentDTO>>(comments.Comments);
            IDictionary<string, string> props = null;
            if (driveComments != null)
            {
                var appProps = GetPropertiesForFile(service, fileId);
                if (appProps != null && appProps.Result != null)
                {
                    props = appProps.Result;
                }
            }
            foreach (var comment in driveComments)
            {
                var author = comment.Author?.DisplayName;
                if (author != null && !author.Contains("iam.gserviceaccount.com"))
                    comment.CreatedBy = comment.Author.DisplayName;
                else if (props != null && props.TryGetValue("CreatedBy" + comment.Id, out string info))
                {
                    comment.CreatedBy = info.Replace("CreatedBy", "");
                }
                else
                    comment.CreatedBy = "System";
            }
            commentResult.Data = driveComments;
            commentResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving comments for file with id {fileId}");
            commentResult.ErrorMessage = $"Error retrieving comments for file with id {fileId}";
        }
        return commentResult;
    }
    public async Task<OperationResult<DriveCommentDTO>> AddComment(string fileId, DriveAddCommentDTO comment)
    {
        var commentResult = new OperationResult<DriveCommentDTO> { IsSuccess = false, Data = null };
        try
        {
            DriveService service = GetService();

            // List files.
            List<DriveCommentDTO> FileList = new List<DriveCommentDTO>();
            var driveComment = _mapper.Map<Comment>(comment);
            var request = service.Comments.Create(driveComment, fileId);
            request.Fields = "*";

            var result = await request.ExecuteAsync();
            commentResult.Data = _mapper.Map<DriveCommentDTO>(result);
            commentResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            commentResult.ErrorMessage = ex.Message;
        }
        return commentResult;
    }
    public async Task<OperationResult<DriveReplyDTO>> ResolveComment(string fileId, string commentId, DriveAddCommentDTO reply, string userName)
    {
        var replyResult = new OperationResult<DriveReplyDTO> { IsSuccess = false, Data = null };
        try
        {
            DriveService service = GetService();
            var replyMetaData = new Google.Apis.Drive.v3.Data.Reply()
            {
                Action = "resolve",
                Content = $"Resolved via Web App by {userName}. {reply.Content}"
            };
            var request = service.Replies.Create(replyMetaData, fileId, commentId);
            request.Fields = "*";

            var result = await request.ExecuteAsync();

            var driveReply = _mapper.Map<DriveReplyDTO>(result);
            replyResult.IsSuccess = true;
            replyResult.Data = driveReply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            replyResult.ErrorMessage = ex.Message;
        }
        return replyResult;
    }

    #endregion Comments
    #region versions/revisions
    public async Task<OperationResult<string>> UploadFileVersionToFolder(string fileId, IFormFile file, string userName)
    {
        var uploadResult = new OperationResult<string> { IsSuccess = false, Data = "" };
        try
        {
            DriveService service = GetService();

            if (file != null && file.Length > 0)
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = file.FileName,
                    MimeType = file.ContentType,
                    AppProperties = new Dictionary<string, string>
                    {
                        { "UploadedBy", userName }
                    }
                };

                FilesResource.UpdateMediaUpload request;

                using (var stream = file.OpenReadStream())
                {
                    request = service.Files.Update(fileMetadata, fileId, stream, file.ContentType);
                    request.Fields = "id, headRevisionId";
                    var result = await request.UploadAsync();
                    if (result.Status == Google.Apis.Upload.UploadStatus.Failed)
                        uploadResult.ErrorMessage = "Failed to upload version.";
                }

                var uploadedFile = request.ResponseBody;

                //Permission permission = new Permission
                //{
                //    Type = "user",
                //    Role = "writer", // Set the desired access level (e.g., "reader" or "writer").
                //    EmailAddress = "admin@synovisionsolutions.com",

                // };

                // await service.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

                await AddOrCreatePropertyOnFile(fileId, "Revision" + uploadedFile.HeadRevisionId, userName);
                uploadResult.IsSuccess = true;
                uploadResult.Data = uploadedFile.HeadRevisionId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            uploadResult.ErrorMessage = ex.Message;
        }

        return uploadResult;

    }
    public async Task<OperationResult<byte[]>> DownloadFileVersion(string fileId, string versionId)
    {
        var downloadResult = new OperationResult<byte[]> { IsSuccess = false, Data = null };
        try
        {
            DriveService service = GetService();

            var request = service.Revisions.Get(fileId, versionId);
            var stream = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged +=
                progress =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                Console.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                Console.WriteLine("Download complete.");
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                Console.WriteLine("Download failed.");
                                break;
                            }
                    }
                };
            await request.DownloadAsync(stream);

            downloadResult.Data = stream.ToArray();
            downloadResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            downloadResult.ErrorMessage = ex.Message;
        }
        return downloadResult;

    }
    public async Task<OperationResult<IEnumerable<DriveRevisionDto>>> GetFileVersions(string fileId)
    {
        List<DriveRevisionDto> FileList = new List<DriveRevisionDto>();
        var fileResult = new OperationResult<IEnumerable<DriveRevisionDto>> { IsSuccess = false, Data = FileList };

        try
        {
            DriveService service = GetService();

            RevisionsResource.ListRequest request = service.Revisions.List(fileId);
            Dictionary<string, string> versionEntries = null;
            request.Fields = "*";

            RevisionList revisions = await request.ExecuteAsync();
            var getReq = service.Files.Get(fileId);
            getReq.Fields = "appProperties, mimeType";
            var res = await getReq.ExecuteAsync();
            if (res.AppProperties != null)
            {
                var props = await GetPropertiesForFile(fileId);
                if (props != null)
                {
                    versionEntries = props
                       .Where(entry => entry.Key.Contains("Revision") || entry.Key.Contains("UploadedBy"))
                       .ToDictionary(entry => entry.Key, entry => entry.Value);
                }
            }

            foreach (var revision in revisions.Revisions)
            {
                string uploadedBy = "System";
                var author = revision.LastModifyingUser?.DisplayName;
                if (author != null && !author.Contains("iam.gserviceaccount.com"))
                    uploadedBy = author;

                if (versionEntries != null)
                {
                    if (versionEntries.TryGetValue("Revision" + revision.Id, out string val))
                        uploadedBy = val;
                    else if (versionEntries.TryGetValue("UploadedBy", out string val2))
                        uploadedBy = val2;

                }

                var revisionDTO = _mapper.Map<DriveRevisionDto>(revision, opts => opts.Items["modifiedBy"] = uploadedBy);
                FileList.Add(revisionDTO);
            }
            fileResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            fileResult.ErrorMessage = ex.Message;
        }
        return fileResult;

    }
    #endregion versions/revisions

    #region downloads
    public async Task<OperationResult<IEnumerable<DriveFileDownloadsDto>>> GetDownloadsForFile(string fileId)
    {
        List<DriveFileDownloadsDto> downloadList = new List<DriveFileDownloadsDto>();
        var downloadResult = new OperationResult<IEnumerable<DriveFileDownloadsDto>> { IsSuccess = false, Data = downloadList };

        try
        {
            DriveService service = GetService();
            var props = await GetPropertiesForFile(service, fileId);

            var downloads = props?.Where(p => p.Key.StartsWith("Downloaded"));
            if (downloads != null)
            {
                foreach (var download in downloads)
                {
                    DateTime? downloadedTime = null;
                    var strTicks = download.Key.Replace("Downloaded", "");
                    if (long.TryParse(strTicks, out long ticks))
                    {
                        downloadedTime = new DateTime(ticks);
                    }
                    downloadList.Add(new DriveFileDownloadsDto() { DownloadedBy = download.Value, ModifiedTime = downloadedTime != null ? downloadedTime?.ToLocalTime() : null });

                }
            }
            downloadResult.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            downloadResult.ErrorMessage = ex.Message;
        }

        return downloadResult;
    }
    #endregion downloads

    #region Properties
    public async Task<IDictionary<string, string>> GetPropertiesForFile(string fileId)
    {
        DriveService service = GetService();
        return await GetPropertiesForFile(service, fileId);
    }
    private async Task<IDictionary<string, string>> GetPropertiesForFile(DriveService service, string fileId)
    {
        var getReq = service.Files.Get(fileId);
        getReq.Fields = "appProperties, mimeType";
        var res = await getReq.ExecuteAsync();
        return res.AppProperties;
    }
    public async Task<bool> AddOrCreatePropertyOnFile(string fileId, string key, string value)
    {
        DriveService service = GetService();
        var req = service.Files.Get(fileId);
        req.Fields = "*";

        bool updateNeeded = false;

        Google.Apis.Drive.v3.Data.File file = await req.ExecuteAsync();

        if (file.AppProperties == null)
        {
            file.AppProperties = new Dictionary<string, string>();
        }

        if (!file.AppProperties.ContainsKey(key))
        {
            file.AppProperties.Add(key, value);
            updateNeeded = true;
        }
        if (updateNeeded)
        {
            // Create a new file object with updated appProperties
            var updatedFile = new Google.Apis.Drive.v3.Data.File
            {
                AppProperties = file.AppProperties
            };

            // Update the file with the new custom property
            FilesResource.UpdateRequest updateRequest = service.Files.Update(updatedFile, fileId);

            await updateRequest.ExecuteAsync();
            return true;
        }
        return false;
    }
    public async Task<bool> AddOrCreatePropertiesOnFile(string fileId, Dictionary<string, string> properties)
    {
        DriveService service = GetService();
        var req = service.Files.Get(fileId);
        req.Fields = "*";

        Google.Apis.Drive.v3.Data.File file = await req.ExecuteAsync();

        bool updateNeeded = false;

        if (file.AppProperties == null)
        {
            file.AppProperties = new Dictionary<string, string>();
        }

        foreach (var kvp in properties)
        {
            if (!file.AppProperties.ContainsKey(kvp.Key))
            {
                file.AppProperties.Add(kvp.Key, kvp.Value);
                updateNeeded = true;
            }
            else
            {
                // If the property already exists, update its value
                if (file.AppProperties[kvp.Key] != kvp.Value)
                {
                    file.AppProperties[kvp.Key] = kvp.Value;
                    updateNeeded = true;
                }
            }
        }

        if (updateNeeded)
        {
            // Create a new file object with updated appProperties
            var updatedFile = new Google.Apis.Drive.v3.Data.File
            {
                AppProperties = file.AppProperties
            };

            // Update the file with the new custom properties
            FilesResource.UpdateRequest updateRequest = service.Files.Update(updatedFile, fileId);

            await updateRequest.ExecuteAsync();
            return true;
        }
        return false;
    }
    #endregion Properties


    public async Task<IEnumerable<DriveChangesDto>> FetchChangesForDrive(string savedStartPageToken)
    {
        var start = "25";//!string.IsNullOrWhiteSpace(savedStartPageToken) ? savedStartPageToken : GetStartToken();
        savedStartPageToken = start;
        List<DriveChangesDto> changesList = new List<DriveChangesDto>();
        try
        {
            DriveService service = GetService();

            // Begin with our last saved start token for this user or the
            // current token from GetStartPageToken()
            string pageToken = "152"; //await DriveFetchStartPageToken();
            while (pageToken != null)
            {
                var request = service.Changes.List(pageToken);

                request.Spaces = "drive";
                request.Fields = "*";
                var changes = await request.ExecuteAsync();
                var results = _mapper.Map<IEnumerable<DriveChangesDto>>(changes.Changes);
                changesList.AddRange(results);
                //foreach (var change in changes.Result.Changes)
                //{
                // Process change
                //Console.WriteLine("Change found for file: " + change.FileId);
                //}

                if (changes.NewStartPageToken != null)
                {
                    // Last page, save this token for the next polling interval
                    savedStartPageToken = changes.NewStartPageToken;
                }
                pageToken = changes.NextPageToken;
            }
            return changesList;
        }
        catch (Exception e)
        {
            if (e is AggregateException)
            {
                Console.WriteLine("Credential Not found");
            }
            else
            {
                throw;
            }
        }
        return changesList;
    }
    public async Task<string> DriveFetchStartPageToken()
    {
        try
        {
            DriveService service = GetService();
            var response = await service.Changes.GetStartPageToken().ExecuteAsync();
            // Prints the token value.
            Console.WriteLine("Start token: " + response.StartPageTokenValue);
            return response.StartPageTokenValue;
        }
        catch (Exception e)
        {
            // TODO(developer) - handle error appropriately
            if (e is AggregateException)
            {
                Console.WriteLine("Credential Not found");
            }
            else
            {
                throw;
            }
        }
        return null;
    }

    private static string GetMimeType(string filename)
    {
        string contentType;
        new FileExtensionContentTypeProvider().TryGetContentType(filename, out contentType);
        return contentType ?? "application/octet-stream";
    }
    private string GetStartToken()
    {
        try
        {
            DriveService service = GetService();

            var response = service.Changes.GetStartPageToken().Execute();
            return response.StartPageTokenValue;
        }
        catch (Exception e)
        {
            // TODO(developer) - handle error appropriately
            if (e is AggregateException)
            {
                Console.WriteLine("Credential Not found");
            }
            else
            {
                throw;
            }
        }
        return null;
    }
    public static Google.Apis.Drive.v3.DriveService GetService()
    {
        var serviceAccountCredentialFilePath = Directory.GetCurrentDirectory() + "/synchub-2ab4c3abb4d6.json";

        GoogleCredential credential;
        using (var stream = new FileStream(serviceAccountCredentialFilePath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                 .CreateScoped(DriveService.ScopeConstants.Drive);
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential
        });

        return service;
    }

}
