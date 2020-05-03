import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable } from 'rxjs';

export interface SpinConfirmArg {
  wheelType: 'a' | 'b';
  wheelCost: number;
  points: Observable<number>;
  isLimit: boolean
}

@Component({
  selector: 'app-spin-confirm',
  templateUrl: './spin-confirm.component.html',
  styleUrls: ['./spin-confirm.component.css']
})
export class SpinConfirmComponent implements OnInit {

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SpinConfirmArg) {
    this.wheelType = data.wheelType;
    this.cost = data.wheelCost;
    this.isLimit = data.isLimit;
    data.points.subscribe(p => this.points = p);
  }

  ngOnInit() {
  }

  isLimit = false;
  wheelType: string
  cost = 0
  points = 0

}
