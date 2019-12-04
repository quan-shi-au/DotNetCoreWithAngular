import { Component, OnInit, AfterViewInit, OnDestroy, NgZone, ElementRef, ViewChild } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { ServerResponse } from '../../core/models/serverResponse'
import { ModalService } from '../../core/services/modal.service';
import { PageService } from '../../core/services/page.service';
import { LicenceService } from '../../core/services/licence.service';
import { AesService } from '../../core/services/aes.service';
import { DeviceeventsService } from '../../core/services/deviceevents.service';
import { UtilityService } from '../../core/services/utility.service';
import { TranslateService } from '@ngx-translate/core';

import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable';
import { SeatFilter } from '../../core/models/seatFilter';
import { deviceEventQuery } from '../../core/models/deviceEventQuery';
import { deviceEventResult } from '../../core/models/deviceEventResult';

declare var $: any;
declare var swal: any;


@Component({
    selector: 'deviceeventsios',
    templateUrl: 'deviceeventsios.component.html',
    styleUrls: ['deviceeventsios.css']
})
export class DeviceeventsIosComponent implements OnInit {

    public deviceEventResults: deviceEventResult[] = [];
    public deviceHealthResult: any = [];
    public deviceInformationResult: any = [];
    public deviceScanSummaryResults: any = [];
    public deviceMalwareEvents: any = [];
    public secureAppsEvents: any = [];

    public subscriptionId?: number;
    public seatKey?: string;
    public deviceName?: string;
    public activationDate?: any;
    public lastUpdateDate?: any;
    public deviceModel?: string;
    public deviceType?: string;
    public osVersion?: string;
    public webProtectStatus: number;

    private brandId: string;
    private licenceKey: string;
    private licencingEnvironment: number = glob.LicencingEnvironmentprod;
    private scanSummaryEventType: string;
    private scanSummaryEventSubtype: string;

    private isDeviceInformationRetrieved: boolean = false;
    private isScanSummaryRetrieved: boolean = false;
    private isFeatureStatusRetrieved: boolean = false;
    public isUpToDate: boolean;

    public waitingForResponse: boolean = false;

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
        private deviceeventsService: DeviceeventsService,
        private utilityService: UtilityService
    ) {
    }

    ngOnInit() {

        this.route.queryParams.subscribe(params => {

            this.subscriptionId = +params['si'] || 0;
            this.seatKey = params['sk'];
            this.deviceName = params['dn'];

            this.alertService.setDeviceName(this.deviceName);
            this.activationDate = this.utilityService.convertUtcToLocalDate(params['ad']);
            this.lastUpdateDate = this.utilityService.convertUtcToLocalDate(params['ld']);

            var dayDifference = this.utilityService.dayDifference(this.lastUpdateDate, new Date());
            if (dayDifference > 7)
                this.isUpToDate = false;
            else
                this.isUpToDate = true;

            this.deviceModel = params['dm'];
            this.deviceType = params['dt'];
            this.osVersion = params['ov'];
            this.brandId = params['bi'];
            this.licenceKey = params['lk'];
            this.licencingEnvironment = params['le'];

            this.getDeviceHealth();
            this.getDeviceInformation();
            this.getWebProtect();
            this.getDeviceScanSummary();

        });

    }

    getDeviceEventQuery(eventName) {

        var eventDefinition = this.deviceeventsService.getEventDefinition(eventName);

        var query: deviceEventQuery = {
            BrandId: this.brandId,
            LicenceKey: this.licenceKey,
            SeatKey: this.seatKey,
            OS: glob.DeviceProductType.SC_SS_iOS
        };

        return query;
    }

    getDeviceHealth() {

        var query = this.getDeviceEventQuery('DeviceHealth');

        this.deviceeventsService.getDeviceEvents(query, glob.DeviceEventUrl.health).subscribe(
            (data: any) => {
                if (data.result == glob.DeviceEventResult.success) {

                    this.deviceHealthResult = data.deviceHealth;
                }
            },
            (err: HttpErrorResponse) => {


            }
        );

    }

    getWebProtect() {

        var query = this.getDeviceEventQuery('WebProtect');

        this.deviceeventsService.getDeviceEvents(query, glob.DeviceEventUrl.webprotect).subscribe(
            (data: any) => {
                if (data.result == glob.DeviceEventResult.success) {

                    if (data.webProtectStatus.status)
                        this.webProtectStatus = data.webProtectStatus.status;
                    else if (data.webProtectStatus)
                        this.webProtectStatus = data.webProtectStatus;

                    this.isFeatureStatusRetrieved = true;

                }
            },
            (err: HttpErrorResponse) => {


            }
        );

    }

    setDeviceInformationItem(item, source) {
        item.DeviceName = source.DeviceName;
        item.DeviceModel = source.DeviceModel;
        item.DeviceType = source.DeviceType;
        item.OperatingSystem = source.OperatingSystem;
        item.Architecture = source.Architecture;

        item.Memory = this.deviceeventsService.getDeviceMemory(source.Memory);

        this.alertService.setDeviceName(item.DeviceName);

    }

    getDeviceInformation() {

        var query = this.getDeviceEventQuery('DeviceInformation');

        this.deviceeventsService.getDeviceEvents(query, glob.DeviceEventUrl.info).subscribe(
            (data: any) => {
                if (data.result == glob.DeviceEventResult.success) {


                    this.deviceInformationResult = data.deviceInformation;
                    this.deviceInformationResult.memory = this.deviceeventsService.getDeviceMemory(data.deviceInformation.memory);

                    this.alertService.setDeviceName(data.deviceInformation.deviceName);

                    this.isDeviceInformationRetrieved = true;

                }
            },
            (err: HttpErrorResponse) => {

            }
        );

    }

    setDeviceScanSummaryItem(item, source) {

        item.ScanType = source.ScanType;
        item.ScanStartDateTime = this.utilityService.convertToDate(source.ScanStartDateTime);
        item.ScanEndDateTime = this.utilityService.convertToDate(source.ScanEndDateTime);
        item.Duration = source.ScanDuration;
        item.AppScanCount = source.AppScanCount;
        item.MaliciousAppCount = source.MaliciousAppCount;

        if (source.StartupStatus == '1')
            item.StartupStatus = 'passed';
        else
            item.StartupStatus = 'failed';

        if (source.PasscodeStatus == '1')
            item.PasscodeStatus = 'enabled';
        else
            item.PasscodeStatus = 'not enabled';

        if (source.InternetStatus = '1')
            item.InternetStatus = 'yes';
        else
            item.InternetStatus = 'no';
    }


    getDeviceScanSummary() {

        var query = this.getDeviceEventQuery('ScanSummary');

        this.deviceeventsService.getDeviceEvents(query, glob.DeviceEventUrl.scansummary).subscribe(
            (data: any) => {
                if (data.result == glob.DeviceEventResult.success) {

                    this.deviceScanSummaryResults = data.scanSummaryResult.map(item => {
                        item.scanStartDateTime = this.utilityService.convertUtcToLocalDate(item.scanStartDateTime);
                        return item;
                    });

                    this.isScanSummaryRetrieved = true;
                }
            },
            (err: HttpErrorResponse) => {


            }
        );

    }

    lastUpdatedToolTip() {

        //this.toastrService.info('Your device is online and has been updated recently.').css("width", "600px");

        if (this.isUpToDate)
            this.toastrService.info('Your device is online and has been updated recently.');
        else
            this.toastrService.info("Your device could be offline as it hasn't updated recently.");

    }

    featureStatusToolTip() {
        this.toastrService.info('The current status of key features for this device.');

    }

} 