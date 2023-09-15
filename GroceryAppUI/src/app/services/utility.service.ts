import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Subject, window } from 'rxjs';
import { Cart, Payment, Product, ProductCart, User } from '../models/models';
import { NavigationService } from './navigation.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class UtilityService {
  toBeDeleted: any = [];
  changeCart = new Subject();
  constructor(
    private router: Router,
    private navigationService: NavigationService,
    private jwt: JwtHelperService
  ) {}

  applyDiscount(price: number, discount: number): number {
    let finalPrice: number = price - price * (discount / 100);
    return finalPrice;
  }
  returnProductAddition(itemKeys: any) {
    this.toBeDeleted.push(itemKeys);
  }
  // JWT Helper Service : npm install @auth0/angular-jwt
  returnProductDeletion(cartItemId: any, cartId: any) {}
  getUser(): User {
    let token = this.jwt.decodeToken();
    let user: User = {
      id: token?.id,
      firstName: token?.firstName,
      lastName: token?.lastName,
      address: token?.address,
      mobile: token?.mobile,
      email: token?.email,
      password: '',
      createdAt: token?.createdAt,
      modifiedAt: token?.modifiedAt,
    };
    return user;
  }

  setUser(token: string) {
    localStorage.setItem('user', token);
  }

  isLoggedIn() {
    return localStorage.getItem('user') ? true : false;
  }
  isAdmin() {
    let user = this.getUser().firstName.slice(0, 6);
    if (user === 'admin-') return true;
    return false;
  }
  logoutUser() {
    localStorage.removeItem('user');
    this.router.navigate(['']);
  }

  addToCart(product: Product, duration: number) {
    let productid = product.id;
    let userid = this.getUser().id;
    this.navigationService
      .addToCart(userid, productid, duration)
      .subscribe((res: any) => {
        if (res.toString() === 'inserted') this.changeCart.next(1);
      });
  }

  calculatePayment(cart: Cart, payment: Payment) {
    payment.totalAmount = 0;
    payment.amountPaid = 0;
    payment.amountReduced = 0;
    for (let cartitem of cart.cartItems) {
      payment.totalAmount += cartitem.product.price * cartitem.duration;

      payment.amountReduced +=
        cartitem.product.price * cartitem.duration -
        this.applyDiscount(
          cartitem.product.price * cartitem.duration,
          cartitem.product.offer.discount
        );

      payment.amountPaid += this.applyDiscount(
        cartitem.product.price * cartitem.duration,
        cartitem.product.offer.discount
      );
    }

    if (payment.amountPaid > 50000) payment.shipingCharges = 2000;
    else if (payment.amountPaid > 20000) payment.shipingCharges = 1000;
    else if (payment.amountPaid > 5000) payment.shipingCharges = 500;
    else payment.shipingCharges = 200;
  }

  calculatePricePaid(cart: Cart) {
    let pricepaid = 0;
    for (let cartitem of cart.cartItems) {
      pricepaid += this.applyDiscount(
        cartitem.product.price,
        cartitem.product.offer.discount
      );
    }
    return pricepaid;
  }

  orderTheCart() {}
}
