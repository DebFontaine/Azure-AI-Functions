import { User } from "./user";

export class UserParams{
    gender = 'All';
    minAge = 18;
    maxAge = 99;
    city = '';
    companyId = -1;
    pageNumber = 1;
    pageSize = 5;
    orderBy = "lastActive";
    queryString ="";
    propertySearch = "";
    propertyValue = "";

    constructor(user: User){
        //this.gender = user.gender === 'female' ? 'male' : 'female';
    }
}