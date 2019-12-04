import { Component, OnInit } from '@angular/core'
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { DashboardService } from '../../core/services/dashboard.service';

import { Partner } from '../../core/models/partner';
import { Enterprise } from '../../core/models/enterprise';
import { Subscription } from '../../core/models/subscription';
import { UserRole } from '../../core/models/userRole';
import { DashboardOverview } from '../../core/models/dashboardOverview';
import { DashboardPartner } from '../../core/models/dashboardPartner';
import { DashboardEnterprise } from '../../core/models/dashboardEnterprise';
import { ApiResult } from '../../core/models/apiResult';

import { LookupService } from '../../core/services/lookup.service'
import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { AuthenticationService } from '../../core/services/authentication.service'
import { TranslateService } from '@ngx-translate/core';

import * as glob from '../../core/variables/global.variable';

@Component({
    selector: 'overview-cmp',
    templateUrl: './overview.component.html',
    styleUrls: ['overview.css']
})
export class OverviewComponent implements OnInit {

    public headerRow: string[] = ['Subscription Name', 'Enterprise Application', 'Created', 'Enterprise Seats', 'Status', 'Details', 'Seat Detail'];

    public dashboardOverview: DashboardOverview = {};

    public waitingForResponse: boolean = false;

    private products: any;

    public userRole: UserRole;

    constructor(
        private dashboardService: DashboardService,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private lookupService: LookupService,
        private router: Router,
        private translateService: TranslateService,
        private authenticationService: AuthenticationService
    ) {
    }

    ngOnInit() {
        this.getHeaders();

        this.getDashboard();

        this.userRole = this.authenticationService.GetCurrentUserIdAndRole().role;
    }

    getHeaders() {

        this.translateService.get('Common.SubscriptionName').subscribe((res: string) => {
            this.headerRow[0] = res;
        });

        this.translateService.get('Common.EnterpriseApplication').subscribe((res: string) => {
            this.headerRow[1] = res;
        });

        this.translateService.get('Common.Created').subscribe((res: string) => {
            this.headerRow[2] = res;
        });

        this.translateService.get('Common.EnterpriseSeats').subscribe((res: string) => {
            this.headerRow[3] = res;
        });

        this.translateService.get('Common.Status').subscribe((res: string) => {
            this.headerRow[4] = res;
        });

        this.translateService.get('Common.Details').subscribe((res: string) => {
            this.headerRow[5] = res;
        });

        this.translateService.get('SeatDetail.SeatDetailButton').subscribe((res: string) => {
            this.headerRow[6] = res;
        });

    }

    getDashboard() {

        this.waitingForResponse = true;

        this.dashboardService.getDashboard()
            .subscribe(
            (data: ApiResult) => {
                if (data.c === glob.SuccessCode) {

                    this.dashboardOverview = this.dashboardService.getOverview(data.d);
                }
                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to retrieve dashboard data.');

                this.waitingForResponse = false;
            }
            );
    }

    viewLicence(subscriptionId) {

        this.router.navigate(['/dashboard/licence'], { queryParams: { subscriptionId: subscriptionId } });

    }

    viewSeatDetail(subscriptionId, brandId, licenceKey, licencingEnvironment) {

        this.router.navigate(['/dashboard/seat'], { queryParams: { si: subscriptionId, bi: brandId, lk: licenceKey, le: licencingEnvironment } });

    }

}