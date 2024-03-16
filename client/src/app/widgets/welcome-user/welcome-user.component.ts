import { Component, ViewChild } from '@angular/core';
import { AppComponent } from 'src/app/app.component';
import { AppMainComponent } from 'src/app/app.main.component';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-welcome-user',
  templateUrl: './welcome-user.component.html',
  styleUrls: ['./welcome-user.component.scss']
})
export class WelcomeUserComponent {
  @ViewChild('topbarMenu') topbarMenu: any; 

  constructor(public app: AppComponent, public appMain: AppMainComponent,public accountService: AccountService)
  {

  }

  toggleTopbarMenu(event: MouseEvent) {
    event.preventDefault(); // Prevent default behavior of the anchor tag
    this.topbarMenu.toggle(); // Toggle the visibility of the topbar menu
  }

}
