import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';
import { HttpClient } from '@angular/common/http';
import { of, take } from 'rxjs';
import { AccountService } from './account.service';
import { Member } from '../models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User | undefined;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
       next: user => {
         if(user){
           this.user = user;
         }
       }
     });
    }

    getMember(username: string){
      const member = [...this.memberCache.values()]
         .reduce((arr, elem) => arr.concat(elem.result),[])
         .find((member: Member) => member.userName === username);
      
     if(member)
         return of(member);
      return this.http.get<Member>(this.baseUrl + 'users/' + username);
   }
}
