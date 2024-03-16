using System.Linq.Expressions;
using API.Dtos;
using API.DTOs;
using API.Helpers;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Interfaces;

public interface IGoogleDriveRepository
{
    Task<string> GetDriveId();

    Task<OperationResult<IEnumerable<GoogleDriveFile>>>  GetDriveFolders();
    Task<OperationResult<bool>> AddFolder(string folderId);
    Task<OperationResult<string>> CreateRootSubfolder(string folderId, string folderName);
    Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetDriveFiles(string userName, UserParams userParams = null);
    Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetFolderContents(string folderId, UserParams userParams, string userName);
    Task<OperationResult<IEnumerable<GoogleDriveFile>>> GetFolderContentsRecursive(string folderId, UserParams userParams, string userName);
    Task<OperationResult<IEnumerable<DriveFileDownloadsDto>>> GetDownloadsForFile(string fileId);
    Task<OperationResult<string>> UploadFileToFolder(string folderId, IFormFile file, string userName);
    Task<OperationResult<byte[]>> DownloadFile(string fileId, string username);
    Task<OperationResult<bool>> DeleteFile(string fileId);
    Task<OperationResult<GoogleDriveFile>> CopyFileToFolder(string fileId, string folderId, string userName);

    Task<string> GetWritersForFile(string fileId);

    Task<OperationResult<DriveCommentDTO>> AddComment(string folderId, DriveAddCommentDTO comment);
    Task<OperationResult<IEnumerable<DriveCommentDTO>>> GetFileComments(string fileId);
    Task<OperationResult<DriveReplyDTO>> ResolveComment(string fileId, string commentId, DriveAddCommentDTO comment, string userName);

    Task<OperationResult<IEnumerable<DriveRevisionDto>>> GetFileVersions(string fileId);
    Task<OperationResult<string>> UploadFileVersionToFolder(string fileId, IFormFile file, string userName);
    Task<OperationResult<byte[]>> DownloadFileVersion(string fileId, string versionId);

    Task<IEnumerable<DriveChangesDto>> FetchChangesForDrive(string savedStartPageToken);
    Task<bool> AddOrCreatePropertyOnFile(string fileId, string key,string value);
    Task<bool> AddOrCreatePropertiesOnFile(string fileId, Dictionary<string, string> properties);
    Task<IDictionary<string, string>> GetPropertiesForFile(string fileId);

    
}

