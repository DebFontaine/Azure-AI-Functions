export interface FileDto {
    [x: string]: any;
    id : string; 
    name : string;
    size: number; 
    version : number;
    modifiedTime : Date;
    parents: string[]; 
    mimeType: string;
    uploadedBy: string;
    previewLink: string;
  }