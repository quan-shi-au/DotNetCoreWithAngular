import { Component, OnInit, AfterViewInit, HostListener, OnDestroy } from '@angular/core'
import { Router } from '@angular/router';
import { PartnerService } from '../../core/services/partner.service';
import { Partner } from '../../core/models/partner';
import { HttpErrorResponse } from '@angular/common/http';

import { AlertService } from '../../core/services/alert.service'
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { ApiResult } from '../../core/models/apiResult'
import * as glob from '../../core/variables/global.variable';
import { ModalService } from '../../core/services/modal.service';
import { PageService } from '../../core/services/page.service';

import { TranslateService } from '@ngx-translate/core';

declare var $: any;
declare var swal: any;

@Component({
    selector: 'partner-list',
    templateUrl: 'partner-list.component.html',
    styleUrls: ['partner-list.css']
})
export class PartnerListComponent implements OnInit, OnDestroy {
    public partners: Partner[];
    public headerRow: string[] = ['Name', 'Location'];
    public waitingForResponse: boolean = false;
    public partner: Partner;

    public isNameValid: boolean = false;
    public isLocationValid: boolean = false;
    public isFormValid: boolean = false;

    public localCount: number = 0;
    public localOffset: number = 0;
    public localLimit: number = 0;  // page size

    public localCaption = "Add Partner";
    public localTitle = "Add a new partner";
    public resetMargin = false;

    private subscription: any;

    constructor(
        private router: Router,
        private partnerService: PartnerService,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private modalService: ModalService,
        private translate: TranslateService,
        private pageService: PageService
    ) {
    }

    ngOnDestroy() {

        this.subscription.unsubscribe();
    }

    ngOnInit() {

        this.getPartnersWithPage(1);

        this.localLimit = this.pageService.getPageSize();
        this.subscription = this.pageService.pageSizeSubject.subscribe(item => { this.localLimit = item; });

    }

    public validateLocation(partnerForm) {
        var partner = <Partner>partnerForm.value;

        if (partner && partner.location && UtilityService.trim(partner.location))
            this.isLocationValid = true;
        else
            this.isLocationValid = false;

        this.validateForm();
    }


    public validateName(partnerForm) {

        var partner = <Partner>partnerForm.value;
        if (partner && partner.name && UtilityService.trim(partner.name) && partner.name.length >= 3)
            this.isNameValid = true;
        else
            this.isNameValid = false;

        this.validateForm();

    }


    public validateForm() {

        if (this.isNameValid && this.isLocationValid)
            this.isFormValid = true;
        else
            this.isFormValid = false;
    }

    getPartnersWithPage(pageNumber) {

        this.partnerService.getPartnersWithPage(pageNumber)
            .subscribe(
            (data: ApiResult) => {
                if (data.c === glob.SuccessCode) {
                    this.partners = <Partner[]>data.d;

                    this.localOffset = (pageNumber - 1) * this.localLimit;
                    this.localCount = data.t;
                }

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to get partner list.');
            }
            );

    }


    onPageChange(offset) {


        this.localOffset = offset;

    }

    addPartner(partnerForm) {
        this.isNameValid = false;
        this.isLocationValid = false;

        partnerForm.resetForm();
        this.alertService.reset();

        this.modalService.open('partner-insert-modal');
    }

    closeModalLocal(event) {

        this.modalService.close('partner-insert-modal');
    }

    submitPartner(partnerForm) {

        var partner = partnerForm.value;

        this.waitingForResponse = true;

        this.partnerService.insertPartner(partner)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {
                    this.getPartnersWithPage(1);

                    this.toastrService.success('Partner Added!');

                    this.modalService.close('partner-insert-modal');
                } else {
                    var errorMessage = this.partnerService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to insert the partner.');

                this.waitingForResponse = false;
            }
            );


    }




}