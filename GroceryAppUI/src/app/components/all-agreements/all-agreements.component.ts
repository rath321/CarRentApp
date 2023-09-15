import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Product } from 'src/app/models/models';
import { NavigationService } from 'src/app/services/navigation.service';
import { UtilityService } from 'src/app/services/utility.service';

@Component({
  selector: 'app-all-agreements',
  templateUrl: './all-agreements.component.html',
  styleUrls: ['./all-agreements.component.css'],
})
export class AllAgreementsComponent implements OnInit {
  url = `https://localhost:7013/api/Shopping/GetAllUsers`;
  constructor(
    private navigationService: NavigationService,
    public utilityService: UtilityService,
    private http: HttpClient,
    private router: Router
  ) {}
  data: any = [];
  updateItems: any;
  activeCartArray: any[] = [];
  ngOnInit(): void {
    this.http.get(this.url).subscribe((res: any) => {
      for (let i = 0; i < res.length; i++) {
        // Your code here
        let tmp = [];
        let activeArrayTmp: any[] = [];
        tmp.push(res[i].id);
        tmp.push(res[i].firstName);
        this.navigationService
          .getActiveCartOfUser(res[i].id)
          .subscribe((data: any) => {
            tmp.push(data.cartItems);
            for (var j = 0; j < data.cartItems.length; j++) {
              activeArrayTmp.push({
                quantity: data.cartItems[j].duration,
                id: data.cartItems[j].product.id,
              });
            }
          });
        this.navigationService
          .getAllPreviousCarts(res[i].id)
          .subscribe((data) => {
            tmp.push(data);
          });
        this.activeCartArray.push(activeArrayTmp);
        this.data.push(tmp);
      }
    });
  }
  deleteProduct(cartItemId: any, cartId: any, id: any) {
    let tmp: Product;
    this.navigationService
      .returnProductDeletion(cartItemId, cartId)
      .subscribe((res) => {
        this.navigationService.getProduct(id).subscribe((res: any) => {
          tmp = res;
          console.log(res);
          console.log(res.quantity);
          tmp.quantity += 1;
          console.log(tmp.quantity);
          this.updateProduct(id, tmp).subscribe((res) => {
            console.log(res);
            this.navigationService
              .deleteReturnToBeDeletedProduct(cartItemId, cartId)
              .subscribe((res) => {
                console.log(res);
              });
            this.refreshPage();
          });
        });
      });
  }
  updateProduct(id: number, product: Product) {
    const url = `https://localhost:7013/api/Shopping/UpdateProduct/${id}`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
    });
    return this.http.put(url, product, { headers });
  }
  onPlus(idx: any, idy: any) {
    this.activeCartArray[idx][idy].quantity += 1;
    this.data[idx][2][idy].duration += 1;
    // console.log(this.usersCart.cartItems);
  }
  onMinus(idx: any, idy: any, arrayid: any) {
    this.activeCartArray[idx][idy].quantity -= 1;
    this.data[idx][2][idy].duration -= 1;
    // console.log(this.updateItems);
  }
  onUpdate(userid: any, ind: any) {
    this.updateItems = this.data[ind][2];

    this.updateActiveCartOfUser(userid, this.updateItems).subscribe((res) => {
      this.refreshPage();
    });
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
  refreshPage() {
    // Get the current URL
    const currentUrl = this.router.url;

    // Navigate to the same URL to refresh the page
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
  }
}
