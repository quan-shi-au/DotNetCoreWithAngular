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

declare var $: any;

@Component({
    moduleId: module.id,
    selector: 'email-verify-cmp',
    templateUrl: 'emailVerify.component.html',
    styleUrls: ['emailverify.css']
})

export class EmailVerifyComponent implements OnInit {

    public currentDate: Date = new Date();

    public ErrorMessage: string;
    public isGeneralServerError: boolean = false;
    public isLinkReopenedError: boolean = false;
    public waitingForResponse: boolean = false;

    public id: string;
    public token: string;
    public returnUrl: string;

    public param: any = { hrefValue: '/pages/resetpassword'};

    constructor(
        private element: ElementRef,
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService,
        private alertService: AlertService,
        private userService: UserService,
        private toastrService: ToastrService,
    ) {

    }

    checkFullPageBackgroundImage() {
        var $page = $('.full-page');
        var image_src = $page.data('image');

        if (image_src !== undefined) {
            var image_container = '<div class="full-page-background" style="background-image: url(' + image_src + ') "/>'
            $page.append(image_container);
        }
    };


    ngOnInit() {

        this.checkFullPageBackgroundImage();


        this.returnUrl = this.route.snapshot.queryParams['returnUrl'];
        this.id = this.route.snapshot.queryParams['id'];
        this.token = this.route.snapshot.queryParams['token'];

        this.authenticationService.verifyToken(this.id, this.token)
            .subscribe(
            (data: ApiResult) => {
                if (data.c == glob.SuccessCode) {
                    this.isGeneralServerError = false;
                    this.isLinkReopenedError = false;
                    this.router.navigate(['/pages/changepassword'], { queryParams: { id: this.id, token: data.d.content } });
                }
                else if (this.authenticationService.isLinkPreviouslyOpened(data.c)) {
                    this.isGeneralServerError = false;
                    this.isLinkReopenedError = true;
                }
                else {
                    this.isGeneralServerError = true;
                    this.isLinkReopenedError = false;
                    this.ErrorMessage = this.authenticationService.getServerErrorByCode(data.c);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to verify email.');
            }
            );

    }
}
