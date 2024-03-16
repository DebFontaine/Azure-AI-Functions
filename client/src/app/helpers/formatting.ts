export function bytesToMegabytes(bytes: number): string {
    if (bytes <= 0)
      return "";
    if (bytes < 1024 * 1024)
      return (bytes / 1024).toFixed(2) + "kb"
    return (bytes / (1024 * 1024)).toFixed(2) + "mb";
  }

  export function initializeColumns(): any[] {
    return [
      { field: 'name', header: 'Name' },
      { field: 'modifiedTime', header: 'Modified' },
      { field: 'type', header: 'Type' },
      { field: 'size', header: 'Size' },
      { field: 'uploadedBy', header: 'Uploaded By' },
      { field: 'previewLink', header: 'Preview' }
    ];
  }