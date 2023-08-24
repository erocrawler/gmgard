import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Inventory } from '../../models/GameScenario';

@Component({
  selector: 'app-item-list',
  templateUrl: './item-list.component.html',
  styleUrls: ['./item-list.component.css']
})
export class ItemListComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: Inventory[]) { }


  ngOnInit(): void {
  }

}
