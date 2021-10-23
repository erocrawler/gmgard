import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessageIndexComponent } from './message-index.component';

describe('MessageIndexComponent', () => {
  let component: MessageIndexComponent;
  let fixture: ComponentFixture<MessageIndexComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MessageIndexComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MessageIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
