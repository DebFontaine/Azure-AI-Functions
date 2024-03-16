import { TreeNode } from "primeng/api";
import { FileDto } from "../models/fileDto";
import { DateHelper } from "./dateutils";

export interface fileJson {
    id: string;
    name: string;
    size: string;
    modifiedTime: Date;
    type: string;
    uploadedBy:string;
    previewLink: string;
    parentNode: string[];
  }

export class FileUtils {

  static findIndex(filesOnly: TreeNode[], file: FileDto): number {
    let index = -1;
    for (let i = 0; i < filesOnly.length; i++) {
      if (file.id === filesOnly[i].data.id) {
        index = i;
        break;
      }
    }
    return index;
  }

  static findIndexById(filesOnly: TreeNode[], id: string): number {
    let index = -1;
    for (let i = 0; i < filesOnly.length; i++) {
      if (id === filesOnly[i].data.id) {
        index = i;
        break;
      }
    }
    return index;
  }

  static createNodeFromFileDto(file: fileJson | null): TreeNode | null {
    if (!file || file == null) return null;

    const node: TreeNode = {
      data: {
        id: file.id,
        name: file.name,
        size: file.size,
        modifiedTime: DateHelper.formatDateTimeFromDate(file.modifiedTime),
        type: file.type.includes("vnd.google-apps.folder") ? "Folder" : file.type,
        uploadedBy: file.uploadedBy,
        previewLink: "",
        parentNode: file.parentNode
      },
      children: [],
      leaf: file.type.includes("vnd.google-apps.folder") ? false : true
    };

    return node;
  }

}