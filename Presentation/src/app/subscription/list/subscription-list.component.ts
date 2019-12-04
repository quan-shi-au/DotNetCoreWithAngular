import { Component, OnInit, AfterViewInit, NgZone, OnDestroy, ChangeDetectorRef  } from '@angular/core'
import { ActivatedRoute, Router } from '@angular/router';
import { SubscriptionService } from '../../core/services/subscription.service';
import { Subscription } from '../../core/models/subscription';
import {HttpErrorResponse} from '@angular/common/http';


import { EnterpriseService } from '../../core/services/enterprise.service';
import { Enterprise } from '../../core/models/enterprise';

import { PartnerService } from '../../core/services/partner.service';
import { Partner } from '../../core/models/partner';

import { AlertService } from '../../core/services/alert.service'
import { ModalService } from '../../core/services/modal.service';
import { ToastrService } from '../../core/services/toastr.service'
import { UtilityService } from '../../core/services/utility.service'
import { LookupService } from '../../core/services/lookup.service'
import { PageService } from '../../core/services/page.service';

import * as glob from '../../core/variables/global.variable';

import { ApiResult } from '../../core/models/apiResult';

declare var $: any;
declare var swal: any;

declare interface TableData {
    headerRow: string[];
    dataRows: string[][];
}

@Component({
    selector: 'subscription-list',
    templateUrl: 'subscription-list.component.html',
    styleUrls: ['subscription-list.css']
})
export class SubscriptionListComponent implements OnInit{
    public tableData1: TableData;
    searchType: any;
    searchText: any;
    searchTypes: string[] = ["Product", "Campaign", "Brand"]
    selectedSearchType: string = "Product";

    subscriptions: Subscription[];
    public headerRow: string[] = ['Partner', 'Enterprise', 'Subscription Name', 'Enterprise Application', 'Created', 'Enterprise Seats', 'Status', 'Details', 'Seat Detail', 'Actions'];

    public localCount: number = 0;
    public localOffset: number = 0;
    public localLimit: number = 0;  // page size

    public subscriptionName: string = '';
    public subscriptionId: number;
    public partners: Partner[];
    public enterprises: Enterprise[];

    public selectedPartnerName: string;
    public currentPartner: Partner;

    public selectedEnterpriseName: string;
    public currentEnterprise: Enterprise;

    public selectedLicencingEnvironment: any = {};
    public licencingEnvironments: any;

    public products: any;
    public selectedProduct: any = {};

    public isPartnerValid: boolean;
    public isPartnerNotToched: boolean = true;
    public isEnterpriseValid: boolean;
    public isEnterpriseNotToched: boolean = true;
    public isProductValid: boolean;
    public isProductNotToched: boolean = true;

    public isLicencingEnvironmentValid: boolean = true;
    public isLicencingEnvironmentNotToched: boolean = true;
    public isBrandIdValid: boolean;
    public isCampaignValid: boolean;
    public isSeatCountValid: boolean;
    public isAuthUsernameValid: boolean;
    public isAuthPasswordValid: boolean;
    public isClientDownloadLocationValid: boolean;

    public isFormValid: boolean;

    public newSeatCount: number;
    public isIncreaseSeatcountFormValid: boolean;
    public isIscNewSeatcountValid: boolean;
    public selectedSubscription: Subscription = {};

    public twoWayModel: any = {
    };

    public waitingForResponse: boolean = false;

    public increaseSeatcountCaption = "Increase Seat Count";
    public increaseSeatcountTitle = "Increase seat count";
    public increaseSeatcountResetMargin = false;

    public addSubscriptionCaption = "Add Subscription";
    public addSubscriptionTitle = "Add a new subscription";
    public addSubscriptionResetMargin = true;

    private subscription: any;
    private currentPageNumber: number = 1;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private subscriptionService: SubscriptionService,
        private partnerService: PartnerService,
        private enterpriseService: EnterpriseService,
        private modalService: ModalService,
        private alertService: AlertService,
        private toastrService: ToastrService,
        private lookupService: LookupService,
        private ngZone: NgZone,
        protected changeDetectorRef: ChangeDetectorRef,
        private pageService: PageService,
        private utilityService: UtilityService
    ) {
    }

    goToInsert = function () {
        this.router.navigateByUrl('/subscription/insert');
    }

    clickSearchType(searchType) {
        this.selectedSearchType = searchType;
    }

    addSubscription(subscriptionForm) {

        this.selectedPartnerName = "";
        this.currentPartner = {};
        this.selectedEnterpriseName = "";
        this.currentEnterprise = <Enterprise>{};
        this.selectedProduct = {};
        this.selectedLicencingEnvironment = {};

        this.isPartnerValid = false;
        this.isPartnerNotToched = true;
        this.isEnterpriseValid = false;
        this.isEnterpriseNotToched = true;
        this.isProductValid = false;
        this.isProductNotToched = true;

        this.isLicencingEnvironmentValid = false;
        this.isLicencingEnvironmentNotToched = true;
        this.isBrandIdValid = false;
        this.isCampaignValid = false;
        this.isSeatCountValid = false;
        this.isAuthUsernameValid = false;
        this.isAuthPasswordValid = false;
        this.isClientDownloadLocationValid = false;
        this.isFormValid = false;

        subscriptionForm.resetForm();
        this.alertService.reset();

        this.modalService.open('subscription-insert-modal');
    }


    goToEdit(id) {
        this.router.navigate(['/subscription/insert', { subscriptionId: id } ]);
    }

    privateFunc(subscriptionId) {
        this.subscriptionService
            .cancel(subscriptionId)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.toastrService.success("Subscription cancelled.");

                    this.getProductsAndRefreshSubscription(this.currentPageNumber);

                } else {
                    var errorMessage = this.subscriptionService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to cancel subscription.');

                this.waitingForResponse = false;
            }
            )
            ;
    }

    publicFunc(subscriptionId) {
        this.ngZone.run(() => this.privateFunc(subscriptionId));
    }

    ngOnDestroy() {
        window['my']['namespace']['publicFunc'] = null;

        this.subscription.unsubscribe();
    }

    ngOnInit() {

        window['my'] = window['my'] || {};
        window['my']['namespace'] = window['my']['namespace'] || {};
        window['my']['namespace']['publicFunc'] = this.publicFunc.bind(this);

        this.localLimit = this.pageService.getPageSize();
        this.subscription = this.pageService.pageSizeSubject.subscribe(item => { this.localLimit = item; });

        this.subscriptionId = this.route.snapshot.params['subscriptionId'];

        if (!this.selectedLicencingEnvironment) {
            this.selectedLicencingEnvironment = this.licencingEnvironments[0];
        }

        this.partnerService.getPartners().subscribe(
            (data : any) => {
                this.partners = <Partner[]>data.d;
            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Failed to retrieve partner list.');
            }
        );

        this.getProductsAndRefreshSubscription(this.currentPageNumber);

        this.getLicenceEnvironments();

    }

    getSubscriptionsWithPage(pageNumber) {

        this.currentPageNumber = pageNumber;

        this.subscriptionService.getSubscriptionsWithPage(pageNumber)
            .subscribe(
            (data: any) => {

                this.localOffset = (pageNumber - 1) * this.localLimit;
                this.localCount = data.t;

                this.subscriptions = data.d;
                this.subscriptions = this.subscriptions.map(x => {
                    var p = this.products.filter(y => y.wId == x.product)[0];
                    x.productName = p.name;
                    x.status = x.status ? 'Active' : 'Cancelled';
                    x.creationTime = this.utilityService.convertUtcToLocalDate(x.creationTime);
                    return x;
                });

            },
            (err: HttpErrorResponse) => {
                this.alertService.error('Failed to retrieve subscription list');
            }
            );
    }

    ngAfterViewInit() {
        $('[rel="tooltip"]').tooltip();
    }

    selectPartner(selctedPartner) {

        this.selectedPartnerName = selctedPartner.name;
        this.currentPartner = selctedPartner;

        this.isPartnerNotToched = false;

        this.validatePartner();

        this.selectedEnterpriseName = '';
        this.currentEnterprise = {};

        this.getEnterprises(this.currentPartner.id);
    }


    getEnterprises(pid: number) {

        this.waitingForResponse = true;

        this.enterpriseService.getEnterprisesForPartner(pid)
            .subscribe(
            (data: any) => {
                this.enterprises = data.d;

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {
                this.alertService.error('failed to retrieve enterprise list.');

                this.waitingForResponse = false;

            }
            );


    }

    public validateForm() {

        if (this.isPartnerValid && this.isEnterpriseValid && this.isProductValid && this.isLicencingEnvironmentValid && this.isBrandIdValid 
            && this.isCampaignValid && this.isSeatCountValid && this.isAuthUsernameValid &&
            this.isAuthPasswordValid && this.isClientDownloadLocationValid
        )
            this.isFormValid = true;
        else
            this.isFormValid = false;
    }

    public validateProduct() {

        if (this.selectedProduct && this.selectedProduct.name && UtilityService.trim(this.selectedProduct.name))
            this.isProductValid = true;
        else
            this.isProductValid = false;

        this.validateForm();
    }

    public validatePartner() {

        if (this.currentPartner && this.currentPartner.name && UtilityService.trim(this.currentPartner.name))
            this.isPartnerValid = true;
        else
            this.isPartnerValid = false;

        this.validateForm();
    }

    selectLicencingEnvironment(licencingEnvironment) {
        this.selectedLicencingEnvironment = licencingEnvironment;

        this.isLicencingEnvironmentNotToched = false;

        this.validateLicencingEnvironment();

    }

    public validateLicencingEnvironment() {

        if (this.selectedLicencingEnvironment && this.selectedLicencingEnvironment.name && UtilityService.trim(this.selectedLicencingEnvironment.name))
            this.isLicencingEnvironmentValid = true;
        else
            this.isLicencingEnvironmentValid = false;

        this.validateForm();
    }

    closeModal() {

        this.modalService.close('subscription-insert-modal');
    }


    selectEnterprise(selctedEnterprise) {
        this.selectedEnterpriseName = selctedEnterprise.name;
        this.currentEnterprise = selctedEnterprise;

        this.isEnterpriseNotToched = false;
        this.validateEnterprise();

    }

    public validateEnterprise() {

        if (this.currentEnterprise && this.currentEnterprise.name && UtilityService.trim(this.currentEnterprise.name))
            this.isEnterpriseValid = true;
        else
            this.isEnterpriseValid = false;

        this.validateForm();
    }


    validateControl(subscriptionForm, controlName) {
        switch (controlName) {
            case 'brandId':
                if (UtilityService.trim(subscriptionForm.value.brandId))
                    this.isBrandIdValid = true;
                else
                    this.isBrandIdValid = false;
                break;

            case 'campaign':
                if (UtilityService.trim(subscriptionForm.value.campaign))
                    this.isCampaignValid = true;
                else
                    this.isCampaignValid = false;
                break;

            case 'seatCount':
                if (UtilityService.trim(subscriptionForm.value.seatCount) && +subscriptionForm.value.seatCount > 0
                    && +subscriptionForm.value.seatCount <= glob.MaxNumber
                )
                    this.isSeatCountValid = true;
                else
                    this.isSeatCountValid = false;
                break;

            case 'authUsername':
                if (UtilityService.trim(subscriptionForm.value.authUsername))
                    this.isAuthUsernameValid = true;
                else
                    this.isAuthUsernameValid = false;
                break;

            case 'authPassword':
                if (UtilityService.trim(subscriptionForm.value.authPassword))
                    this.isAuthPasswordValid = true;
                else
                    this.isAuthPasswordValid = false;
                break;

            case 'clientDownloadLocation':
                if (UtilityService.trim(subscriptionForm.value.clientDownloadLocation))
                    this.isClientDownloadLocationValid = true;
                else
                    this.isClientDownloadLocationValid = false;
                break;


        }

        this.validateForm();

    }

    getDate(): string {
        var date = new Date();

        var day = String(date.getDate());
        var month = String(date.getMonth() + 1);
        if (month.length == 1)
            month = '0' + month;

        var year = String(date.getFullYear());

        return day + month + year;

    }

    submitSubscription(subscriptionForm, viewModel, isValid) {

        var uniqueName = this.currentEnterprise.name.substring(0, 3) + this.selectedProduct.name.substring(0, 3) + this.getDate();

        var subscription: any =
            {
                name: uniqueName.toUpperCase(),
                enterpriseclientid: this.currentEnterprise.id,
                product: this.selectedProduct.wId,
                licencingenvironment: this.selectedLicencingEnvironment.wId,
                brandid: viewModel.brandId,
                campaign: viewModel.campaign,

                clientdownloadlocation: viewModel.clientDownloadLocation,
                coreauthusername: viewModel.authUsername,
                coreauthpassword: viewModel.authPassword,
                regauthusername: viewModel.authUsername,
                regauthpassword: viewModel.authPassword,
                seatcount: viewModel.seatCount,

            };

        this.waitingForResponse = true;

        this.subscriptionService.insertSubscription(subscription)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.toastrService.success('Subscription Added!');

                    this.getProductsAndRefreshSubscription(this.currentPageNumber);

                    this.modalService.close('subscription-insert-modal');
                } else {
                    var errorMessage = this.subscriptionService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to insert the subscription.');

                this.waitingForResponse = false;
            }
            )
            ;


    }

    cancelAdd() {
        this.modalService.close('subscription-insert-modal');

    }

    increaseSeatCountLink(subscription, increaseSeatcountForm) {

        this.selectedSubscription = subscription;

        this.isIscNewSeatcountValid = false;

        if (increaseSeatcountForm && increaseSeatcountForm.controls && increaseSeatcountForm.controls.iscNewSeatcount)
            increaseSeatcountForm.controls.iscNewSeatcount.markAsPristine();

        this.isIncreaseSeatcountFormValid = false;

        this.newSeatCount = null;
        this.alertService.reset();

        this.modalService.open('increase-seatcount-modal');

    }

    increaseSeatcount(increaseSeatcountForm) {

        this.waitingForResponse = true;

        this.subscriptionService
            .setSeatCount(this.selectedSubscription.id, +increaseSeatcountForm.value.iscNewSeatcount)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.toastrService.success('Seat count has been updated!');
                    this.getProductsAndRefreshSubscription(this.currentPageNumber);

                    this.modalService.close('increase-seatcount-modal');
                } else {
                    var errorMessage = this.subscriptionService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to increase the seat count.');

                this.waitingForResponse = false;
            }
            )
            ;


    }

    closeIncreaseSeatcountModal() {
        this.modalService.close('increase-seatcount-modal');

    }

    validateNewSeatcount(increaseSeatcountForm) {
        if (this.newSeatCount > this.selectedSubscription.seatCount && this.newSeatCount <= glob.MaxNumber) {
            this.isIscNewSeatcountValid = true;
            this.isIncreaseSeatcountFormValid = true;
        } else {
            this.isIscNewSeatcountValid = false;
            this.isIncreaseSeatcountFormValid = false;
        }

    }


    cancelSubscription(subscription: Subscription) {

        var message = "Cancelling this subscription will Expire the enterprise activation key associated with subscription " + subscription.name + " and all active seats."

        swal({
            title: 'Are you sure?',
            text: message,
            type: 'warning',
            showCancelButton: true,
            confirmButtonClass: 'btn',
            cancelButtonClass: 'btn',
            confirmButtonText: 'Yes, Cancel subscription',
            cancelButtonText: 'No',
            buttonsStyling: false
        }).then(function (result) {

            if (result.value)
                window['my']['namespace']['publicFunc'](subscription.id);

            }).catch(swal.noop);

    }

    getProductsAndRefreshSubscription(pageNumber) {
        

            this.waitingForResponse = true;

            this.lookupService.getProducts().subscribe(
                (data: ApiResult) => {

                    this.products = data.d;

                    this.getSubscriptionsWithPage(pageNumber);

                    this.waitingForResponse = false;
                },
                (err: HttpErrorResponse) => {

                    this.alertService.error('Error, failed to get products.');
                    this.waitingForResponse = false;
                }
            )
            ;

    }

    selectProduct(product) {

        this.isProductNotToched = false;
        this.selectedProduct = product;
        this.validateProduct();

    }

    getLicenceEnvironments() {


        this.waitingForResponse = true;

        this.lookupService.getLicenceEnvironments().subscribe(
            (data: ApiResult) => {

                this.licencingEnvironments = data.d;

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error, failed to get licence environments.');
                this.waitingForResponse = false;
            }
        )
            ;

    }

    selectLicenceEnvironment(licenceEnvironment) {
        this.selectedLicencingEnvironment = licenceEnvironment;
    }

    viewLicence(subscriptionId) {

        this.router.navigate(['/dashboard/licence'], { queryParams: { subscriptionId: subscriptionId } });

    }

    sendInstructions(subscription) {
        this.subscriptionService
            .sendInstructions(subscription.id)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.toastrService.success('Instructions have been sent successfully!');

                } else {
                    var errorMessage = this.subscriptionService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.toastrService.error(errorMessage);
                }

            },
            (err: HttpErrorResponse) => {

                this.toastrService.error('Error, failed to send instructions.');
            });
    }

    viewSeatDetail(subscriptionId, brandId, licenceKey, licencingEnvironment) {
        this.router.navigate(['/dashboard/seat'], { queryParams: { si: subscriptionId, bi: brandId, lk: licenceKey, le: licencingEnvironment } });
    }

}