import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { NavigationService } from '../services/navigation.service';
import { ImageService } from '../services/image.service';

@Component({
  selector: 'app-create-product',
  templateUrl: './create-product.component.html',
  styleUrls: ['./create-product.component.css'],
})
export class CreateProductComponent implements OnInit {
  mode = 'create';
  routeId!: any;
  imageUrl: string | undefined;
  productForm!: FormGroup;
  categories: any[] = [
    { id: 1, category: 'MAKER', subCategory: 'MAKER-A' },
    { id: 9, category: 'MAKER', subCategory: 'MAKER-C' },
    { id: 11, category: 'MODEL', subCategory: 'MODEL-A' },
    { id: 4, category: 'MAKER', subCategory: 'MAKER-B' },
    { id: 13, category: 'MODEL', subCategory: 'MODEL-C' },
    { id: 12, category: 'MODEL', subCategory: 'MODEL-B' },
  ];
  offers: any[] = [
    { id: 1, title: 'MODEL-A', discount: 10 },
    { id: 2, title: 'MODEL-B', discount: 20 },
    { id: 3, title: 'MODEL-C', discount: 30 },
    { id: 4, title: 'MODEL-D', discount: 40 },
  ];
  imageForm: any;
  selectedFile!: File;

  constructor(
    public imageService: ImageService,
    private router: Router,
    public formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private navigationService: NavigationService,
    private http: HttpClient,
    private changeDetector: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.productForm = this.formBuilder.group({
      id: [0],
      title: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.maxLength(255)]],
      category: ['', [Validators.required, Validators.maxLength(100)]],
      offer: ['', Validators.required],

      price: [0, Validators.required],
      quantity: [0, Validators.required],
      imageName: ['', [Validators.required]],
    });
    this.route.paramMap.subscribe((paramMap: ParamMap) => {
      if (paramMap.has('id')) {
        this.mode = 'edit';
        this.routeId = paramMap.get('id');
        this.navigationService
          .getProduct(this.routeId)
          .subscribe((res: any) => {
            this.productForm.patchValue({
              title: res.title,
              description: res.description,
              price: res.price,
              quantity: res.quantity,
              imageName: res.imageName,
              category: res.productCategory.id,
              offer: res.offer.id,
            });
          });
      } else {
        this.mode = 'create';
        this.routeId = null;
      }
    });
  }
  loading = false;
  onFileSelected(event: any) {
    const file: File = event.target.files[0];

    if (file) {
      this.imageService.uploadImage(file).subscribe(
        async (url) => {
          // console.log(url)
          this.loading = true;
          setTimeout(() => {
            // Handle the download URL after the delay
            this.loading = false;
            this.imageUrl = this.imageService.imageUrl;

            this.productForm.patchValue({
              imageName: this.imageUrl,
            });
            // Display the image using the download URL
            // ...
          }, 8000);
        },
        (error: any) => {
          console.error('Image upload error:', error);
        }
      );
    }
  }
  tmp() {
    console.log(this.imageService.imageUrl);
  }
  onSubmit(form: any) {
    if (this.mode === 'create') {
      let productData = {
        id: 0,
        title: form.value.title,
        description: form.value.description,
        productCategory: this.categories.find(
          (item) => item.id == form.value.category
        ),
        offer: this.offers.find((item) => item.id == form.value.offer),
        price: form.value.price,
        quantity: form.value.quantity,
        imageName: form.value.imageName,
      };
      const requestBody = {
        id: 0,
        title: 'samsung',
        description: 'string',
        productCategory: {
          id: 1,
          category: 'string',
          subCategory: 'string',
        },
        offer: {
          id: 1,
          title: 'string',
          discount: 0,
        },
        price: 0,
        quantity: 0,
        imageName: 'string',
      };
      const url = 'https://localhost:7255/api/Shopping/CreateProduct';
      const headers = new HttpHeaders({
        'Content-Type': 'application/json',
        accept: '*/*',
      });

      this.http.post(url, productData, { headers }).subscribe(
        (response) => {
          console.log('Product created successfully', response);
        },
        (error) => {
          console.error('Error creating product', error);
        }
      );
    } else {
      this.updateProduct(this.routeId).subscribe((data) => {
        // console.log(data)
      });
    }
    // console.log(this.createProduct())
    this.router.navigate(['']);
  }

  updateProduct(id: number) {
    const url = `https://localhost:7255/api/Shopping/UpdateProduct/${id}`;
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      accept: '*/*',
    });

    const requestBody = {
      id: 0,
      title: this.productForm.value.title,
      description: this.productForm.value.description,
      productCategory: this.categories.find(
        (item) => item.id == this.productForm.value.category
      ),
      offer: this.offers.find(
        (item) => item.id == this.productForm.value.offer
      ),
      price: this.productForm.value.price,
      quantity: this.productForm.value.quantity,
      imageName: this.productForm.value.imageName,
    };

    return this.http.put(url, requestBody, { headers });
  }
}
