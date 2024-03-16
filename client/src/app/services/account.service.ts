import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: any){
    return this.http.post<User>(this.baseUrl + 'account/login', model)
      .pipe(
        map((response: User) =>{
          const user = response;
          if(user) {
            this.setCurrentUser(user);
            console.log("login", user);
          }
        })
      );
  }

  register(model: any, login: boolean){
    return this.http.post<User>(this.baseUrl + 'account/register', model)
    .pipe(
      map(user => {
        if(user && login){
          this.setCurrentUser(user);
        }
        return user;
      })
    
    )
  }
  updatePassword(model: any){
    console.log("update password", model)
    return this.http.put<boolean>(this.baseUrl + 'account/update', model);
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

  setCurrentUser(user: User){
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  getDecodedToken(token:string){
    return JSON.parse(atob(token.split('.')[1]));
  }
}
