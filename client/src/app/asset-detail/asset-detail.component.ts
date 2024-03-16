import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import * as saveAs from 'file-saver';
import { MenuItem } from 'primeng/api';
import { fileComment, Revision, downloads } from '../models/file';
import { FileDto } from '../models/fileDto';
import { FilesService } from '../services/files.service';

@Component({
  selector: 'app-asset-detail',
  templateUrl: './asset-detail.component.html'
})
export class AssetDetailComponent implements OnChanges{
  @Input() fileInfo: FileDto | undefined;
  comments!: fileComment[];
  versions!: Revision[];
  downloads: downloads[] = [];
  items: MenuItem[] | undefined;

  activeItem: MenuItem | undefined;
  activeTab:number = 0;
  addVersion = false;
  addComment = false;
  resolveComment = false;
  commentToResolveId = "";
  inputComment = "";
  replyComment = "";
  newComment: fileComment | undefined
  resolvedComment: fileComment | undefined

  constructor(private fileService: FilesService){

  }
  ngOnChanges(changes: SimpleChanges) {
    const log: string[] = [];
    for (const propName in changes) {
      const changedProp = changes[propName];
      const to = JSON.stringify(changedProp.currentValue);
      console.log(this.fileInfo);
    }
    this.onTabChanged();
  }


  ngOnInit() {
    this.items = [
      { label: 'Action Items', icon: 'pi pi-fw pi-pencil' },
      { label: 'Versions', icon: 'pi pi-fw pi-file' },
      { label: 'Info', icon: 'pi pi-fw pi-info' },
    ];

    this.activeItem = this.items[0];
    this.onTabChanged();
  }
  get numComments(): number{
    return (this.comments != null ? this.comments.length : 0);
  }
  onTabChanged(){
    console.log("ActiveTab",this.activeTab)
    if(this.activeTab == 0)
      this.getCommentsForFile();
    else if(this.activeTab == 1)
      this.getRevisionsForFile();
    else
      this.getDownloadsForFile();
  }
  handleChange(event: any){
    if(this.activeTab == 0)
      this.getCommentsForFile();
    else if(this.activeTab == 1)
      this.getRevisionsForFile();
    else
      this.getDownloadsForFile();
  }
  getCommentsForFile()
  {
    if(!this.fileInfo) return;
    let id = this.fileInfo['id'];
    this.fileService.getCommentsForFile(id).
      subscribe({
        next: comments => {
          this.comments = comments;
          console.log("Comments", this.comments);
        }
      });
  }
  getDownloadsForFile()
  {
    if(!this.fileInfo) return;
    let id = this.fileInfo['id'];
    this.fileService.getDownloadsForFile(id).
      subscribe({
        next: downloads => {
          this.downloads = downloads;
          console.log("Downloads", this.downloads);
        }
      });
  }
  handleUploadComplete(event: any)
  {
    this.toggleAddVersion();
    this.getRevisionsForFile();
  }
  toggleAddVersion()
  {
    this.addVersion = !this.addVersion;
  }
  toggleAddComment()
  {
    this.addComment = !this.addComment
  }
  resolveCommentByReply()
  {
    if(this.replyComment && this.replyComment != "")
      this.fileService.resolveCommentForFile(this.fileInfo!.id, this.commentToResolveId, this.replyComment).
      subscribe({
        next: comment => {
          this.resolvedComment = comment;
          console.log("Comment Added", this.replyComment);
          this.resolveComment = false;
          this.replyComment = "";
          var indexToUpdate = this.comments.findIndex(c => c.id == this.commentToResolveId);
          console.log("index", indexToUpdate)
          if(indexToUpdate != -1)
              this.comments[indexToUpdate].resolved = true;
          this.commentToResolveId ="";
        },
        error: error => {
          console.log(error)
          this.resolveComment = false;
          this.commentToResolveId = "";
          this.replyComment = "";
        }
      });
  }
  
  toggleResolveComment(id: string)
  {
    this.resolveComment = !this.resolveComment;
    this.commentToResolveId = id;
  }

  addNewComment()
  {
    console.log("Add Comment", this.inputComment)
    if(this.inputComment && this.inputComment != "")
      this.fileService.addCommentToFile(this.fileInfo!.id, this.inputComment).
      subscribe({
        next: comment => {
          this.newComment = comment;
          console.log("Comment Added", this.newComment);
          this.addComment = false;
          this.inputComment = "";
          this.addCommentToList(this.newComment)
        },
        error: error => {
          console.log(error)
          this.addComment = false;
        }
      });
  }
  addCommentToList(comment: fileComment)
  {
    this.comments.push(comment);
  }
  getRevisionsForFile()
  {
    if(!this.fileInfo) return;
    let id = this.fileInfo['id'];
    this.fileService.getRevisionsForFile(id).
      subscribe({
        next: revisions => {
          this.versions = revisions;
        }
      });
  }
  downloadFile(revisionId: string)
  {
    if(!this.fileInfo) return;

    let id = this.fileInfo['id'];
    this.fileService.downloadFileVersion(id, revisionId).
      subscribe(data => saveAs(data, revisionId+this.fileInfo!['name']));
  }
}
