import { Component, OnInit } from "@angular/core";

import { AuthService } from "app/auth/auth.service";
import { User } from "../models/User";

@Component({
  selector: "bounty",
  templateUrl: "./bounty.component.html",
  styleUrls: ["./bounty.component.css"]
})
export class BountyComponent implements OnInit {

    constructor(private auth: AuthService) { }

    user: User;

    ngOnInit() {
        this.auth.getUser().subscribe(u => this.user = u);
    }

}
