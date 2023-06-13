import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CartComponent } from './cart/cart.component';
import { HomeComponent } from './home/home.component';
import { OrderComponent } from './order/order.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { ProductDetailsComponent } from './product-details/product-details.component';
import { ProductsComponent } from './products/products.component';
import { AccountComponent } from './account/account.component';
import { AuthGuard } from './services/auth.guard';
import { CreateProductComponent } from './create-product/create-product.component';
import { UtilityService } from './services/utility.service';
import { HeaderComponent } from './header/header.component';

const routes: Routes = [
  { path: 'home', component: ProductsComponent },
  { path: 'update-product/:id', component: CreateProductComponent , canActivate:[AuthGuard]},
  { path: 'home/update-product/:id', component: CreateProductComponent , canActivate:[AuthGuard]},
  { path: 'products/update-product/:id', component: CreateProductComponent , canActivate:[AuthGuard]},
  {path:'account', component:AccountComponent, canActivate:[AuthGuard]},
  { path: 'products', component: ProductsComponent },
  { path: 'product-details', component: ProductDetailsComponent },
  {path:'header', component:HeaderComponent},
  { path: 'cart', component: CartComponent, canActivate:[AuthGuard]},
  { path: 'orders', component: OrderComponent, canActivate:[AuthGuard] },
  { path: 'create-product', component: CreateProductComponent, canActivate:[AuthGuard] },
  { path: '', component:ProductsComponent },
  { path: '**', component: PageNotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
