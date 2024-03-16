export interface fileComment {
    id: string;
    kind: string;
    createdTime: string;
    modifiedTime: string;
    resolved: boolean;
    replies: fileReply[];
    author: fileUser;
    deleted: boolean;
    htmlContent: string;
    content: string;
    createdBy: string;
  }
  export interface fileCommentAdd {
    content: string;
  }

  export interface fileReply {
    id: string;
    kind: string;
    createdTime: string;
    modifiedTime: string;
    action: string;
    author: fileUser;
    deleted: boolean;
    htmlContent: string;
    content: string;
  }
  export interface fileUser {
    displayName: string;
    kind: string;
    me: boolean;
    photoLink: string;
  }

  export interface createComment {
    content: string;
  }

  export interface downloads {
    downloadedBy: string;
    modifiedTime: Date;

  }

  export interface Revisions {
    kind: string
    revisions: Revision[]
  }
  
  export interface Revision {
    id: string
    mimeType: string
    kind: string
    published: boolean
    keepForever: boolean
    md5Checksum: string
    modifiedTime: Date
    size: number
    originalFilename: string
    lastModifyingUser: LastModifyingUser
    uploadedBy: string
  }
  
  export interface LastModifyingUser {
    displayName: string
    kind: string
    me: boolean
    permissionId: string
    emailAddress: string
    photoLink: string
  }
  