"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var login_component_1 = require("./login/login.component");
var resetPassword_component_1 = require("./resetPassword/resetPassword.component");
var changePassword_component_1 = require("./changePassword/changePassword.component");
var emailVerify_component_1 = require("./emailVerify/emailVerify.component");
exports.PagesRoutes = [{
        path: '',
        children: [{
                path: 'login',
                component: login_component_1.LoginComponent
            }, {
                path: 'resetpassword',
                component: resetPassword_component_1.ResetPasswordComponent
            }, {
                path: 'changepassword',
                component: changePassword_component_1.ChangePasswordComponent
            }, {
                path: 'emailverify',
                component: emailVerify_component_1.EmailVerifyComponent
            }]
    }];
//# sourceMappingURL=pages.routing.js.map