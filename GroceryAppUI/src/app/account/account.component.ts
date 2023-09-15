import { Component, OnInit } from '@angular/core';
import { UtilityService } from '../services/utility.service';
import { NavigationService } from '../services/navigation.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css'],
})
export class AccountComponent implements OnInit {
  userName: any;
  constructor(
    public utilityService: UtilityService,
    private navigationService: NavigationService
  ) {}
  toBeDeleted: any = [];
  ngOnInit(): void {
    this.userName = this.utilityService.getUser().firstName;
    this.navigationService.returnToBeDeletedProduct().subscribe((res) => {
      this.toBeDeleted = res;
    });
  }
}
