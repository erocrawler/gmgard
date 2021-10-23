import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MessageOutboxComponent } from './message-outbox.component';

describe('MessageOutboxComponent', () => {
  let component: MessageOutboxComponent;
  let fixture: ComponentFixture<MessageOutboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MessageOutboxComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MessageOutboxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
