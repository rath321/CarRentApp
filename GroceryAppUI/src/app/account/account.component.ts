import { Component, OnInit } from '@angular/core';
import { UtilityService } from '../services/utility.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css'],
})
export class AccountComponent implements OnInit {
  userName: any;
  constructor(public utilityService: UtilityService) {}

  ngOnInit(): void {
    this.userName = this.utilityService.getUser().firstName;
  }
}
