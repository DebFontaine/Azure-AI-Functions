import { Component, Input, ViewChild } from '@angular/core';
import { FileUpload, FileUploadErrorEvent } from 'primeng/fileupload';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-summarize-text',
  templateUrl: './summarize-text.component.html'
})
export class SummarizeTextComponent {
  @ViewChild('fileuploaderAISummary', { static: false }) fileUploaderSummary!: FileUpload;
  @Input() textSummaryUrl = `${environment.textSummarizeUrl}?code=${environment.textSummarizeKey}`;

  summary: string = "";
  fileName: any;


  onFileUploadEventSummary(event: any) {
    this.fileName = event.files[0].name;
    console.log (event?.originalEvent.body);
    this.summary = event?.originalEvent.body.summary;
}
onUploadErrorSummary(event: FileUploadErrorEvent) {
    console.log(event);
    this.fileUploaderSummary.clear();
}

}
