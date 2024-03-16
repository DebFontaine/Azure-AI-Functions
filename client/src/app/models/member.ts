export interface Company{
    companyId: number;
    name: string;
    folder: string;
    photoUrl: string;
    city: string;
    country: string;
  }
  export interface CompanyAddModel{
    name: string;
    photoFile: File;
    city: string;
    country: string;
  }
  export interface CompanyAddModelAPI{
    name: string;
    city: string;
    country: string;
  }
  export interface CompanyUpdateModel{
    city: string;
    country: string;
  }
  export interface CompanyPhotoUpdateModel{
    photoUrl: string;
  }
  export interface Member {
      id: number;
      userName: string;
      photoUrl: string;
      age: number;
      knownAs: string;
      created: Date;
      lastActive: Date;
      gender: string;
      introduction: string;
      lookingFor: string;
      interests: string;
      city: string;
      country: string;
      //photos: Photo[];
      company: Company;
    }