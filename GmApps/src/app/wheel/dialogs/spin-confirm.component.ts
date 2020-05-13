import { Component, OnInit, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable } from 'rxjs';

export interface SpinConfirmArg {
  wheelType: 'a' | 'b';
  wheelCost: number;
  points: Observable<number>;
  remainSpin?: number;
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
    this.isLimit = data.remainSpin === 0;
    this.remainSpin = data.remainSpin;
    data.points.subscribe(p => this.points = p);
  }

  ngOnInit() {
  }

  remainSpin: number;
  isLimit = false;
  wheelType: string
  cost = 0
  points = 0

}
