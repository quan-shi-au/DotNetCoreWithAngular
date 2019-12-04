import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginationModule } from '../shared/pagination/pagination.module';
import { ModalModule } from '../shared/simplemodal/modal.module';
import { ModalheaderModule } from '../shared/modalheader/modalheader.module';

import { EnterpriseListComponent } from './list/enterprise-list.component';
import { AlertModule } from '../shared/alert/alert.module';

import { EnterpriseRoutes } from './enterprise.routing';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(EnterpriseRoutes),
        FormsModule,
        PaginationModule,
        ModalModule,
        AlertModule,
        ModalheaderModule,

    ],
    declarations: [
        EnterpriseListComponent,
    ]
})

export class EnterpriseModule {}
