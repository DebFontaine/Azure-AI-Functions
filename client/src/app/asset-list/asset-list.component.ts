import { Component, ElementRef, Input, Renderer2, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import * as saveAs from 'file-saver';
import { ConfirmationService, MenuItem, MessageService as PrimeMessageService, TreeNode, TreeTableNode } from 'primeng/api';
import { FileUpload, UploadEvent, FileUploadErrorEvent } from 'primeng/fileupload';
import { Inplace } from 'primeng/inplace';
import { TTRow, TreeTable, TreeTableNodeSelectEvent, TreeTableNodeUnSelectEvent } from 'primeng/treetable';
import { take } from 'rxjs';
import { FileDto } from '../models/fileDto';
import { Member } from 'src/app/models/member';
import { PaginatedResult, Pagination } from '../models/pagination';
import { User } from '../models/user';
import { FileKeywordsResponse} from '../models/fileKeywordsDto';
import { UserParams } from '../models/userparams';
import { panelAnimation } from '../helpers/panelAnimation';
import { ObjectCache } from 'src/app/helpers/cache';
import { DateHelper, formatDate } from 'src/app/helpers/dateutils';
import { AccountService } from '../services/account.service';
import { FilesService } from '../services/files.service';
import { MembersService } from '../services/members.service';
import { iconMap } from '../helpers/icon-map';
import { bytesToMegabytes, initializeColumns } from '../helpers/formatting';
import { FileUtils, fileJson } from '../helpers/fileutils';
import { environment } from 'src/environments/environment';

interface Column {
  field: string;
  header: string;
}

interface NodeEvent {
  originalEvent: TreeTableNodeSelectEvent;
  node: TreeTableNode | TreeTableNode[];
}

@Component({
  selector: 'app-asset-list',
  templateUrl: './asset-list.component.html',
  styleUrls: ['./asset-list.component.scss'],
  animations: [panelAnimation]
})
export class AssetListComponent {
  @ViewChild('tt') tt: TreeTable | undefined;
  @Input() folderId!: string;
  files!: TreeNode[];
  filesOnly!: TreeNode[];
  cols!: Column[];
  member: Member | undefined;
  user: User | null = null;
  rawfiles: FileDto[] = [];
  loading: boolean = false;
  tablePanelWidth = 100; // Initial width for the table panel
  rightPanelWidth = 0;  // Initial width for the right panel
  showRightPanel = false;
  selectedFile: FileDto | undefined;
  baseUrl: string = environment.apiUrl + "assets/add-file/";
  @ViewChild('nameCol') nameCol: ElementRef | undefined;
  @ViewChild('searchString') searchString: ElementRef | undefined;
  @ViewChild('fileuploader', { static: false }) fileUploader!: FileUpload;
  @ViewChild('fileuploaderAI', { static: false }) fileUploaderAI!: FileUpload;
  @ViewChild('folderAddInplace', { static: false }) folderAddInplace!: Inplace;

  dateFilterOperator = "contains";
  dateFilterValue: Date | undefined;
  operators: string[] = ["=", ">=", "<="];
  userParams: UserParams | undefined;
  pagination: Pagination | undefined;
  assetFolderId = "";
  showFilters = false;
  searchInputValue: string = '';
  nameFilter: string = '';
  searchInPlace = false;
  addSubfolderInline = false;
  addTopSubfolderInline = false;
  currentNode: any = null;
  expandedIdToNode: { [id: string]: TreeNode } = {};
  selectedNode: TreeTableNode | TreeTableNode[] | null = null;

  draggedFile: fileJson | undefined | null;
  expandedRows: { [key: string]: boolean } = {};
  uploadRows: { [key: string]: boolean } = {};
  filesOnlyCache = new ObjectCache<TreeNode[]>();
  autoFile: boolean;
  keyWordsResponse: FileKeywordsResponse | undefined;

 
  constructor(private fileService: FilesService,private accountService: AccountService, private memberService: MembersService, private router: Router,
    private confirmationService: ConfirmationService, private messageService: PrimeMessageService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        this.user = user;
      }
    });
  }

  ngOnInit() {
    this.loadMember();
    this.cols = initializeColumns();
    this.initializeUserParams();

  }

  initializeUserParams(): void {
    this.userParams = new UserParams(this.user!);
    this.fileService.userParams = this.userParams;
  }

 
  nodeSelect(event: TreeTableNodeSelectEvent) {
  if(event.node != this.currentNode)
  {
    if(event.node?.expanded)
    {     
      const response = this.filesOnlyCache?.get(event.node.data.id);
      if (response)
      {
        this.currentNode = event.node;
        this.selectedNode = this.currentNode;
        console.log("FROM files only CACHE")
        this.filesOnly = response;
      }
      else
        this.onNodeExpand(event)
    }   
    else
    {
      this.currentNode = event.node;
      this.selectedNode = this.currentNode;  
      this.filesOnly = [];
    }
  } 
  
}

  nodeUnselect(event: TreeTableNodeUnSelectEvent) {
    console.log(event)
  }

  dragStart(event: DragEvent, fileDto: fileJson) {
    event.dataTransfer!.setData("filedto", JSON.stringify(fileDto))
    console.log("data added", event.dataTransfer!.getData("filedto"))
    this.draggedFile = fileDto;
  }

  drop(event: DragEvent, folderId: string) {
    console.log("drop", event)
    if(event.dataTransfer && event.dataTransfer!.getData("filedto")){
      this.draggedFile = JSON.parse(event.dataTransfer!.getData("filedto"));
    }
    if (this.draggedFile && this.draggedFile != null) {
      if (this.canDrop(folderId)) {
        this.fileService.copyFileTofolder(this.draggedFile.id, folderId).subscribe({
          next: (response => {
            if (response) {
              const fileToInsert = this.draggedFile ?? null
              const node = FileUtils.createNodeFromFileDto(fileToInsert)
              if(node != null)
              {
                this.filesOnlyCache?.remove(folderId)
                if(this.currentNode && this.currentNode != null && this.currentNode.expanded) 
                {
                  if(this.currentNode.data.id == folderId)
                  {
                    this.filesOnly = [...this.filesOnly];
                    this.filesOnly.push(node);
                  }
                }
              }
              this.messageService.add({ severity: 'info', summary: 'Confirmed', detail: 'The file was copied to the selected folder' });
              console.log(response)
            }
          })
        })
      }
      else {
        this.dragEnd()
        this.messageService.add({ severity: 'error', summary: 'Rejected', detail: 'The file is already in the selected folder' });
        this.draggedFile = null;
      }
    }
  }

  canDrop(folderId: string) {
    if (this.draggedFile) {
      if (this.draggedFile.parentNode) {
        const parentNodeArray: string[] = this.draggedFile.parentNode;
        return parentNodeArray[0] != folderId
      }
    }
    return true;
  }

  dragEnd() {
    this.draggedFile = null;
  }


  toggleFilters() {
    if (this.tt) {
      this.tt.filter("", "name", 'contains')
      this.tt.filter("", "modifiedTime", 'contains')
      this.tt.filter("", "type", 'contains')
      this.tt.filter("", "size", 'contains')
      this.tt.filter("", "uploadedBy", 'contains')
    }

    this.showFilters = !this.showFilters;
  }

  resetSearch() {
    this.searchInputValue = "";
    this.userParams!.queryString = "";
    this.fileService.userParams!.queryString = "";
    this.filesOnlyCache.clear();
    this.loadFiles(this.assetFolderId);
    this.searchInPlace = false;
    this.expandedIdToNode = {};
  }


  loadMember() {
    if (!this.user) return;
    this.memberService.getMember(this.user.username).subscribe({
      next: member => {
        this.member = member;
        var id = this.folderId ? this.folderId : this.member?.company.folder;
        if (id && id.length > 12) {
          this.assetFolderId = id;
          this.loadFiles(this.assetFolderId);
        }
        else
          this.router.navigate(['/members']);
      }
    });
  }

  confirm(event: Event) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: 'Are you sure that you want to delete this file?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.messageService.add({ severity: 'info', summary: 'Confirmed', detail: 'You have accepted' });
      },
      reject: () => {
        this.messageService.add({ severity: 'error', summary: 'Rejected', detail: 'You have rejected' });
      }
    });
  }
  toggleRightPanel() {
    this.showRightPanel = !this.showRightPanel;
    console.log("showpanel", this.showRightPanel)
  }
  closeRightPanel() {
    this.toggleRightPanel();
  }
  openRightPanel() {
    this.tablePanelWidth = 60; // Adjust widths to make space for the right panel
    this.rightPanelWidth = 40;
    this.showRightPanel = true;
  }

  searchFiles(queryString: string): void {
    console.log("queryString", queryString);
  
    if (!this.assetFolderId || !this.member) {
      return;
    }
  
    let searchPhrase = this.searchInputValue || "";
    if (!searchPhrase.includes(" ")) {
      searchPhrase = "name contains '" + searchPhrase + "'";
    }
  
    this.userParams!.queryString = searchPhrase;
    this.fileService.userParams = this.userParams;
  
    this.loading = true;
    this.fileService.searchForFiles(this.assetFolderId).subscribe({
      next: (response) => {
        if (response.result) {
          this.rawfiles = response.result;
          this.files = this.transformData(this.rawfiles);
          this.filesOnly = this.files;
          this.searchInPlace = true;
          //this.pagination = response.pagination;
        }
      }
    });
  }

  loadFiles(id: string): void {
    if (!this.member || !id) {
      return;
    }
  
    if (this.searchInputValue) {
      this.userParams!.queryString = this.searchInputValue;
      this.fileService.userParams = this.userParams;
    }
  
    this.loading = true;
    this.fileService.getFilesForCompany(id).subscribe({
      next: (response) => {
        if (response.result) {
          this.rawfiles = response.result;
          this.files = this.transformData(this.rawfiles);
          this.filesOnly = this.files;
          //this.pagination = response.pagination;
        }
      }
    });
  }

  getCommentsForFile(fileDto: any) {
    this.selectedFile = fileDto;
    console.log("getcomments", fileDto)
    this.toggleRightPanel();
  }

  deleteFile(event: any, fileDto: any) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: 'Are you sure that you want to delete this file?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        let id = fileDto['id'];
        this.fileService.deleteFile(id).
          subscribe(() => {
            var index = FileUtils.findIndex(this.filesOnly,fileDto)
            this.filesOnly = this.filesOnly?.filter((val, i) => i != index);
            console.log("DELETE", fileDto.parentNode[0])
            this.filesOnlyCache?.remove(fileDto.parentNode[0])
            this.messageService.add({ severity: 'success', summary: 'Confirmed', detail: 'File was deleted' });
          });

      },
      reject: () => {
        console.log("rejected")
        this.messageService.add({ severity: 'error', summary: 'Cancelled', detail: 'File was not deleted' });
      }
    });
  }
  downloadFile(fileDto: any) {
    console.log(fileDto);
    let id = fileDto['id'];
    this.fileService.downloadFile(id).
      subscribe(data => {
        console.log('downloading', fileDto['name'])
        saveAs(data, fileDto['name'])
      });
  }
  
  onFileUploadEvent(event: any, id:string)
  {
    console.log("FILE UPLAODED", event)
    this.toggleUpload(id)
    if(this.currentNode.expanded)
    {
      this.filesOnlyCache?.remove(this.currentNode.id)
      this.getFilesForNode(this.currentNode)
    }
  }
  onFileUploadEventAI(event: any)
  {
    console.log("FILE UPLOAD Event AI", event)
    this.keyWordsResponse = event?.originalEvent.body;
    console.log(this.keyWordsResponse);
  }
  onUploadErrorAI(event: FileUploadErrorEvent) {
    this.fileUploader?.clear();
    this.addSubfolderInline = false;
    console.log("UPLOADERROR")
  }
  onUpload(event: UploadEvent, folder: FileDto) {
    this.addSubfolderInline = false;
  }
  onBeforeUpload(event: any, folder: FileDto) {
    console.log("Before UPLOAD")
    this.addSubfolderInline = true;
  }
  onClear(event: any) {
    this.addSubfolderInline = false;
  }
  toggleTopFolderControls() {
    this.addTopSubfolderInline = !this.addTopSubfolderInline;
  }
  toggleControls(id: string) {
    console.log("toggle")
    this.addSubfolderInline = !this.addSubfolderInline;
    this.expandedRows[id] = !this.expandedRows[id];
  }
  toggleUpload(id: string)
  {
    this.uploadRows[id] = !this.uploadRows[id];
  }
  toggleAutoFile()
  {
    this.autoFile = !this.autoFile;
    console.log("Autofile : this.autoFile")
  }
  addSubfolder(fileDto: any, folderName: string) {
    this.addSubfolderInline = true;
    this.addTopSubfolderInline = false;
    if (fileDto && folderName) {

      let id = fileDto['id'];
      this.fileService.addfolder(id, folderName).
        subscribe(event => {
          console.log(event)
          this.resetSearch();
          this.addSubfolderInline = false;
          this.expandedRows[id] = !this.expandedRows[id];
        });
    }

  }
  onAddSubfolder(folderName: string) {
    if (!folderName) return;

    this.addSubfolderInline = false;
    this.addTopSubfolderInline = false;
    this.fileService.addfolder(this.assetFolderId, folderName).
      subscribe(event => {
        console.log(event)
        this.resetSearch()
      });
  }
  onUploadError(event: FileUploadErrorEvent) {
    this.fileUploaderAI?.clear();
    console.log("UPLOADERROR")
  }

  updateView() {
    this.files = [...this.files];
  }
  updateViewFilesOnly() {
    this.filesOnly = [...this.files];
  }
  onNodeCollapsed(event: any) {
    if(event.node == this.currentNode)
      this.filesOnly = [];
  }

  onNodeExpand(event: any) {
    console.log("BEGIN NODE EXPAND")
    this.loading = true;
    var inputValue = '';

    if (this.nameCol) {
      inputValue = this.nameCol.nativeElement.value;
      console.log(inputValue);
    }
    const node = event.node;
    this.currentNode = node;
    this.selectedNode = this.currentNode;

    this.getFilesForNode(node);
  }
  getFilesForNode(node: TreeNode)
  {
    setTimeout(() => {
      this.loading = false;
      this.fileService.getFilesForCompany(node.data.id).subscribe({
        next: (response => {
          node.children = this.transformData(response.result!);
          if (response.result) {
            this.files = [...this.files];
            if (node.children.length > 0) {
              this.filesOnly = node.children;
              this.filesOnlyCache?.add(node.data.id, node.children, 60)
            }
            else
              this.filesOnly = [];
          }
          else {
            this.filesOnly = [];
          }
        }),
        error: () => console.log("ERROR")
      })


    }, 250);
  }



  transformData(input: FileDto[]): TreeNode[] {
    const rootNodes: TreeNode[] = [];


    // Create a mapping of IDs to nodes
    const idToNode: { [id: string]: TreeNode } = {};

    input.forEach((file) => {
      //console.log("In transformdata", file);
      const node: TreeNode = {
        data: {
          id: file.id,
          name: file.name,
          size: bytesToMegabytes(file.size) || "",
          modifiedTime: DateHelper.formatDateTimeFromDate(file.modifiedTime),
          type: file.mimeType.includes("vnd.google-apps.folder") ? "Folder" : file.mimeType,
          uploadedBy: file.uploadedBy,
          previewLink: "",
          parentNode: file.parents
        },
        children: [],
        leaf: file.mimeType.includes("vnd.google-apps.folder") ? false : true
      };
      idToNode[file.id] = node;

      // Find the parent node and add the current node as a child
      if (file.parents) {
        file.parents.forEach((parent) => {
          const parentNode = idToNode[parent];
          if (parentNode) {
            parentNode?.children?.push(node);
          }
        });
      }


      // If no parent is found, this node becomes a root node
      //if (node.data.type === "Folder") {
      rootNodes.push(node);
      //}
    });
    //console.log("rootnodes", rootNodes);
    return rootNodes;
  }

  applyDateFilter(date: string) {
    console.log("date 1", date);
    console.log("date 2", this.dateFilterOperator)

    if (this.dateFilterOperator == '=')
      this.tt!.filter(formatDate(date), 'modifiedTime', 'contains')

  }

  getIconForFileExtension(type: string) {
    if (iconMap.hasOwnProperty(type)) {
      return iconMap[type];
    } else {
      return 'pi pi-file';
    }
  }
  receiveMessage(id: string) {
    this.toggleUpload(id);
  }


}
