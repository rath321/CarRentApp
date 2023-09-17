import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Subject, window } from 'rxjs';
import { Cart, Payment, Product, ProductCart, User } from '../models/models';
import { NavigationService } from './navigation.service';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class UtilityService {
  toBeDeleted: any = [];
  baseurl = 'https://localhost:7255/api/Shopping/';
  changeCart = new Subject();
  constructor(
    private router: Router,
    private navigationService: NavigationService,
    private jwt: JwtHelperService,
    private http: HttpClient
  ) {
    this.authToken = this.getToken();
  }
  authToken: any;
  myUser: any;
  authRole: any;
  applyDiscount(price: number, discount: number): number {
    let finalPrice: number = price - price * (discount / 100);
    return finalPrice;
  }

  getToken() {
    return sessionStorage.getItem('user');
  }
  getRole() {
    return localStorage.getItem('authRole');
  }
  setUserDetails(user: any) {
    localStorage.setItem('id', user.id);
    localStorage.setItem('firstName', user.firstName);
    localStorage.setItem('lastName', user.lastName);
    localStorage.setItem('address', user.address);
    localStorage.setItem('mobile', user.mobile);
    localStorage.setItem('email', user.email);
    localStorage.setItem('createdAt', user.createdAt);
    localStorage.setItem('modifiedAt', user.modifiedAt);
  }
  getUser(): any {
    let id: any = localStorage.getItem('id');
    let firstName: any = localStorage.getItem('firstName');
    let lastName: any = localStorage.getItem('lastName');
    let address: any = localStorage.getItem('address');
    let mobile: any = localStorage.getItem('mobile');
    let email: any = localStorage.getItem('email');
    let createdAt: any = localStorage.getItem('createdAt');
    let modifiedAt: any = localStorage.getItem('modifiedAt');

    let user: User = {
      id: id,
      firstName: firstName,
      lastName: lastName,
      address: address,
      mobile: mobile,
      email: email,
      password: '',
      createdAt: createdAt,
      modifiedAt: modifiedAt,
    };
    return user;
  }

  setUser(token: User, authToken: any, authRole: any) {
    console.log(authToken);

    sessionStorage.setItem('user', authToken);
    localStorage.setItem('authRole', authRole);
    this.setUserDetails(token);
    this.authToken = authToken;
    this.authRole = authRole;
  }

  isLoggedIn() {
    return sessionStorage.getItem('user') ? true : false;
  }
  isAdmin() {
    let user = this.getUser().firstName.slice(0, 6);
    this.authRole = localStorage.getItem('authRole');
    if (user === 'admin-' && this.authRole === 'Admin') return true;
    return false;
  }
  logoutUser() {
    sessionStorage.removeItem('user');
    localStorage.removeItem('authRole');
    localStorage.clear();
    sessionStorage.clear();
    this.router.navigate(['']);
  }
  getProductsAll() {
    const headers = new HttpHeaders().set(
      'Authorization',
      `Bearer ` + this.authToken
    );
    // console.log(this.authToken, headers);
    return this.http.get<any[]>(this.baseurl + 'GetProductsAll', {
      headers: new HttpHeaders().set(
        'Authorization',
        `Bearer ` + this.authToken
      ),
    });
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
        cartitem.product.price * cartitem.duration,
        cartitem.product.offer.discount
      );
    }
    return pricepaid;
  }

  orderTheCart() {}
}
