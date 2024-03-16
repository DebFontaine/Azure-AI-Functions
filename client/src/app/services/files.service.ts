import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { FileDto } from '../models/fileDto';
import { UserParams } from '../models/userparams';
import { ObjectCache } from '../helpers/cache';
import { getPaginatedResults } from './paginationHelper';
import { fileComment, fileCommentAdd, Revision, downloads } from '../models/file';
import { PaginatedResult } from '../models/pagination';
import { PropertiesDTO } from '../models/propertiesDto';

@Injectable({
  providedIn: 'root'
})
export class FilesService {
  baseUrl = environment.apiUrl;
  userParams: UserParams | undefined;



  constructor(private http: HttpClient, private fileCache: ObjectCache<PaginatedResult<FileDto[]>>) {
    this.fileCache = new ObjectCache<PaginatedResult<FileDto[]>>();
  }

  getFiles() {
    console.log("getfiles", this.userParams)
    //return this.http.get<FileDto[]>(this.baseUrl + 'assets');

    let params = new HttpParams()
    params = params.append('pageNumber', 1);
    params = params.append('pageSize', 500);
    if(this.userParams)
    {
        params = params.append("queryString", this.userParams.queryString)
        params = params.append("propertySearch", this.userParams.propertySearch)
        params = params.append("propertyValue", this.userParams.propertyValue)
    }


    console.log("params", params)
    return getPaginatedResults<FileDto[]>(this.baseUrl + 'assets', params, this.http).pipe(
      map(response => {
        return response;
      })

    );
  }
  searchForFiles(folderId: string) {
    console.log("searchForFiles", this.userParams)
    //return this.http.get<FileDto[]>(this.baseUrl + 'assets');

    let params = new HttpParams()
    params = params.append('pageNumber', 1);
    params = params.append('pageSize', 500);
    if(this.userParams)
    {
        params = params.append("queryString", this.userParams.queryString)
        params = params.append("propertySearch", this.userParams.propertySearch)
        params = params.append("propertyValue", this.userParams.propertyValue)
    }


    console.log("params", params)
    return getPaginatedResults<FileDto[]>(this.baseUrl + 'assets/search/'  + folderId, params, this.http).pipe(
      map(response => {
        return response;
      })

    );
  }
  getFilesForCompany(folderId: string) {
    console.log("in getfileforcompany", folderId)
    const response = this.fileCache?.get(folderId);
    if (response)
    {
      console.log("FROM CACHE")
      return of(response);
    } 

    let params = new HttpParams()
    params = params.append('pageNumber', 1);
    params = params.append('pageSize', 500);
    if(this.userParams)
      params = params.append("queryString", this.userParams.queryString)
    console.log("params", params)
    return getPaginatedResults<FileDto[]>(this.baseUrl + 'assets/' + folderId, params, this.http).pipe(
      map(response => {
        if(response.result){
          this.fileCache.add(folderId, response, 5)
        }
        console.log("response",response)
        return response;
      })

    );


    //return this.http.get<FileDto[]>(this.baseUrl + 'assets/' + folderId ).pipe(
    // map(response => {
    // this.fileCache.add(folderId, response,5)
    // return response;
    // }))
  }
  deleteFile(fileId: string) {
    return this.http.delete(this.baseUrl + 'assets/' + fileId);
  }


  downloadFile(fileId: string) {
    return this.http.get(this.baseUrl + 'assets/download/' + fileId, { responseType: 'blob' });
  }
  downloadFileVersion(fileId: string, versionId: string) {
    return this.http.get(this.baseUrl + 'assets/download-version/' + fileId + "/" + versionId, { responseType: 'blob' });
  }

  uploadFile(folderId: string, file: FormData) {
    console.log("UPLOADFILE", file)
    return this.http.post(this.baseUrl + 'assets/add-file/' + folderId, file);
  }

  addfolder(folderId: string, folderName: string) {
    return this.http.post(this.baseUrl + 'assets/add-folder/' + folderId + "/" + folderName, {});
  }

  getCommentsForFile(fileId: string) {
    console.log("file id", fileId)
    return this.http.get<fileComment[]>(this.baseUrl + 'assets/comments/' + fileId);
  }
  resolveCommentForFile(fileId: string, commentId: string, comment: string) {
    console.log("file id", this.baseUrl + 'assets/resolve-comment/' + fileId + '/' + commentId)
    const commentToAdd: fileCommentAdd = {
      content: comment,
    };
    console.log(commentToAdd)
    return this.http.post<fileComment>(this.baseUrl + 'assets/resolve-comment/' + fileId + '/' + commentId, commentToAdd);
  }
  addCommentToFile(fileId: string, comment: string) {
    console.log("file id", this.baseUrl + 'assets/comments/' + fileId)
    const commentToAdd: fileCommentAdd = {
      content: comment,
    };
    console.log(commentToAdd)
    return this.http.post<fileComment>(this.baseUrl + 'assets/comments/' + fileId, commentToAdd);
  }
  getRevisionsForFile(fileId: string) {
    console.log("file id", fileId)
    return this.http.get<Revision[]>(this.baseUrl + 'assets/revisions/' + fileId);
  }
  getDownloadsForFile(fileId: string) {
    console.log("file id", fileId)
    return this.http.get<downloads[]>(this.baseUrl + 'assets/downloads/' + fileId);
  }
  copyFileTofolder(fileId: string, folderId: string) {
    console.log("file id", fileId)
    console.log("folder id", folderId)
    return this.http.put<FileDto>(this.baseUrl + 'assets/copy-file/' + fileId + '/' + folderId, {});
  }
  addPropertiesToFile(fileId: string, properties: PropertiesDTO ) {
    console.log("file id", this.baseUrl + 'assets/properties/' + fileId)

    return this.http.post(this.baseUrl + 'assets/properties/' + fileId, properties);
  }


}
