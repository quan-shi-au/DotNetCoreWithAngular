import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core'
import { Router } from '@angular/router';
import { EnterpriseService } from '../../core/services/enterprise.service';
import { Enterprise } from '../../core/models/enterprise';
import { HttpErrorResponse } from '@angular/common/http';
import { PartnerService } from '../../core/services/partner.service';
import { Partner } from '../../core/models/partner';

import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { ServerResponse } from '../../core/models/serverResponse'
import { ModalService } from '../../core/services/modal.service';
import { PageService } from '../../core/services/page.service';

import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable';

declare var $: any;

declare interface TableData {
    headerRow: string[];
    dataRows: string[][];
}

@Component({
    selector: 'enterprise-list',
    templateUrl: 'enterprise-list.component.html',
    styleUrls: ['enterprise-list.css']
})
export class EnterpriseListComponent implements OnInit, OnDestroy {
    public tableData1: TableData;
    public enterprises: Enterprise[];
    public headerRow: string[] = ['Partner', 'Enterprise', 'Location'];

    public localCount: number = 0;
    public localOffset: number = 0;
    public localLimit: number = 0;  // page size
    public waitingForResponse: boolean = false;

    public isNameValid: boolean = false;
    public isLocationValid: boolean = false;
    public isFormValid: boolean = false;

    public enterpriseName: string = '';
    public enterpriseId: number;
    public partners: Partner[];
    public viewModel: any = {};

    public isPartnerValid: boolean = false;
    public isPartnerNotToched: boolean = true;

    public selectedPartnerName: string;
    public currentPartner: Partner;

    public localCaption = "Add Enterprise";
    public localTitle = "Add a new enterprise";
    public resetMargin = false;

    private subscription: any;

    constructor(
        private router: Router,
        private enterpriseService: EnterpriseService,
        private partnerService: PartnerService,
        private modalService: ModalService,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private pageService: PageService
    ) {
    }

    goToInsert = function () {
        this.router.navigateByUrl('/enterprise/insert');
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }


    ngOnInit() {

        this.waitingForResponse = true;
        this.partnerService.getPartners().subscribe(
            (data : any) => {
                this.partners = <Partner[]>data.d;

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to insert the enterprise.');
                this.waitingForResponse = false;

            }
        );

        this.localLimit = this.pageService.getPageSize();
        this.subscription = this.pageService.pageSizeSubject.subscribe(item => { this.localLimit = item; });

        this.getEnterpriseListWithPage(1);
    }

    selectPartnerButtonClick() {
        this.isPartnerNotToched = false;
    }

    selectPartner(selctedPartner) {

        this.selectedPartnerName = selctedPartner.name;
        this.currentPartner = selctedPartner;

        this.isPartnerNotToched = false;

        this.validatePartner();
    }

    public validatePartner() {

        if (this.currentPartner && this.currentPartner.name && UtilityService.trim(this.currentPartner.name))
            this.isPartnerValid = true;
        else
            this.isPartnerValid = false;

        this.validateForm();
    }

    public validateLocation(enterpriseForm) {
        var partner = <Partner>enterpriseForm.value;

        if (partner && partner.location && UtilityService.trim(partner.location))
            this.isLocationValid = true;
        else
            this.isLocationValid = false;

        this.validateForm();
    }


    public validateName(enterpriseForm) {

        var partner = <Partner>enterpriseForm.value;
        if (partner && partner.name && UtilityService.trim(partner.name) && partner.name.length >= 3 )
            this.isNameValid = true;
        else
            this.isNameValid = false;

        this.validateForm();

    }


    public validateForm() {

        if (this.isNameValid && this.isLocationValid && this.isPartnerValid)
            this.isFormValid = true;
        else
            this.isFormValid = false;
    }

    addEnterprise(enterpriseForm) {

        this.selectedPartnerName = "";
        this.currentPartner = {};

        this.isNameValid = false;
        this.isLocationValid = false;
        this.isPartnerValid = false;
        this.isPartnerNotToched = true;
        this.isFormValid = false;
        enterpriseForm.resetForm();

        this.alertService.reset();

        this.modalService.open('enterprise-insert-modal');
    }


    submitEnterprise(enterpriseForm) {

        this.waitingForResponse = true;

        var serverModel =
            {
                name: this.viewModel.name,
                location: this.viewModel.location,
                pid: this.currentPartner.id
            };

        this.enterpriseService.insertEnterprise(serverModel)
            .subscribe(
            (data: ApiResult) => {


                if (data.c === glob.SuccessCode) {
                    this.getEnterpriseListWithPage(1);

                    this.modalService.close('enterprise-insert-modal');

                    this.toastrService.success('Enterprise Added!');
                } else {
                    var errorMessage = this.enterpriseService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to insert the enterprise.');

                this.waitingForResponse = false;

            }
            )
            ;

    }

    closeModal() {

        this.modalService.close('enterprise-insert-modal');
    }

    getEnterpriseListWithPage(pageNumber) {

        this.enterpriseService.getEnterprisesWithPage(pageNumber)
            .subscribe(
            (data: any) => {

                this.enterprises = data.d;

                this.localOffset = (pageNumber - 1) * this.localLimit;
                this.localCount = data.t;
            },
            (err: HttpErrorResponse) => {
                this.alertService.error('Failed to retrieve enterprise list.');

            }
            );

    }

    ngAfterViewInit() {
        $('[rel="tooltip"]').tooltip();
    }

}