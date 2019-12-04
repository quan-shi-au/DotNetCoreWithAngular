import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { PagesRoutes } from './pages.routing';

import { LoginComponent } from './login/login.component';
import { ResetPasswordComponent } from './resetPassword/resetPassword.component';
import { ChangePasswordComponent } from './changePassword/changePassword.component';
import { EmailVerifyComponent } from './emailVerify/emailVerify.component';


import { AlertComponent } from '../shared/alert/alert.component';
import { AlertModule } from '../shared/alert/alert.module';
import { FooterModule } from '../shared/footer/footer.module';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(PagesRoutes),
        FormsModule,
        ReactiveFormsModule,
        AlertModule,
        FooterModule,
        TranslateModule,
    ],
    declarations: [
        LoginComponent,
        ResetPasswordComponent,
        ChangePasswordComponent,
        EmailVerifyComponent
    ]

})

export class PagesModule {}
