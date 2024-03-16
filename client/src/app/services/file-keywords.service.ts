import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { FileKeywordsResponse } from '../models/fileKeywordsDto';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FileKeywordsService {

  private functionUrl = `${environment.imageKeywordsUrl}?code=${environment.imageKeywordsKey}`;


  constructor(private http: HttpClient) { }

  uploadFile(file: File): Observable<FileKeywordsResponse> {
    console.log("url", this.functionUrl);
    const formData: FormData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<FileKeywordsResponse>(this.functionUrl, formData);
  }
}
