import { Component, OnInit } from '@angular/core';
import { Cart, Payment } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
})
export class CartComponent implements OnInit {
  usersCart: Cart = {
    id: 0,
    user: this.utilityService.getUser(),
    cartItems: [],
    ordered: false,
    orderedOn: '',
  };
  updateItems: any;

  usersPaymentInfo: Payment = {
    id: 0,
    user: this.utilityService.getUser(),
    paymentMethod: {
      id: 0,
      type: '',
      provider: '',
      available: false,
      reason: '',
    },
    totalAmount: 0,
    shipingCharges: 0,
    amountReduced: 0,
    amountPaid: 0,
    createdAt: '',
  };
  activeCartArray: any[] = [];
  usersPreviousCarts: Cart[] = [];

  constructor(
    public utilityService: UtilityService,
    private navigationService: NavigationService,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get Cart
    this.activeCartArray = [];
    this.updateItems = [];
    this.navigationService
      .getActiveCartOfUser(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersCart = res;
        let len = this.usersCart.cartItems.length;
        this.updateItems = this.usersCart.cartItems;
        // console.log(this.usersCart.cartItems[0]);
        for (var i = 0; i < len; i++) {
          this.activeCartArray.push({
            quantity: this.usersCart.cartItems[i].duration,
            id: this.usersCart.cartItems[i].product.id,
          });
        }
        // Calculate Payment
        this.utilityService.calculatePayment(
          this.usersCart,
          this.usersPaymentInfo
        );
      });

    // Get Previous Carts
    this.navigationService
      .getAllPreviousCarts(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersPreviousCarts = res;
        this.usersPreviousCarts = this.usersPreviousCarts.reverse();
      });
  }
  onPlus(id: any) {
    this.activeCartArray[id].quantity += 1;
    this.updateItems[id].duration += 1;
    // console.log(this.usersCart.cartItems);
  }
  onMinus(id: any, arrayid: any) {
    this.activeCartArray[arrayid].quantity -= 1;
    this.updateItems[id].duration -= 1;
    // console.log(this.updateItems);
  }

  onUpdate() {
    console.log(this.updateItems);
    this.updateActiveCartOfUser(
      this.usersCart.user.id,
      this.updateItems
    ).subscribe(
      (res) => {
        this.navigationService
          .getActiveCartOfUser(this.utilityService.getUser().id)
          .subscribe((res: any) => {
            this.usersCart = res;
            console.log(this.usersCart);
            let len = this.usersCart.cartItems.length;
            this.updateItems = this.usersCart.cartItems;

            // console.log(this.usersCart.cartItems[0]);
            for (var i = 0; i < len; i++) {
              this.activeCartArray[i].quantity = this.updateItems[i].duration;
            }
            // Calculate Payment
            this.utilityService.calculatePayment(
              this.usersCart,
              this.usersPaymentInfo
            );
          });
      },
      (err) => {
        console.log(err);
      }
    );
  }
  updateActiveCartOfUser(userId: number, updatedCartItems: any[]) {
    const url = `https://localhost:7013/api/Shopping/UpdateActiveCartOfUser/${userId}`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Accept: '*/*',
    });
    const options = { headers: headers };

    return this.http.put(url, updatedCartItems, options);
  }
}
