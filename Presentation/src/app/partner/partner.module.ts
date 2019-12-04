import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AlertModule } from '../shared/alert/alert.module';
import { PaginationModule } from '../shared/pagination/pagination.module';
import { ModalModule } from '../shared/simplemodal/modal.module';
import { ModalheaderModule } from '../shared/modalheader/modalheader.module';

import { PartnerListComponent } from './partnerlist/partner-list.component';

import { PartnerRoutes } from './partner.routing';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(PartnerRoutes),
        FormsModule,
        AlertModule,
        PaginationModule,
        ModalModule,
        ModalheaderModule
    ],
    declarations: [
        PartnerListComponent,
    ]
})

export class PartnerModule {}
