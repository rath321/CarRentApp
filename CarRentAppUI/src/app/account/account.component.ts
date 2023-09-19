import { Component, OnInit } from '@angular/core';
import { UtilityService } from '../services/utility.service';
import { NavigationService } from '../services/navigation.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css'],
})
export class AccountComponent implements OnInit {
  userName: any;
  constructor(
    public utilityService: UtilityService,
    private navigationService: NavigationService
  ) {}
  toBeDeleted: any = [];
  userId: any;
  ngOnInit(): void {
    this.userName = this.utilityService.getUser().firstName;
    this.userId = this.utilityService.getUser().id;
    if (this.utilityService.isAdmin()) {
      this.navigationService.returnToBeDeletedProduct().subscribe((res) => {
        this.toBeDeleted = res;
        console.log(res);
      });
    } else {
      this.navigationService
        .returnToBeDeletedProductUser(this.userId)
        .subscribe((res) => {
          this.toBeDeleted = res;
          console.log(res);
        });
    }
  }
  getText(val: any): string {
    return val == 0 ? 'requested' : 'Accepted';
  }
  accept(cartId: any, cartItemId: any, userId: any, deleted: any) {
    console.log(deleted);
    if (!this.utilityService.isAdmin() || deleted == 1) return;

    this.navigationService
      .updateToBeDeletedProduct(cartId, cartItemId, userId)
      .subscribe(
        (res) => {
          console.log(res);
          this.navigationService.returnToBeDeletedProduct().subscribe((res) => {
            this.toBeDeleted = res;
            console.log(res);
          });
        },
        (err) => {
          console.log(err);
        }
      );
  }
}
