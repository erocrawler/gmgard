import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessageIndexComponent } from './message-index.component';
import { RouterModule, Routes } from '@angular/router';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MessageWriteComponent } from './message-write.component';
import { MessageOutboxComponent } from './message-outbox.component';
import { MessageInboxComponent } from './message-inbox.component';
import { AppMaterialModule } from '../app-material.module';
import { PaginationModule } from '../shared/pagination';
import { MessageService } from './message.service';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MessageDetailsComponent } from './message-details.component';

const routes: Routes = [
  {
    path: "",
    component: MessageIndexComponent,
    children: [
      { path: "", redirectTo: "/message/inbox", pathMatch: "full" },
      { path: "inbox", component: MessageInboxComponent },
      { path: "outbox", component: MessageOutboxComponent },
      { path: "write", component: MessageWriteComponent },
    ]
  },
];

@NgModule({
  declarations: [MessageIndexComponent, MessageWriteComponent, MessageOutboxComponent, MessageInboxComponent, MessageDetailsComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    PaginationModule,
    AppMaterialModule,
    FlexLayoutModule,
    MatAutocompleteModule,
  ],
  providers: [MessageService],
})
export class MessageModule { }
