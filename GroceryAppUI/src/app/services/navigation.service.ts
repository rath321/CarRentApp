import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs';
import {
  Category,
  Order,
  Payment,
  PaymentMethod,
  User,
} from '../models/models';

@Injectable({
  providedIn: 'root',
})
export class NavigationService {
  baseurl = 'https://localhost:7255/api/Shopping/';
  authToken: any;
  authRole: any;
  constructor(private http: HttpClient) {
    this.authToken = sessionStorage.getItem('authToken');
    this.authRole = localStorage.getItem('authRole');
    console.log(this.authToken);
  }

  getCategoryList() {
    let url = this.baseurl + 'GetCategoryList';
    return this.http.get<any[]>(url).pipe(
      map((categories: any) =>
        categories.map((category: any) => {
          let mappedCategory: Category = {
            id: category.id,
            category: category.category,
            subCategory: category.subCategory,
          };
          return mappedCategory;
        })
      )
    );
  }
  getAuthorizedData(jwtToken: string) {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${jwtToken}`,
    });

    return this.http.get<any>(this.baseurl + 'your-authorized-endpoint', {
      headers,
    });
  }
  getProducts(category: string, subcategory: string, count: number) {
    console.log(this.authToken);
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ` + this.authToken,
    });
    const options = {
      headers: headers,
      params: new HttpParams()
        .set('category', category)
        .set('subcategory', subcategory)
        .set('count', count),
    };
    return this.http.get<any[]>(this.baseurl + 'GetProducts', options);
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

  getProduct(id: number) {
    let url = this.baseurl + 'GetProduct/' + id;
    return this.http.get(url);
  }

  registerUser(user: User) {
    let url = this.baseurl + 'RegisterUser';
    return this.http.post(url, user, { responseType: 'text' });
  }
  registerUserEF(
    UserName: string,
    password: string,
    email: string,
    role: string
  ) {
    let apiUrl = 'https://localhost:7255/api/Authentication';
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
    });

    const body = {
      username: UserName,
      email: email,
      password: password,
    };

    // Append the role query parameter
    const url = `${apiUrl}/RegisterUser?role=${role}`;

    return this.http.post(url, JSON.stringify(body), { headers: headers });
  }
  loginUser(email: string, password: string) {
    let url = this.baseurl + 'LoginUser';
    return this.http.post(
      url,
      { Email: email, Password: password },
      { responseType: 'text' }
    );
  }
  loginUserEF(UserName: string, password: string) {
    let url = 'https://localhost:7255/api/Authentication/LoginUser';
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
    });

    const body = {
      username: UserName,
      password: password,
    };

    return this.http.post(url, JSON.stringify(body), {
      headers: headers,
    });
  }

  submitReview(userid: number, productid: number, review: string) {
    let obj: any = {
      User: {
        Id: userid,
      },
      Product: {
        Id: productid,
      },
      Value: review,
    };

    let url = this.baseurl + 'InsertReview';
    return this.http.post(url, obj, { responseType: 'text' });
  }

  getAllReviewsOfProduct(productId: number) {
    let url = this.baseurl + 'GetProductReviews/' + productId;
    return this.http.get(url);
  }

  addToCart(userid: number, productid: number, duration: number) {
    let url =
      this.baseurl +
      'InsertCartItem/' +
      userid +
      '/' +
      productid +
      '/' +
      duration;
    return this.http.post(url, null, { responseType: 'text' });
  }

  getActiveCartOfUser(userid: number) {
    let url = this.baseurl + 'GetActiveCartOfUser/' + userid;
    return this.http.get(url);
  }
  updateActiveCartOfUser(userid: number) {
    let url = this.baseurl + 'UpdateActiveCartOfUser/' + userid;
    // return this.http.post(url);
  }
  returnProductAddition(itemKeys: any) {
    let url =
      this.baseurl +
      'ToBeDeleted/' +
      itemKeys.cartItemId +
      '/' +
      itemKeys.cartId;
    return this.http.post(url, null);
  }
  returnToBeDeletedProduct() {
    let url = this.baseurl + 'ToBeDeleted/';
    return this.http.get(url);
  }
  deleteReturnToBeDeletedProduct(cartItemId: any, cartId: any) {
    let url = this.baseurl + 'DeleteToBeDeleted/' + cartItemId + '/' + cartId;
    return this.http.delete(url);
  }
  // JWT Helper Service : npm install @auth0/angular-jwt
  returnProductDeletion(cartItemId: any, cartId: any) {
    let url = this.baseurl + 'DeleteCartItem/' + cartItemId + '/' + cartId;
    return this.http.delete(url);
  }
  getAllPreviousCarts(userid: number) {
    let url = this.baseurl + 'GetAllPreviousCartsOfUser/' + userid;
    return this.http.get(url);
  }

  getPaymentMethods() {
    let url = this.baseurl + 'GetPaymentMethods';
    return this.http.get<PaymentMethod[]>(url);
  }

  insertPayment(payment: Payment) {
    return this.http.post(this.baseurl + 'InsertPayment', payment, {
      responseType: 'text',
    });
  }

  insertOrder(order: Order) {
    return this.http.post(this.baseurl + 'InsertOrder', order);
  }
}
