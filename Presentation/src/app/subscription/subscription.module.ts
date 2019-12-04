import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { PaginationModule } from '../shared/pagination/pagination.module';
import { ModalModule } from '../shared/simplemodal/modal.module';
import { AlertModule } from '../shared/alert/alert.module';

import { SubscriptionListComponent } from './list/subscription-list.component';

import { SubscriptionRoutes } from './subscription.routing';
import { ModalheaderModule } from '../shared/modalheader/modalheader.module';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(SubscriptionRoutes),
        FormsModule,
        PaginationModule,
        ModalModule,
        AlertModule,
        ModalheaderModule
    ],
    declarations: [
        SubscriptionListComponent,
    ]
})

export class SubscriptionModule {}
