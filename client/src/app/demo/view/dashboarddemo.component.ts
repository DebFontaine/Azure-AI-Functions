import { Component, OnInit} from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
    templateUrl: './dashboard.component.html',
    styleUrls: ['../../../assets/demo/badges.scss']
})
export class DashboardDemoComponent implements OnInit {

    url = `${environment.imageKeywordsUrl}?code=${environment.imageKeywordsKey}`;
    textAnalysisUrl = `${environment.textKeywordsUrl}?code=${environment.textKeywordsKey}`;
    textSummaryUrl = `${environment.textSummarizeUrl}?code=${environment.textSummarizeKey}`;

    ngOnInit() {

    }
}
