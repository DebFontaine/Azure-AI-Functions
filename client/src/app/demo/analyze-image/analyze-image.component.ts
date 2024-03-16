import { Component, Input, ViewChild } from '@angular/core';
import { FileUpload, FileUploadErrorEvent } from 'primeng/fileupload';
import { FileKeywordsResponse } from 'src/app/models/fileKeywordsDto';
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-analyze-image',
  templateUrl: './analyze-image.component.html'
})

export class AnalyzeImageComponent {
  @ViewChild('fileuploaderAI', { static: false }) fileUploaderAI!: FileUpload;
  @Input() url = `${environment.imageKeywordsUrl}?code=${environment.imageKeywordsKey}`;
  
  keyWordsResponse: FileKeywordsResponse | undefined;
    fileUploader: any;

    imageKeywordsChart: { labels: string[]; datasets: { label: string; data: number[]; backgroundColor: string; borderWidth: number; fill: boolean; barPercentage: number; }[]; };
    fileName: any;

    constructor() {
    }

    ngOnInit() {

        this.imageKeywordsChart = {
            labels: [],
            datasets: [
                {
                    label: 'Confidence',
                    data: [],
                    backgroundColor: 'rgba(57, 132, 184, .7)',
                    borderWidth: 0,
                    fill: false,
                    barPercentage: 0.5
                }
            ]
        };
    }

    onFileUploadEvent(event: any) {
        this.fileName = event.files[0].name;
        this.keyWordsResponse = event?.originalEvent.body;
        console.log(this.keyWordsResponse);
        this.formatData();
    }
    onUploadError(event: FileUploadErrorEvent) {
        console.log(event);
        this.fileUploaderAI?.clear();
    }

    formatData() {

        const labels: string[] = [];
        const data: number[] = [];

        this.keyWordsResponse.tagsResult.values.forEach(tag => {
            labels.push(tag.name);
            data.push(tag.confidence * 100); // Assuming confidence is a percentage (0-1)
        });

        // Creating the imageKeywordsChart object
        this.imageKeywordsChart = {
            labels: labels,
            datasets: [
                {
                    label: 'Confidence',
                    data: data,
                    backgroundColor: 'rgba(57, 132, 184, .7)',
                    borderWidth: 0,
                    fill: false,
                    barPercentage: 0.5
                }
            ]
        }
    }
   

    
}


