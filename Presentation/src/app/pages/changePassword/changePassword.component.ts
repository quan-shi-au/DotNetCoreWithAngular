import { Component, OnInit, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';


import { AuthenticationService } from '../../core/services/authentication.service';

import { AlertService } from '../../core/services/alert.service'
import { UserService } from '../../core/services/user.service'
import { UserRole } from '../../core/models/userRole'
import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable'
import { ToastrService } from '../../core/services/toastr.service'
import { TranslateService } from '@ngx-translate/core';

declare var $: any;

@Component({
    moduleId: module.id,
    selector: 'change-password-cmp',
    templateUrl: 'changePassword.component.html',
    styleUrls: ['changePassword.css']
})

export class ChangePasswordComponent implements OnInit {

    public userName: string;
    public waitingForResponse: boolean = false;
    public currentDate: Date = new Date();

    public password: string;
    public confirmPassword: string;
    public id: string;
    public token: string;
    public returnUrl: string;
    public isPasswordMatch: boolean = true;

    public isTokenVerified: boolean = false;

    constructor(
        private element: ElementRef,
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService,
        private alertService: AlertService,
        private userService: UserService,
        private toastrService: ToastrService,
        private translateService: TranslateService,
    ) {

    }

    ngOnInit() {

        this.returnUrl = this.route.snapshot.queryParams['returnUrl'];
        this.id = this.route.snapshot.queryParams['id'];
        this.token = this.route.snapshot.queryParams['token'];

        this.checkFullPageBackgroundImage();

    }

    checkFullPageBackgroundImage() {
        var $page = $('.full-page');
        var image_src = $page.data('image');

        if (image_src !== undefined) {
            var image_container = '<div class="full-page-background" style="background-image: url(' + image_src + ') "/>'
            $page.append(image_container);
        }
    };

    blurConfirmPassword() {

        this.isPasswordMatch = this.password === this.confirmPassword;
    }

    changePassword(changePasswordForm) {

        var successMessage = this.translateService.instant("PagesChangePassword.ChangePasswordSuccess");
        var errorMessage = this.translateService.instant("PagesChangePassword.ChangePasswordFailed");

        this.waitingForResponse = true;

        this.userService
            .changePasswordWithToken(this.id, this.token, this.password)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {
                    this.toastrService.success(successMessage);
                    this.router.navigate(['/pages/login']);
                } else {
                    this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error(errorMessage);

                this.waitingForResponse = false;
            }
        );
    }

}
