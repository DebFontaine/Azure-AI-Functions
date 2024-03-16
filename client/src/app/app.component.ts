import { Component, OnInit } from '@angular/core';
import { PrimeNGConfig } from 'primeng/api';
import { User } from './models/user';
import { AccountService } from './services/account.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {

    menu = 'slim';

    layout = 'default';

    darkMenu = true;

    inputStyle = 'outlined';

    ripple: boolean;

    constructor(private primengConfig: PrimeNGConfig, private accountService: AccountService) { }

    ngOnInit() {
        this.primengConfig.ripple = true;
        this.setCurrentUser();
    }


    setCurrentUser() {
        const userString = localStorage.getItem('user');
        if (!userString) return;
        const user: User = JSON.parse(userString);
        console.log("user", user);
        this.accountService.setCurrentUser(user);
    }
}
