import { Component, OnInit, ViewChild } from '@angular/core';
import { Product } from '../domain/product';
import { ProductService } from '../service/productservice';
import { FileKeywordsResponse } from 'src/app/models/fileKeywordsDto';
import { FileUpload, FileUploadErrorEvent } from 'primeng/fileupload';
import { environment } from 'src/environments/environment';

@Component({
    templateUrl: './dashboard.component.html',
    styleUrls: ['../../../assets/demo/badges.scss']
})
export class DashboardDemoComponent implements OnInit {
    @ViewChild('fileuploaderAI', { static: false }) fileUploaderAI!: FileUpload;
    @ViewChild('fileuploaderAIText', { static: false }) fileUploaderText!: FileUpload;
    @ViewChild('fileuploaderAISummary', { static: false }) fileUploaderSummary!: FileUpload;


    keyWordsResponse: FileKeywordsResponse | undefined;
    fileUploader: any;
    url = `${environment.imageKeywordsUrl}?code=${environment.imageKeywordsKey}`;
    textAnalysisUrl = `${environment.textKeywordsUrl}?code=${environment.textKeywordsKey}`;
    textSummaryUrl = `${environment.textSummarizeUrl}?code=${environment.textSummarizeKey}`;

    textKeyWords: string[] = [];
    summary: string = "";

    imageKeywordsChart: { labels: string[]; datasets: { label: string; data: number[]; backgroundColor: string; borderWidth: number; fill: boolean; barPercentage: number; }[]; };
    fileName: any;

    constructor(private productService: ProductService) {
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
    onFileUploadEventAnalyzeText(event: any) {
        this.fileName = event.files[0].name;
        console.log (event?.originalEvent.body);
        this.textKeyWords = [];
        const keywordsString = event?.originalEvent.body.keywords;
        this.textKeyWords = keywordsString.split(',')
    }
    onUploadErrorAnalyzeText(event: FileUploadErrorEvent) {
        console.log(event);
        this.fileUploaderText.clear();
    }

    onFileUploadEventSummary(event: any) {
        this.fileName = event.files[0].name;
        console.log (event?.originalEvent.body);
        this.summary = event?.originalEvent.body.summary;
    }
    onUploadErrorSummary(event: FileUploadErrorEvent) {
        console.log(event);
        this.fileUploaderText.clear();
    }
}
