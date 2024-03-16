import { Component, Input, ViewChild } from '@angular/core';
import { FileUpload, FileUploadErrorEvent } from 'primeng/fileupload';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-analyze-text',
  templateUrl: './analyze-text.component.html'
})
export class AnalyzeTextComponent {
  @ViewChild('fileuploaderAIText', { static: false }) fileUploaderText!: FileUpload;
  @Input() textAnalysisUrl = `${environment.textKeywordsUrl}?code=${environment.textKeywordsKey}`;
  fileName: any;
  textKeyWords: string[] = [];

  onFileUploadEventAnalyzeText(event: any) {
    this.fileName = event.files[0].name;
    console.log(event?.originalEvent.body);
    this.textKeyWords = [];
    const keywordsString = event?.originalEvent.body.keywords;
    this.textKeyWords = keywordsString.split(',')
  }
  onUploadErrorAnalyzeText(event: FileUploadErrorEvent) {
    console.log(event);
    this.fileUploaderText.clear();
  }
}
