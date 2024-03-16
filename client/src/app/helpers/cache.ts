import {Injectable} from "@angular/core";
@Injectable({
    providedIn: 'root'
  })
export class ObjectCache<T> {
    private cache: Map<string, { value: T; expiration: number }> = new Map();
  
    // Add an object to the cache with a specified expiration time (in seconds)
    public add(id: string, value: T, expirationSeconds: number): void {
      const expiration = Date.now() + expirationSeconds * 1000;
      this.cache.set(id, { value, expiration });
    }
  
    // Get an object from the cache by ID
    public get(id: string): T | undefined {
      const item = this.cache.get(id);
      if (item && item.expiration > Date.now()) {
        return item.value;
      }
      // Remove expired or non-existent items
      this.remove(id);
      return undefined;
    }
  
    // Remove an object from the cache by ID
    public remove(id: string): void {
      this.cache.delete(id);
    }
  
    // Check if an object with a specific ID is in the cache and has not expired
    public has(id: string): boolean {
      const item = this.cache.get(id);
      return !!item && item.expiration > Date.now();
    }
  
    // Clear the cache
    public clear(): void {
      this.cache.clear();
    }
  
    // Get all objects in the cache that have not expired
    public getAll(): T[] {
      const currentTime = Date.now();
      return [...this.cache.values()]
        .filter((item) => item.expiration > currentTime)
        .map((item) => item.value);
    }
  }
  
  