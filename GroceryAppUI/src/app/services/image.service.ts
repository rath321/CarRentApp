import { Injectable } from '@angular/core';
import { AngularFireStorage } from '@angular/fire/compat/storage';
import { map } from 'rxjs';
import { last } from 'rxjs';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ImageService {
  imageUrl: any;
  constructor(private storage: AngularFireStorage) {}
  uploadImage(imageFile: File): Observable<any> {
    const filePath = `path/to/images/${imageFile.name}`;
    const storageRef = this.storage.ref(filePath);
    const uploadTask = this.storage.upload(filePath, imageFile);

    return uploadTask.snapshotChanges().pipe(
      finalize(async () => {
        await new Promise((resolve) => setTimeout(resolve, 6000));
        const downloadURL = await storageRef.getDownloadURL().toPromise();
        this.imageUrl = downloadURL;
        return this.imageUrl;
      })
    );
  }
}
