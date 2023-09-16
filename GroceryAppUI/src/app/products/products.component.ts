import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Category, NavigationItem, Product } from '../models/models';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css'],
})
export class ProductsComponent implements OnInit {
  view: 'grid' | 'list' = 'list';
  sortby: 'default' | 'htl' | 'lth' = 'default';
  products: Product[] = [];
  p: number = 1;
  items: any[] = [
    { id: 1, name: 'Item 1' },
    { id: 2, name: 'Item 2' },
    { id: 3, name: 'Item 3' },
    // Add more items
  ];
  searchQuery: string = '';

  filteredItems!: any[];

  applyFilter() {
    this.filteredItems = this.products.filter((item) =>
      item.title.toLowerCase().includes(this.searchQuery.toLowerCase())
    );
  }
  navigationList: NavigationItem[] = [];
  // Pagination properties
  pageSize: number = 10;
  constructor(
    private activatedRoute: ActivatedRoute,
    private navigationService: NavigationService,
    public utilityService: UtilityService,
    private http: HttpClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.navigationService.getProductsAll().subscribe((data) => {
      this.filteredItems = data;
      this.products = data;
    });
    this.navigationService.getCategoryList().subscribe((list: Category[]) => {
      // this.navigationList=list;
      for (let item of list) {
        let present = false;

        for (let navItem of this.navigationList) {
          if (navItem.category === item.category) {
            navItem.subcategories.push(item.subCategory);
            present = true;
          }
        }
        if (!present) {
          this.navigationList.push({
            category: item.category,
            subcategories: [item.subCategory],
          });
        }
      }
    });

    this.activatedRoute.queryParams.subscribe((params: any) => {
      let category = params.category;
      let subcategory = params.subcategory;

      if (category != 'MAKER' && category != 'MODEL') {
        this.navigationService.getProductsAll().subscribe((data) => {
          if (subcategory == '<1000') {
            this.filteredItems = data.filter((items) => items.price < 1000);
          } else if (subcategory == '>1000 && <5000') {
            this.filteredItems = data.filter(
              (items) => items.price > 1000 && items.price < 5000
            );
          } else if (subcategory == '>5000') {
            this.filteredItems = data.filter((items) => items.price > 5000);
          }
        });
      } else {
        if (category && subcategory)
          this.navigationService
            .getProducts(category, subcategory, 10)
            .subscribe((res: any) => {
              this.products = res;
              // console.log(this.products)
              this.filteredItems = res;
            });
      }
    });
  }
  getAllProducts() {
    this.navigationService.getProductsAll().subscribe((data) => {
      this.filteredItems = data;
      this.products = data;
      const newUrl = ''; // Replace 'new-url' with the desired URL
      this.router.navigateByUrl(newUrl);
    });
  }
  sortByPrice(sortKey: string) {
    this.products.sort((a, b) => {
      if (sortKey === 'default') {
        return a.id > b.id ? 1 : -1;
      }
      return (
        (sortKey === 'htl' ? 1 : -1) *
        (this.utilityService.applyDiscount(a.price, a.offer.discount) >
        this.utilityService.applyDiscount(b.price, b.offer.discount)
          ? -1
          : 1)
      );
    });
  }
  deleteProduct(id: number) {
    const url = `https://localhost:7255/api/Shopping/DeleteProduct/${id}`;

    this.http.delete(url).subscribe(
      () => {
        // Delete request successful
        console.log('Product deleted successfully.');
        this.activatedRoute.queryParams.subscribe((params: any) => {
          let category = params.category;
          let subcategory = params.subcategory;

          if (category && subcategory)
            this.navigationService
              .getProducts(category, subcategory, 10)
              .subscribe((res: any) => {
                this.products = res;
                this.filteredItems = res;
              });
        });
      },
      (error) => {
        // Error occurred
        console.error('Failed to delete product:', error);
      }
    );
  }
}
