import { Component, OnInit, OnDestroy, NgZone } from '@angular/core'
import { HttpErrorResponse } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

import { DashboardService } from '../../core/services/dashboard.service';
import { SubscriptionService } from '../../core/services/subscription.service';

import { Partner } from '../../core/models/partner';
import { ChartistData } from '../../core/models/chartistData';
import { UsageReport } from '../../core/models/usageReport';
import { Enterprise } from '../../core/models/enterprise';
import { Subscription } from '../../core/models/subscription';
import { DashboardOverview } from '../../core/models/dashboardOverview';
import { DashboardPartner } from '../../core/models/dashboardPartner';
import { DashboardEnterprise } from '../../core/models/dashboardEnterprise';
import { ApiResult } from '../../core/models/apiResult';

import { LookupService } from '../../core/services/lookup.service'
import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { ReportService } from '../../core/services/report.service';
import { LicenceService } from '../../core/services/licence.service';
import { TranslateService } from '@ngx-translate/core';

import * as glob from '../../core/variables/global.variable';

import * as Chartist from 'chartist';

declare var swal: any;

@Component({
    selector: 'licence-cmp',
    templateUrl: './licence.component.html',
    styleUrls: ['licence.css']
})
export class LicenceComponent implements OnInit, OnDestroy {

    public waitingForResponse: boolean = false;
    private subscriptionId: number;
    public currentSubscription: any = {};

    public headerRow: string[] = ['Device Type', 'Device Name', 'First Name', 'Last Name', 'Device Model', 'OS Version', 'Activation Date', 'Last Updated Date', 'Actions'];
    public seaDetails = [];

    public usageTitle = 'Usage';
    public usageId = 'usageId';
    public usageLegend = ['Active', 'Available'];
    public usageData: ChartistData = { series: [] };
    
    public osTitle = 'Device OS Version';
    public osId = 'osId';
    public osLegend = [];
    public osData: ChartistData = { series: [] };

    public manufacturerTitle = 'Device Manufacturer';
    public manufacturerId = 'manufacturerId';
    public manufacturerLegend = [];
    public manufacturerData: ChartistData = { series: [] };

    public typeTitle = 'Device Type';
    public typeId = 'typeId';
    public typeLegend = [];
    public typeData: ChartistData = { series: [] };

    public noChartMessageUsage = 'No data available';
    public noChartMessageOs = 'No data available';
    public noChartMessageManufacturer = 'No data available';
    public noChartMessageType = 'No data available';

    public isChartAvailableUsage: boolean = false;
    public isChartAvailableOs: boolean = false;
    public isChartAvailableManufacturer: boolean = false;
    public isChartAvailableType: boolean = false;

    constructor(
        private dashboardService: DashboardService,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private lookupService: LookupService,
        private route: ActivatedRoute,
        private router: Router,
        private subscriptionService: SubscriptionService,
        private ngZone: NgZone,
        private reportService: ReportService,
        private translateService: TranslateService,
        private licenceService: LicenceService,
        private utilityService: UtilityService
    ) {
    }

    ngOnDestroy() {
    }

    ngOnInit() {

        this.usageId = glob.usageId;

        this.route.queryParams.subscribe(params => {
            this.subscriptionId = +params['subscriptionId'] || 0;

            this.getReport(this.subscriptionId);

        });

        this.localize();

    }

    localize() {
        this.translateService.get('DashboardLicence.Usage').subscribe((res: string) => {
            this.usageTitle = res;
        });

        this.translateService.get('DashboardLicence.DeviceOsVersion').subscribe((res: string) => {
            this.osTitle = res;
        });
        this.translateService.get('DashboardLicence.DeviceManufacturer').subscribe((res: string) => {
            this.manufacturerTitle = res;
        });
        this.translateService.get('DashboardLicence.DeviceType').subscribe((res: string) => {
            this.typeTitle = res;
        });

        this.translateService.get('DashboardLicence.NoDataAvailable').subscribe((res: string) => {
            this.noChartMessageUsage = res;
            this.noChartMessageOs = res;
            this.noChartMessageManufacturer = res;
            this.noChartMessageType = res;
        });

        this.setDeviceGrid();
        this.setLegend();

    }

    setLegend() {
        this.usageLegend[0] = this.translateService.instant("DashboardLicence.Active");
        this.usageLegend[1] = this.translateService.instant("DashboardLicence.Available");

    }

    setDeviceGrid() {
        this.headerRow[0] = this.translateService.instant("DashboardLicence.DeviceType");
        this.headerRow[1] = this.translateService.instant("DashboardLicence.grid.DeviceName");
        this.headerRow[2] = this.translateService.instant("Common.FirstName");
        this.headerRow[3] = this.translateService.instant("Common.LastName");

        this.headerRow[4] = this.translateService.instant("DashboardLicence.grid.DeviceModel");
        this.headerRow[5] = this.translateService.instant("DashboardLicence.grid.OsVersion");
        this.headerRow[6] = this.translateService.instant("DashboardLicence.grid.ActivationDate");
        this.headerRow[7] = this.translateService.instant("DashboardLicence.grid.LastUpdatedDate");
    }

    getReport(subscriptionId) {

        this.waitingForResponse = true;

        this.reportService.getReport(subscriptionId).subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.getSubscription(data);

                    this.getUsageData(data);
                    this.getOsData(data);
                    this.getManufacturerData(data);
                    this.getTypeData(data);

                    this.getSeatDetails(data);

                } else {
                    var errorMessage = this.reportService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to retrieve report.');

                this.waitingForResponse = false;
            }

        );


    }

    getSeatDetails(data) {
        if (data.d.seatdetailsreport.report)
            this.seaDetails = data.d.seatdetailsreport.data;

    }

    getSubscription(data) {
        this.currentSubscription = data.d.subscriptiondetails;
        this.currentSubscription.subdate = this.utilityService.convertUtcToLocalDate(this.currentSubscription.subdate);
        this.currentSubscription.reportdate = this.utilityService.convertUtcToLocalDate(data.d.reportdate);
    }

    getUsageData(data) {
        var usageReport: UsageReport = data.d.usagereport;
        if (usageReport.report) {
            var newUsageData: ChartistData = { series: [] };

            if (usageReport.used && +usageReport.used > 0)
                newUsageData.series.push({ value: usageReport.used, className: 'manager-pie-chunk1' });

            if (usageReport.available && +usageReport.available > 0)
                newUsageData.series.push({ value: usageReport.available, className: 'manager-pie-chunk2' });

            this.usageData = newUsageData;

        } else {
            this.noChartMessageUsage = this.reportService.getServerErrorByCode(data.c);
        }

        if (this.usageData && this.usageData.series && this.usageData.series.length > 0)
            this.isChartAvailableUsage = true;
        else
            this.isChartAvailableUsage = false;
    }

    getOsData(data) {
        var osReport = data.d.deviceosreport;
        if (osReport.report) {
            var newOsData: ChartistData = { series: [] };

            let i: number = 1;
            for (let item of osReport.data) {
                newOsData.series.push({ value: item.count, className: 'manager-pie-chunk' + i.toString() });
                this.osLegend.push(item.name);
                i++;
            }

            this.osData = newOsData;

        } else {
            this.noChartMessageOs = this.reportService.getServerErrorByCode(data.c);
        }

        if (this.osData && this.osData.series && this.osData.series.length > 0)
            this.isChartAvailableOs = true;
        else
            this.isChartAvailableOs = false;
    }

    getManufacturerData(data) {
        var manufacturerReport = data.d.devicemanufacturerreport;
        if (manufacturerReport.report) {
            var newManufacturerData: ChartistData = { series: [] };

            let i: number = 1;
            for (let item of manufacturerReport.data) {
                newManufacturerData.series.push({ value: item.count, className: 'manager-pie-chunk' + i.toString() });
                this.manufacturerLegend.push(item.name);
                i++;
            }

            this.manufacturerData = newManufacturerData;

        } else {
            this.noChartMessageManufacturer = this.reportService.getServerErrorByCode(data.c);
        }

        if (this.manufacturerData && this.manufacturerData.series && this.manufacturerData.series.length > 0)
            this.isChartAvailableManufacturer = true;
        else
            this.isChartAvailableManufacturer = false;

    }

    getTypeData(data) {
        var typeReport = data.d.devicetypereport;
        if (typeReport.report) {
            var newTypeData: ChartistData = { series: [] };

            let i: number = 1;
            for (let item of typeReport.data) {
                newTypeData.series.push({ value: item.count, className: 'manager-pie-chunk' + i.toString() });
                this.typeLegend.push(item.name);
                i++;
            }

            this.typeData = newTypeData;

        } else {
            this.noChartMessageType = this.reportService.getServerErrorByCode(data.c);
        }

        if (this.typeData && this.typeData.series && this.typeData.series.length > 0)
            this.isChartAvailableType = true;
        else
            this.isChartAvailableType = false;

    }


    searchDevices(searchCriteria) {
        console.log('search clicked');

        this.seaDetails = this.seaDetails.filter(x => x.deviceType == 'phone');
    }

    pageDevices(event) {
        console.log('paging clicked');
        console.log(event);
    }

}