import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AllAgreementsComponent } from './all-agreements.component';

describe('AllAgreementsComponent', () => {
  let component: AllAgreementsComponent;
  let fixture: ComponentFixture<AllAgreementsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AllAgreementsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AllAgreementsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
