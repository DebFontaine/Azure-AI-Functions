export interface FileKeywordsResponse {
    modelVersion: string;
    metadata: Metadata;
    tagsResult: TagsResult;
  }
  
  export interface Metadata {
    width: number;
    height: number;
  }
  
  export interface TagsResult {
    values: Tag[];
  }
  
  export interface Tag {
    name: string;
    confidence: number;
  }