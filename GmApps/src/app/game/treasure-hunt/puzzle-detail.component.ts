import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TreasureHuntStatus, TreasureHuntPuzzle, TreasureHuntAttemptResult } from '../../models/TreasureHuntStatus';
import { GameService } from '../game.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Subject } from 'rxjs';

export interface DetailArgs {
    status: TreasureHuntStatus;
    index: number;
    dailyAttempts: number;
    resultSubject: Subject<TreasureHuntAttemptResult>;
}

@Component({
  selector: 'app-puzzle-detail',
  templateUrl: './puzzle-detail.component.html',
  styleUrls: ['./puzzle-detail.component.css']
})
export class PuzzleDetailComponent implements OnInit {

    puzzle: TreasureHuntPuzzle;
    letter: string
    digits: number
    dailyAttempts = 0
    maxAttempt = 0
    isCorrect?: boolean
    reward?: number
    rank?: number
    submitting = false;
    result: TreasureHuntAttemptResult
    resultSubject: Subject<TreasureHuntAttemptResult>

    constructor(
        private service: GameService,
        private snackbar: MatSnackBar,
        dialogRef: MatDialogRef<PuzzleDetailComponent>,
        @Inject(MAT_DIALOG_DATA) arg: DetailArgs
    ) {
        this.maxAttempt = arg.status.dailyAttemptLimit;
        this.puzzle = arg.status.puzzles.find(t => t.position == arg.index);
        if (this.puzzle.answer) {
            this.letter = this.puzzle.answer[0]
            this.digits = Number(this.puzzle.answer.substring(1))
        }
        this.dailyAttempts = arg.dailyAttempts;
        this.resultSubject = arg.resultSubject;
        dialogRef.afterClosed().subscribe(_ => this.resultSubject.complete());
    }

    ngOnInit() {
    }

    get exceedLimit() {
        return this.dailyAttempts >= this.maxAttempt
    }

    get shouldDisable() {
        return this.puzzle.completed || this.exceedLimit || this.submitting;
    }

    get attempts() {
        if (!this.puzzle.attempts) {
            return []
        }
        return Object.keys(this.puzzle.attempts).map(k => { return { date: new Date(k), ans: this.puzzle.attempts[k] }})
    }

    submit() {
        if (!this.letter) {
            return;
        }
        if (!this.digits) {
            return;
        }
        this.submitting = true;
        let ans = this.letter + this.digits.toString();
        this.service.treasureHuntAttempt(this.puzzle.position, ans)
            .subscribe(r => {
                this.submitting = false;
                this.result = r;
                this.isCorrect = r.isCorrect;
                this.puzzle.completed = this.isCorrect;
                this.puzzle.attempts[new Date().toJSON()] = ans;
                if (r.isCorrect) {
                    this.rank = r.rank
                    this.reward = r.reward
                }
                this.dailyAttempts = r.dailyAttemptCount
                this.resultSubject.next(r);
            }, (e: HttpErrorResponse) => {
                this.submitting = false;
                this.snackbar.open(e.error.error ? `出错了：${e.error.error}` : e.message, null, { duration: 3000 })
            });
    }

}
