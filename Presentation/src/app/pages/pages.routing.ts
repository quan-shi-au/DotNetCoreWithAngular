import { Routes } from '@angular/router';

import { LoginComponent } from './login/login.component';
import { ResetPasswordComponent } from './resetPassword/resetPassword.component';
import { ChangePasswordComponent } from './changePassword/changePassword.component';
import { EmailVerifyComponent } from './emailVerify/emailVerify.component';

export const PagesRoutes: Routes = [{
    path: '',
    children: [ {
        path: 'login',
        component: LoginComponent
    }, {
        path: 'resetpassword',
        component: ResetPasswordComponent
    }, {
        path: 'changepassword',
        component: ChangePasswordComponent
    }, {
        path: 'emailverify',
        component: EmailVerifyComponent
    }]
}];
