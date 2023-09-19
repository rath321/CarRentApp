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
  url = `https://localhost:7255/api/Shopping/GetAllUsers`;
  constructor(
    private navigationService: NavigationService,
    public utilityService: UtilityService,
    private http: HttpClient,
    private router: Router
  ) {}
  data: any[] = [];
  updateItems: any;
  activeCartArray: any[] = [];
  isActive: boolean = true;
  isLoading: boolean = false;
  ngOnInit(): void {
    let authToken = sessionStorage.getItem('user');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
      Authorization: `Bearer ${authToken}`,
    });

    this.data = [];
    this.activeCartArray = [];
    if (this.isActive) {
      this.http.get(this.url, { headers: headers }).subscribe((res: any) => {
        for (let i = 0; i < res.length; i++) {
          // Your code here
          let tmp: Object[] = [];
          let activeArrayTmp: any[] = [];
          tmp.push(res[i].id);
          tmp.push(res[i].firstName);

          this.navigationService
            .getActiveCartOfUser(res[i].id)
            .subscribe((data: any) => {
              tmp.push(data.cartItems);
              for (var j = 0; j < data?.cartItems?.length; j++) {
                activeArrayTmp.push({
                  quantity: data.cartItems[j].duration,
                  id: data.cartItems[j].product.id,
                });
              }
            });

          this.activeCartArray.push(activeArrayTmp);

          this.data.push(tmp);
        }
      });
    }
  }
  buttonClass1: string = 'btn btn-primary';
  buttonClass2: string = 'btn btn-secondary';
  fn2() {
    let authToken = sessionStorage.getItem('user');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
      Authorization: `Bearer ${authToken}`,
    });

    this.data = [];
    this.activeCartArray = [];
    this.http.get(this.url, { headers: headers }).subscribe((res: any) => {
      for (let i = 0; i < res.length; i++) {
        // Your code here
        let tmp: Object[] = [];
        let activeArrayTmp: any[] = [];
        tmp.push(res[i].id);
        tmp.push(res[i].firstName);

        this.navigationService
          .getActiveCartOfUser(res[i].id)
          .subscribe((data: any) => {
            tmp.push(data.cartItems);
            for (var j = 0; j < data?.cartItems?.length; j++) {
              activeArrayTmp.push({
                quantity: data.cartItems[j].duration,
                id: data.cartItems[j].product.id,
              });
            }
          });

        this.activeCartArray.push(activeArrayTmp);

        this.data.push(tmp);
      }
    });
  }
  fn1() {
    this.data = [];
    let authToken = sessionStorage.getItem('user');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
      Authorization: `Bearer ${authToken}`,
    });
    this.http.get(this.url, { headers: headers }).subscribe((res: any) => {
      for (let i = 0; i < res.length; i++) {
        // Your code here
        let tmp: Object[] = [];
        let activeArrayTmp: any[] = [];
        tmp.push(res[i].id);
        tmp.push(res[i].firstName);

        this.navigationService
          .getAllPreviousCarts(res[i].id)
          .subscribe((data) => {
            tmp.push(data);
          });

        this.data.push(tmp);
      }
    });
  }
  negate(e: any) {
    this.isActive = !this.isActive;
    if (this.isActive == false) {
      this.fn1();
    } else {
      this.fn2();
    }
    // console.log(this.buttonClass1, this.isActive);
    // console.log(this.buttonClass2, this.isActive);
    this.buttonClass1 = !this.isActive
      ? 'btn btn-secondary'
      : 'btn btn-primary';
    this.buttonClass2 = this.isActive ? 'btn btn-secondary' : 'btn btn-primary';

    // this.buttonClass1 = this.isActive
    //   ? 'btn btn-secondary'
    //   : 'btn btn-primary';
    // this.buttonClass2 = !this.isActive
    //   ? 'btn btn-secondary'
    //   : 'btn btn-primary';
  }
  deleteProduct(cartItemId: any, cartId: any, id: any) {
    let tmp: Product;
    this.navigationService
      .returnProductDeletion(cartItemId, cartId)
      .subscribe((res) => {
        this.navigationService.getProduct(id).subscribe((res: any) => {
          tmp = res;
          tmp.quantity += 1;
          this.updateProduct(id, tmp).subscribe((res) => {
            // this.navigationService
            //   .deleteReturnToBeDeletedProduct(cartItemId, cartId)
            //   .subscribe((res) => {
            //     console.log(res);
            //   });
            this.refreshPage();
          });
        });
      });
  }
  updateProduct(id: number, product: Product) {
    const url = `https://localhost:7255/api/Shopping/UpdateProduct/${id}`;
    let authToken = sessionStorage.getItem('user');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
      Authorization: `Bearer ${authToken}`,
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
    const url = `https://localhost:7255/api/Shopping/UpdateActiveCartOfUser/${userId}`;
    let authToken = sessionStorage.getItem('user');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
      Authorization: `Bearer ${authToken}`,
    });

    const options = { headers: headers };

    return this.http.put(url, updatedCartItems, { headers: headers });
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
