import { Component, OnInit, AfterViewInit, OnDestroy, NgZone } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { ServerResponse } from '../../core/models/serverResponse'
import { ModalService } from '../../core/services/modal.service';
import { PageService } from '../../core/services/page.service';
import { LicenceService } from '../../core/services/licence.service';
import { AesService } from '../../core/services/aes.service';
import { TranslateService } from '@ngx-translate/core';

import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable';
import { SeatFilter } from '../../core/models/seatFilter';

declare var $: any;
declare var swal: any;

@Component({
    selector: 'seat',
    templateUrl: 'seat.component.html',
    styleUrls: ['seat.css']
})
export class SeatComponent implements OnInit, OnDestroy {

    public headerRow: string[] = ['Device Type', 'Device Name', 'First Name', 'Last Name', 'Optional Data', 'Device Model', 'OS Version', 'Activation Date', 'Last Updated Date'];
    public seaDetails = [];
    private subscriptionId: number;
    private brandId: string;
    private licenceKey: string;
    private licencingEnvironment: number;

    public localCount: number = 0;
    public localOffset: number = 0;
    public localLimit: number = 0;  // page size
    public waitingForResponse: boolean = false;

    private currentPageNumber: number = 1;
    private seatFilter: SeatFilter = {};

    private subscription: any;
    private key: string;
    private vector: string;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private pageService: PageService,
        private licenceService: LicenceService,
        private ngZone: NgZone,
        private translateService: TranslateService,
        private aesService: AesService,
        private utilityService: UtilityService
    ) {
    }

    setDeviceGrid() {
        this.headerRow[0] = this.translateService.instant("DashboardLicence.DeviceType");
        this.headerRow[1] = this.translateService.instant("DashboardLicence.grid.DeviceName");
        this.headerRow[2] = this.translateService.instant("Common.FirstName");
        this.headerRow[3] = this.translateService.instant("Common.LastName");

        this.headerRow[4] = this.translateService.instant("Common.OptionalData");

        this.headerRow[5] = this.translateService.instant("DashboardLicence.grid.DeviceModel");
        this.headerRow[6] = this.translateService.instant("DashboardLicence.grid.OsVersion");
        this.headerRow[7] = this.translateService.instant("DashboardLicence.grid.ActivationDate");
        this.headerRow[8] = this.translateService.instant("DashboardLicence.grid.LastUpdatedDate");

    }


    zoneDeactivateDevice(seatDetail) {

        this.licenceService.deactivateSeat(this.subscriptionId, seatDetail).subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    if (this.isItLastRecordInPage() && this.currentPageNumber > 1) {
                        this.currentPageNumber--;
                    }
                    this.getDecryptInfo(this.currentPageNumber);

                    var successfulMessage = this.translateService.instant("SeatDetail.SeatDeactivateSuccessful");
                    this.toastrService.success(successfulMessage);

                } else {
                    var errorMessage = this.licenceService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.toastrService.error(errorMessage);
                }
            },
            (err: HttpErrorResponse) => {
                var errorMessage = this.translateService.instant("SeatDetail.SeatDeactivateFailed");
                this.toastrService.error(errorMessage);
            }

        );
    }

    deactivate(seatDetail) {
        var message = this.translateService.instant("SeatDetail.SeatDeactivateWarning", { deviceName: seatDetail.deviceName });
        var confirmText = this.translateService.instant("SeatDetail.SeatDeactivateConfirm");
        var cancelText = this.translateService.instant("Common.No");
        var titleText = this.translateService.instant("Common.AreYouSure");

        swal({
            title: titleText,
            text: message,
            type: 'warning',
            showCancelButton: true,
            confirmButtonClass: 'btn',
            cancelButtonClass: 'btn',
            confirmButtonText: confirmText,
            cancelButtonText: cancelText,
            buttonsStyling: false
        }).then(function (result) {

            if (result.value)
                window['my']['licence']['deactivateDevice'](seatDetail);

        }).catch(swal.noop);

    }


    deactivateDevice(seatDetail) {
        this.ngZone.run(() => this.zoneDeactivateDevice(seatDetail));
    }

    ngOnDestroy() {
        window['my']['licence']['deactivateDevice'] = null;
        this.subscription.unsubscribe();
    }


    ngOnInit() {

        window['my'] = window['my'] || {};
        window['my']['licence'] = window['my']['namespace'] || {};
        window['my']['licence']['deactivateDevice'] = this.deactivateDevice.bind(this);

        this.route.queryParams.subscribe(params => {
            this.subscriptionId = +params['si'] || 0;
            this.brandId = params['bi'];
            this.licenceKey = params['lk'];
            this.licencingEnvironment = params['le'];

            this.getDecryptInfo(1);
        });

        this.localLimit = this.pageService.getPageSize();
        this.subscription = this.pageService.pageSizeSubject.subscribe(item => { this.localLimit = item; });

        this.setDeviceGrid();
    }

    getDecryptInfo(pageNumber) {

        this.aesService.getKey(this.subscriptionId)
            .subscribe(
            (data: any) => {
                if (data.c === glob.SuccessCode) {
                    this.key = data.key;
                    this.vector = data.vector;

                    this.getSeatDetailWithPage(pageNumber);
                }
            },
            (err: HttpErrorResponse) => {
                this.alertService.error('Failed to retrieve key.');
            });
    }

    isItLastRecordInPage(): boolean {

        return this.localCount - (this.currentPageNumber - 1) * this.localLimit == 1;
    }

    searchSeat(seatFilter: SeatFilter) {
        this.seatFilter = seatFilter;

        this.getDecryptInfo(1);
    }

    getSeatDetailWithPage(pageNumber) {

        this.licenceService.getSeatDetail(this.subscriptionId, pageNumber, this.seatFilter)
            .subscribe(
            (data: any) => {

                let tempSeatDetails = data.d.map((item) => {

                    item.firstName = this.aesService.decrypt(this.vector, item.firstName, this.key);
                    item.lastName = this.aesService.decrypt(this.vector, item.lastName, this.key);
                    item.optionalData = this.aesService.decrypt(this.vector, item.optionalData, this.key);
                    item.deviceName = item.deviceName.replace("?", "'");
                    item.localActivationDate = this.utilityService.convertUtcToLocalDate(item.activationDate);
                    item.localLastUpdateDate = this.utilityService.convertUtcToLocalDate(item.lastUpdateDate);

                    if (item.osVersion.indexOf('iOS') >= 0) {
                        item.isIos = true;

                        if (this.utilityService.versionCompare(item.versionNumber, '2.5.1', { zeroExtend: true}) >= 0)
                            item.showEvent = true;
                        else
                            item.showEvent = false;
                    }
                    else {
                        item.isIos = false;
                        if (this.utilityService.versionCompare(item.versionNumber, '1.0148', { zeroExtend: true }) >= 0)
                            item.showEvent = true;
                        else
                            item.showEvent = false;
                    }

                    return item;
                });

                this.seaDetails = tempSeatDetails;

                this.localOffset = (pageNumber - 1) * this.localLimit;
                this.localCount = data.t;

                this.key = null;
            },
            (err: HttpErrorResponse) => {
                this.alertService.error('Failed to retrieve enterprise list.');

            }
            );

    }

    viewDeviceEvents(seatDetail) {

        var url = '/dashboard/deviceeventsandroid';

        if (seatDetail.isIos)
            url = '/dashboard/deviceeventsios';
        else
            url = '/dashboard/deviceeventsandroid';

        this.router.navigate([url], {
            queryParams: {
                sk: seatDetail.seatKey,
                dn: seatDetail.deviceName,
                ad: seatDetail.activationDate,
                ld: seatDetail.lastUpdateDate,
                ov: seatDetail.osVersion,
                bi: this.brandId,
                lk: this.licenceKey,
                le: this.licencingEnvironment
            }
        });

    }


}