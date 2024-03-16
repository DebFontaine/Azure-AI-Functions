import { Component, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { take } from 'rxjs';
import { AccountService } from '../services/account.service';
import { MembersService } from '../services/members.service';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { User } from '../models/user';

@Component({
  selector: 'app-login',
  templateUrl: './app.login.component.html',
})
export class AppLoginComponent {
  @ViewChild('usernameInput') usernameInput: ElementRef<HTMLInputElement>;
  @ViewChild('passwordInput') passwordInput: ElementRef<HTMLInputElement>;

  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map()
  user: User | undefined;
  member: Member | null = null;
  model: any = {};
  //userParams: UserParams | undefined;

  constructor(public accountService: AccountService,
    private memberService: MembersService,
    private router: Router) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        this.user = user;
        this.loadMember();

      }
    });
  }

  loadMember() {
    if (!this.user) return;
    this.memberService.getMember(this.user.username).subscribe({
      next: member => {
        this.member = member;
      }
    })
    
  }
  login() {

    if (environment.production)
      console.log("production");
    
    this.model = {
        username: this.usernameInput.nativeElement.value,
        password: this.passwordInput.nativeElement.value
      };

    this.accountService.login(this.model)
      .subscribe({
        next: () => {
          this.router.navigateByUrl('/')
        }
      });
  }
  logout() {
    this.accountService.logout();
    this.model = {};
    this.router.navigateByUrl('/')
  }

}
