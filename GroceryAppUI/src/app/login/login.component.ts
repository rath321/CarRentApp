import { Component, ComponentRef, OnInit, ViewChild } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';
import { NavigationService } from '../services/navigation.service';
import { UtilityService } from '../services/utility.service';
import { Router } from '@angular/router';
import { ModalService } from '../services/modal.service';
import { HeaderComponent } from '../header/header.component';
import { User } from '../models/models';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  @ViewChild('child') headerComponent!: HeaderComponent;

  message = '';
  constructor(
    private fb: FormBuilder,
    private router: Router,
    private navigationService: NavigationService,
    private utilityService: UtilityService,
    private modalService: ModalService
  ) {}
  private modalComponentRef: any;
  ngOnInit(): void {
    this.loginForm = this.fb.group({
      UserName: ['', [Validators.required, Validators.maxLength(25)]],
      email: ['', [Validators.required, Validators.email]],
      pwd: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(15),
        ],
      ],
    });
  }

  login() {
    let tmp: any;
    this.navigationService
      .loginUser(this.Email.value, this.PWD.value)
      .subscribe(
        (res: any) => {
          console.log(res);

          this.navigationService
            .loginUserEF(this.UserName.value, this.PWD.value)
            .subscribe(
              (response: any) => {
                console.log(response.token);
                if (response.token.toString() !== 'invalid') {
                  this.message = 'Logged In Successfully.';
                  this.utilityService.setUser(
                    res,
                    response.token.toString(),
                    response.roles
                  );

                  this.router.navigate(['']);
                } else {
                  this.message = 'Invalid Credentials!';
                }
              },
              (err) => {
                this.message = 'Invalid Credentials!';
              }
            );
        },
        (err) => {
          this.message = 'Invalid Credentials!';
        }
      );
  }
  closeModal() {
    if (this.modalComponentRef) {
      this.modalComponentRef.destroy();
      this.modalComponentRef = null;
    }
  }
  get Email(): FormControl {
    return this.loginForm.get('email') as FormControl;
  }
  get PWD(): FormControl {
    return this.loginForm.get('pwd') as FormControl;
  }
  get UserName(): FormControl {
    return this.loginForm.get('UserName') as FormControl;
  }
}
