import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DraftResult } from '../admin.service';

@Component({
  selector: 'app-draft-result',
  templateUrl: './draft-result.component.html',
  styleUrls: ['./draft-result.component.css']
})
export class DraftResultComponent implements OnInit {

  constructor(
    @Inject(MAT_DIALOG_DATA) public dataSource: DraftResult) { }

  ngOnInit(): void {
  }

}
