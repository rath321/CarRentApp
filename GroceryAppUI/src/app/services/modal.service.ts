import { Injectable, ComponentRef } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private modalComponentRef!: any;

  setModalComponentRef(componentRef: ComponentRef<any>) {
    this.modalComponentRef = componentRef;
  }

  closeModal() {
    if (this.modalComponentRef) {
      this.modalComponentRef.destroy();
      this.modalComponentRef = null;
    }
  }
}
