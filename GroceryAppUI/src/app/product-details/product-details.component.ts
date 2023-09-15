import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Product, Review } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-product-details',
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css'],
})
export class ProductDetailsComponent implements OnInit {
  imageIndex: number = 1;
  product!: Product;
  reviewControl = new FormControl('');
  showError = false;
  reviewSaved = false;
  otherReviews: Review[] = [];
  quantityExceeded: boolean = false;
  duration: number = 0;
  constructor(
    private activatedRoute: ActivatedRoute,
    private navigationService: NavigationService,
    public utilityService: UtilityService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe((params: any) => {
      let id = params.id;
      this.navigationService.getProduct(id).subscribe((res: any) => {
        this.product = res;
        this.fetchAllReviews();
      });
    });
  }
  onAddToCartCall(product: Product) {
    // Call the addToCart method, which may not return an observable.
    this.utilityService.addToCart(product, this.duration);
    this.updateProduct(product.id).subscribe((data) => {
      this.activatedRoute.queryParams.subscribe((params: any) => {
        let id = params.id;
        this.navigationService.getProduct(id).subscribe((res: any) => {
          this.product = res;
        });
      });
    });
  }

  updateProduct(id: number) {
    const url = `https://localhost:7013/api/Shopping/UpdateProduct/${id}`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
    });

    const requestBody = {
      id: this.product.id,
      title: this.product.title,
      description: this.product.description,
      productCategory: this.product.productCategory,
      offer: this.product.offer,
      price: this.product.price,
      quantity: this.product.quantity - 1,
      imageName: this.product.imageName,
    };
    this.product.quantity -= 1;

    return this.http.put(url, requestBody, { headers });
  }

  submitReview() {
    let review = this.reviewControl.value;

    if (review === '' || review === null) {
      this.showError = true;
      return;
    }

    let userid = this.utilityService.getUser().id;
    let productid = this.product.id;

    this.navigationService
      .submitReview(userid, productid, review)
      .subscribe((res) => {
        this.reviewSaved = true;
        this.fetchAllReviews();
        this.reviewControl.setValue('');
      });
  }

  fetchAllReviews() {
    this.otherReviews = [];
    this.navigationService
      .getAllReviewsOfProduct(this.product.id)
      .subscribe((res: any) => {
        for (let review of res) {
          this.otherReviews.push(review);
        }
      });
  }
}
