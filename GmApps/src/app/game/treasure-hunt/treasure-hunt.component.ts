import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { GameService } from 'app/game/game.service';
import { TreasureHuntStatus, TreasureHuntPuzzle, TreasureHuntAttemptResult } from '../../models/TreasureHuntStatus';
import { Gallery } from './gallery';
import { combineLatest, map, switchMap } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PuzzleDetailComponent, DetailArgs } from './puzzle-detail.component';
import { Subject, interval } from 'rxjs';
import { HintDialogComponent } from './hint-dialog.component';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-treasure-hunt',
  templateUrl: './treasure-hunt.component.html',
  styleUrls: ['./treasure-hunt.component.css']
})
export class TreasureHuntComponent implements OnInit {

    constructor(private gameService: GameService, public dialog: MatDialog,
        private snackbar: MatSnackBar, private userService: AuthService) { }

    status: TreasureHuntStatus
    showStage2 = false;
    hasStage2Initially = false;
    hasStage2 = false;
    @ViewChild("stage1") stage1Canvas: ElementRef<HTMLCanvasElement>
    @ViewChild("stage2") stage2Canvas: ElementRef<HTMLCanvasElement>
    stage1: Gallery
    stage2: Gallery
    loading = false;
    lastResult: TreasureHuntAttemptResult
    dailyAttempts = 0
    maxAttempt = 0
    isAdmin = false;

    ngOnInit() {
        this.loading = true;
        this.gameService.treasureHuntStatus().subscribe(status => {
                this.initStatus(status);
                this.hasStage2Initially = this.hasStage2;
            },
            (err: HttpErrorResponse) => {
                this.loading = false;
                this.snackbar.open(err.error ? `出错了：${err.error}` : err.message, null, { duration: 3000 })
            })
        this.userService.getUser().subscribe(u => this.isAdmin = u.isAdmin())
    }

    private toBjTime(d: Date): Date {
        let utc = d.getTime() + (d.getTimezoneOffset() * 60000);
        return new Date(utc + (3600000 * 8));
    }

    initStatus(status: TreasureHuntStatus) {
        this.status = status;
        this.maxAttempt = status.dailyAttemptLimit;
        let d = new Date();
        let nd = this.toBjTime(d);
        let today = new Date(nd.getFullYear(), nd.getMonth(), nd.getDate());
        this.dailyAttempts = 0;
        for (let s of status.puzzles.filter(p => p.attempts)) {
            for (let d of Object.keys(s.attempts)) {
                let att = this.toBjTime(new Date(d));
                if (att > today && s.attempts[d] != s.answer) {
                    this.dailyAttempts++;
                }
            }
        }

        this.status.puzzles.sort((a, b) => a.position - b.position)
        this.hasStage2 = status.puzzles.length > 4;
        let createImg = (p: TreasureHuntPuzzle) => {
            let img = new Image()
            img.src = p.image
            img.addEventListener("click", () => this.openPuzzle(p.position))
            return img;
        }
        if (!this.stage1) {
            let imgs = status.puzzles.slice(0, 4).map(createImg);
            this.stage1 = new Gallery(this.stage1Canvas.nativeElement, imgs);
        }
        let obs = this.stage1.loaded;
        if (this.hasStage2 && !this.stage2) {
            let imgs = status.puzzles.slice(4).map(createImg);
            this.stage2 = new Gallery(this.stage2Canvas.nativeElement, imgs);
            obs = this.stage1.loaded.pipe(
                combineLatest(this.stage2.loaded),
                map(([a, b]) => a && b)
            );
        }
        obs.subscribe(loaded => {
            this.loading = !loaded
            if (this.showStage2) {
                this.stage2.render()
            } else {
                this.stage1.render()
            }
        }, (err: HttpErrorResponse) => {
            this.loading = false;
            this.snackbar.open(`载入图像出错了，请刷新重试。`, null, { duration: 3000 })
        })
    }

    get completedCount() {
        return this.status.puzzles.filter(f => f.completed).length
    }

    get isTopPlayer() {
        return this.status.topPlayers.find(f => f.userName == this.status.currentPlayer.userName) != undefined
    }

    openPuzzle(idx: number) {
        let arg: DetailArgs = {
            status: this.status,
            index: idx,
            dailyAttempts: this.dailyAttempts,
            resultSubject: new Subject<TreasureHuntAttemptResult>(),
        }
        let dlg = this.dialog.open<PuzzleDetailComponent, DetailArgs, TreasureHuntAttemptResult | null>(PuzzleDetailComponent, { data: arg });
        arg.resultSubject.subscribe(result => {
            this.lastResult = result;
            this.dailyAttempts = result.dailyAttemptCount;
            if (result.isCorrect) {
                this.gameService.treasureHuntStatus().subscribe(status => this.initStatus(status))
            }
        }, null, () => {
            if (!this.hasStage2Initially && this.hasStage2) {
                this.hasStage2Initially = true;
                this.snackbar.open("新的画廊开放了！", null, { duration: 3000 })
                this.stage1Canvas.nativeElement.classList.add('fade');
                setTimeout(() => {
                    this.stage1.unload()
                    delete this.stage1;
                    this.initStatus(this.status);
                    this.stage1.loaded.subscribe(a => {
                        if (a) {
                            this.stage1Canvas.nativeElement.classList.remove('fade');
                        }
                    })
                }, 500);
            }
        })
    }

    showHint() {
        this.dialog.open(HintDialogComponent)
    }

    transitStage(showStage2: boolean) {
        let fade = (e1: HTMLElement, e2: HTMLElement) => {
            e1.classList.add('fade');
            e2.classList.add('fade');
            setTimeout(() => {
                this.showStage2 = showStage2;
                e1.classList.remove('fade');
                e2.classList.remove('fade');
                setTimeout(() => {
                    if (this.showStage2) {
                        this.stage2.render()
                    } else {
                        this.stage1.render()
                    }
                }, 4)
            }, 500);
        }
        if (showStage2) {
            fade(this.stage1Canvas.nativeElement, this.stage2Canvas.nativeElement)
        } else {
            fade(this.stage2Canvas.nativeElement, this.stage1Canvas.nativeElement)
        }
    }
}
