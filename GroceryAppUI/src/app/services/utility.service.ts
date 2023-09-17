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
  authRole: any;
  applyDiscount(price: number, discount: number): number {
    let finalPrice: number = price - price * (discount / 100);
    return finalPrice;
  }

  getToken() {
    return sessionStorage.getItem('authToken');
  }
  getRole() {
    return localStorage.getItem('authRole');
  }
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

  setUser(token: string, authToken: any, authRole: any) {
    console.log(authToken);

    sessionStorage.setItem('authToken', authToken);
    localStorage.setItem('authRole', authRole);
    localStorage.setItem('user', token);
    this.authToken = authToken;
    this.authRole = authRole;
  }

  isLoggedIn() {
    return localStorage.getItem('user') ? true : false;
  }
  isAdmin() {
    let user = this.getUser().firstName.slice(0, 6);
    this.authRole = localStorage.getItem('authRole');
    if (user === 'admin-' && this.authRole === 'Admin') return true;
    return false;
  }
  logoutUser() {
    localStorage.removeItem('user');
    sessionStorage.removeItem('authToken');
    localStorage.removeItem('authRole');
    this.router.navigate(['']);
  }
  getProductsAll() {
    const headers = new HttpHeaders().set(
      'Authorization',
      `Bearer ` + this.authToken
    );
    console.log(this.authToken, headers);
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
