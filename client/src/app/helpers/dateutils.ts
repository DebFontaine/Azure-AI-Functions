export class DateHelper {
   
    public static formatDateTimeFromDate(inputDate: Date): string {
        if(!inputDate) return '';
        const date = new Date(inputDate);
        const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Month is zero-based
        const day = date.getDate().toString().padStart(2, '0');
        const year = date.getFullYear().toString();
        const hours = date.getHours();
        const minutes = date.getMinutes().toString().padStart(2, '0');
        const seconds = date.getSeconds().toString().padStart(2, '0');
        const amOrPm = hours >= 12 ? 'PM' : 'AM';
        const formattedHours = (hours % 12 || 12).toString(); // Convert to 12-hour format
    
        return `${month}-${day}-${year} ${formattedHours}:${minutes}:${seconds} ${amOrPm}`;
    }

}
export function formatDate(inputDate: string): string {
    // Parse the input date string into a Date object
    const dt = new Date(inputDate);
  
    // Format the date into "MM-DD-YYYY" format
    const formattedDate = `${(dt.getMonth() + 1).toString().padStart(2, '0')}-${dt.getDate().toString().padStart(2, '0')}-${dt.getFullYear()}`;
  
    return formattedDate;
  }