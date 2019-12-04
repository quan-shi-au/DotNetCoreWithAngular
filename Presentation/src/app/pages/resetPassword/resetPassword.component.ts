import { Component, OnInit, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable';

import { AuthenticationService } from '../../core/services/authentication.service';

import { AlertService } from '../../core/services/alert.service'
import { UserService } from '../../core/services/user.service'
import { UserRole } from '../../core/models/userRole'
import { ToastrService } from '../../core/services/toastr.service'
import { TranslateService } from '@ngx-translate/core';

declare var $: any;

@Component({
    moduleId: module.id,
    selector: 'resetPassword-cmp',
    templateUrl: 'resetPassword.component.html',
    styleUrls: ['resetPassword.css']
})

export class ResetPasswordComponent implements OnInit {

    public userName: string;
    public waitingForResponse: boolean = false;
    public currentDate: Date = new Date();

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


    resetPassword(passwordResetForm) {

        var commonUserPrompt = this.translateService.instant("PagesResetPassword.UserPrompt");

        this.waitingForResponse = true;

        this.userService
            .resetPassword(this.userName)
            .subscribe(
            (data: ApiResult) => {
                if (data.c === glob.SuccessCode) {
                    this.toastrService.success(commonUserPrompt);
                } else {
                    this.toastrService.success(commonUserPrompt);
                }

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {

                this.toastrService.success(commonUserPrompt);

                this.waitingForResponse = false;
            }
        );
    }

}
