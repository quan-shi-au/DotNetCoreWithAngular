import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AlertModule } from '../shared/alert/alert.module';
import { PaginationModule } from '../shared/pagination/pagination.module';
import { ModalModule } from '../shared/simplemodal/modal.module';
import { ModalheaderModule } from '../shared/modalheader/modalheader.module';

import { AdminReportComponent } from './report/admin-report.component';

import { AdminRoutes } from './admin.routing';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(AdminRoutes),
        FormsModule,
        AlertModule,
        PaginationModule,
        ModalModule,
        ModalheaderModule
    ],
    declarations: [
        AdminReportComponent,
    ]
})

export class AdminModule {}
