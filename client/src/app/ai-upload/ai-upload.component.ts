import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FileUpload, FileUploadErrorEvent } from 'primeng/fileupload';
import { FileKeywordsResponse, Tag } from '../models/fileKeywordsDto';
import { FilesService } from '../services/files.service';
import { Subject, map, switchMap, takeUntil } from 'rxjs';
import { UploadResponse } from '../models/uploadResponse';
import { AppProperty, PropertiesDTO } from '../models/propertiesDto';
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-ai-upload',
  templateUrl: './ai-upload.component.html',
  styleUrls: ['./ai-upload.component.scss']
})


export class AiUploadComponent {
  @ViewChild('fileuploaderAI', { static: false }) fileUploaderAI!: FileUpload;
  @Output() messageEvent = new EventEmitter<string>();
  fileUploader: any;
  autoFile: boolean;
  selectedCount = 0;
  @Input() visible: boolean = false;
  @Input() folderId: string = "";
  fileToUpload: File | undefined;
  private unsubscribe$: Subject<void> = new Subject<void>();
  private url = `${environment.imageKeywordsUrl}?code=${environment.imageKeywordsKey}`;

  keyWordsResponse: FileKeywordsResponse | undefined;
  selectedTags: boolean[] = [];
  selectedTagNames: string[] = [];

  constructor(private fileService: FilesService) { }

  onFileUploadEvent(event: any) {
    const originalFile = event.files[0];
    this.fileToUpload = new File([originalFile], originalFile.name, {
      type: originalFile.type,
      lastModified: originalFile.lastModified
    });
    this.keyWordsResponse = event?.originalEvent.body;
    console.log(this.keyWordsResponse);
    this.autoFile = true;
  }
  onUploadError(event: FileUploadErrorEvent) {
    console.log(event);
    this.fileUploader?.clear();
    this.messageEvent.emit(this.folderId);
  }

  uploadFile() {
    if (!this.fileToUpload) return;

    var formData = new FormData();
    formData.append('file', this.fileToUpload);

    // Call the uploadFile method and switch to another observable
    // representing the second method call
    this.fileService.uploadFile(this.folderId, formData).pipe(
      switchMap((response: UploadResponse) => {
        const id: string = response.message;
        console.log("id", id);
        this.messageEvent.emit(this.folderId);

        const properties: AppProperty[] = this.selectedTagNames.map(tagName => ({ key: tagName, value: tagName }));
        const propertiesDTO: PropertiesDTO = {
          properties: properties
        };

        // Call another method from fileService and return its observable
        return this.fileService.addPropertiesToFile(id,propertiesDTO);
      })
    ).subscribe(
      response => {
        console.log('Response:', response); // Output: response from anotherMethod
      },
      error => {
        console.error('Error:', error);
      }
    );
  }

  updateSelectedCount(event: any, tag: string): void {
    if (event.checked[0]) {
      if (this.selectedTagNames.length < 3) {
        this.selectedTagNames.push(tag); // Add the selected tag to the array       
      } else {
        event.checked[0] = false;
      }
    } else {
      // Remove the deselected tag from the array
      this.selectedTagNames = this.selectedTagNames.filter(selectedTag => selectedTag !== tag);
    }
    console.log("selected tag names", this.selectedTagNames);
  }

  closeDialog() {
    this.autoFile = false;
    this.visible = false;
    this.messageEvent.emit(this.folderId);
  }
  ngOnDestroy(): void {
    this.unsubscribe$.next();
    this.unsubscribe$.complete();
  }

}
