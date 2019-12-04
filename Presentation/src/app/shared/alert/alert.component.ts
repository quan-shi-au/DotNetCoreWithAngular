import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { AlertService } from '../../core/services/alert.service';

@Component({
    moduleId: module.id.toString(),
    selector: 'alert',
    templateUrl: 'alert.component.html',
    styleUrls: ['alert.css'],
})

export class AlertComponent implements OnDestroy, OnInit {
    message: any;
    subscription: Subscription;

    constructor(private alertService: AlertService) { }

    ngOnInit() {
        this.subscription = this.alertService.getMessage().subscribe(message => { this.message = message; });
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }
}