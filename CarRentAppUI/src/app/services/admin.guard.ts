import { ObserversModule } from "@angular/cdk/observers";
import { ActivatedRoute, ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { Observable } from "rxjs/internal/Observable";
import { Injectable } from "@angular/core";
import { UtilityService } from "./utility.service";
@Injectable()
export class AdminGuard implements CanActivate{
    constructor(private utilityService:UtilityService, private router: Router) {}

    canActivate(route:ActivatedRouteSnapshot, state:RouterStateSnapshot):boolean| Observable <boolean> | Promise<boolean>{
        const isAuth = (this.utilityService.isLoggedIn() && this.utilityService.isAdmin());
    if (!isAuth) {
      this.router.navigate(['/home']);
    }
    return isAuth;
    }
}