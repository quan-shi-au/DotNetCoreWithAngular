import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { UserRoutes } from './user.routing';
import { PaginationModule } from '../shared/pagination/pagination.module';
import { ModalModule } from '../shared/simplemodal/modal.module';
import { AlertModule } from '../shared/alert/alert.module';

import { UserListComponent } from './userlist/user-list.component';
import { ModalheaderModule } from '../shared/modalheader/modalheader.module';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(UserRoutes),
        FormsModule,
        ReactiveFormsModule,
        PaginationModule,
        ModalModule,
        AlertModule,
        ModalheaderModule
    ],
    declarations: [
        UserListComponent
    ],
})

export class UserModule {}
