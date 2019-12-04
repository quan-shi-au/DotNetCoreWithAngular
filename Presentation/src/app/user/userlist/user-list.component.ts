import { Component, OnInit, NgZone, OnDestroy, Inject, Injectable} from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import 'rxjs/add/operator/switchMap';

import { User } from '../../core/models/user';
import { UserRole } from '../../core/models/userRole';
import { UserFilter } from '../../core/models/userFilter';

import {UserService} from '../../core/services/user.service';

import { EnumMapping } from '../../core/models/enumMapping';
import { ToastrService } from '../../core/services/toastr.service';

import { EnterpriseService } from '../../core/services/enterprise.service';
import { Enterprise } from '../../core/models/enterprise';

import { PartnerService } from '../../core/services/partner.service';
import { Partner } from '../../core/models/partner';

import { HttpErrorResponse } from '@angular/common/http';


import { AlertService } from '../../core/services/alert.service'
import { ModalService } from '../../core/services/modal.service';
import { UtilityService } from '../../core/services/utility.service'

import * as glob from '../../core/variables/global.variable';

import { ApiResult } from '../../core/models/apiResult';
import { PageService } from '../../core/services/page.service';

import { DOCUMENT } from '@angular/platform-browser';

declare var $: any;
declare var swal: any;

@Component({
    selector: 'user-list',
    templateUrl: 'user-list.component.html',
    styleUrls: ['user-list.css']
})
export class UserListComponent implements OnInit{
    public viewModel: any[];
    public headerRow: string[] = ['Role', 'Username', 'First Name', 'Last Name', 'Associated Partner', 'Associated Enterprise', 'Status', 'Actions'];
    public waitingForResponse: boolean = false;

    public localCount: number = 0;
    public localOffset: number = 0;
    public localLimit: number = 0;  // page size

    public selectedSearchUserRole: UserRole;
    public selectedSearchRoleDescription: string;

    public currentUser: User = {};
    public selectedUserRole: UserRole;
    public selectedRoleDescription: string;
    public userRoleMappings: EnumMapping[] = [];
    public searchUserRoleMappings: EnumMapping[] = [];
    public cardTitle: string;
    public hideEnterprise: boolean = false;
    public hidePartner: boolean = false;

    public partners: Partner[];
    public searchPartners: Partner[] = [];
    public enterprises: Enterprise[];

    public searchEnterprises: Enterprise[] = [];

    public selectedPartnerName: string;
    public currentPartner: Partner;

    public selectedSearchPartnerName: string;
    public currentSearchPartner: Partner;

    public selectedEnterpriseName: string;
    public currentEnterprise: Enterprise;

    public selectedSearchEnterpriseName: string;
    public currentSearchEnterprise: Enterprise;


    public isFirstNameValid: boolean;
    public isLastNameValid: boolean;
    public isEmailValid: boolean;

    public isRoleValid: boolean;
    public isRoleNotToched: boolean = true;

    public isFormValid: boolean;
    public IsOthersValid: boolean = true;
    public otherErrorMessage: string;

    public addUserCaption = "Add User";
    public addUserTitle = "Add a new user";
    public addUserResetMargin = false;

    private subscription: any;
    public userFilter: UserFilter = {};

    private currentPageNumber: number = 1;

    constructor(
        private router: Router,
        private userService: UserService,
        private route: ActivatedRoute,
        private toastrService: ToastrService,
        private partnerService: PartnerService,
        private enterpriseService: EnterpriseService,
        private modalService: ModalService,
        private alertService: AlertService,
        private ngZone: NgZone,
        private pageService: PageService,
        @Inject(DOCUMENT) private document: any

    ) {
    }



    ngOnInit() {

        window['my'] = window['my'] || {};
        window['my']['user'] = window['my']['user'] || {};
        window['my']['user']['publicDisableUser'] = this.publicDisableUser.bind(this);
        window['my']['user']['publicDeleteUser'] = this.publicDeleteUser.bind(this);

        this.localLimit = this.pageService.getPageSize();
        this.subscription = this.pageService.pageSizeSubject.subscribe(item => { this.localLimit = item; });

        this.getUsersWithPage(1);

        this.partnerService.getPartners().subscribe(
            (data : any) => {

                this.partners = <Partner[]>data.d;

                this.searchPartners.push({ id: 0, name: '&nbsp' });
                this.searchPartners = this.searchPartners.concat(this.partners);
            },
            (err: HttpErrorResponse) => {

                this.alertService.error("Partners data can't be retrieved. ")

            }
        );

        this.userRoleMappings = this.userService.getUserRoleMappings().filter(x => x.id != UserRole.entAdmin);
        this.searchUserRoleMappings.push({ id: 0, description: '&nbsp' });
        this.searchUserRoleMappings = this.searchUserRoleMappings.concat(this.userRoleMappings);
    }

    ngOnDestroy() {
        window['my']['user']['publicDisableUser'] = null;
        window['my']['user']['publicDeleteUser'] = null;

        this.subscription.unsubscribe();
    }

    publicDisableUser(userName) {
        this.ngZone.run(() => this.privateDisableUser(userName));
    }


    privateDisableUser(userName) {

        this.waitingForResponse = true;

        this.userService.lockUser(userName)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.getUsersWithPage(this.currentPageNumber);

                    this.toastrService.success("User has been disabled.");

                } else {
                    var errorMessage = this.userService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Failed to disable user.');
            }
        );

    }

    publicDeleteUser(userName) {
        this.ngZone.run(() => this.privateDeleteUser(userName));
    }

    isItLastRecordInPage(): boolean {

        return this.localCount - (this.currentPageNumber - 1) * this.localLimit == 1;
    }

    privateDeleteUser(userName) {

        this.waitingForResponse = true;

        this.userService.deleteUser(userName)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    if (this.isItLastRecordInPage()) {
                        if (this.currentPageNumber > 1)
                            this.currentPageNumber--;
                    }

                    this.getUsersWithPage(this.currentPageNumber);

                    this.toastrService.success("User has been deleted.");

                } else {
                    var errorMessage = this.userService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Failed to delete user.');
            }
            );

    }


    closeModal() {

        this.modalService.close('user-insert-modal');
    }

    
    getUsersWithPage(pageNumber: number) {

        this.currentPageNumber = pageNumber;

        this.waitingForResponse = true;

        this.userService.getUsersWithPage(pageNumber, this.userFilter).subscribe(
            (data : any) => {

                this.viewModel = data.d;

                this.localOffset = (pageNumber - 1) * this.localLimit;
                this.localCount = data.t;

                this.viewModel.map(x => {
                    var vm: any = x;
                    x.roleDescription = this.userService.GetRoleDescrptionByServerRole(x.role);
                    return vm;
                });

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Error getting all users ');

                this.waitingForResponse = false;
            }
        );

;





    }

    selectSearchPartner(selctedPartner) {
        this.selectedSearchPartnerName = selctedPartner.name == '&nbsp' ? '' : selctedPartner.name;
        this.currentSearchPartner = selctedPartner;
        this.userFilter.partnerId = this.currentSearchPartner.id;

        this.selectedSearchEnterpriseName = '';
        this.currentSearchEnterprise = {};
        this.userFilter.enterpriseId = this.currentSearchEnterprise.id;

        this.getSearchEnterprises(this.currentSearchPartner.id);
    }

    selectPartner(selctedPartner) {
        this.selectedPartnerName = selctedPartner.name;
        this.currentPartner = selctedPartner;

        this.selectedEnterpriseName = '';
        this.currentEnterprise = {};

        this.getEnterprises(this.currentPartner.id);
    }

    getSearchEnterprises(pid: number) {

        this.waitingForResponse = true;

        this.enterpriseService.getEnterprisesForPartner(pid)
            .subscribe(
            (data: any) => {

                this.searchEnterprises = [];
                this.searchEnterprises.push({ id: 0, name: '&nbsp' });
                this.searchEnterprises = this.searchEnterprises.concat(data.d);

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {
                this.alertService.error('failed to retrieve enterprise list.');

                this.waitingForResponse = false;

            }
            );


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

    selectSearchEnterprise(selctedEnterprise) {
        this.selectedSearchEnterpriseName = selctedEnterprise.name == '&nbsp' ? '' : selctedEnterprise.name;
        this.currentSearchEnterprise = selctedEnterprise;

        this.userFilter.enterpriseId = this.currentSearchEnterprise.id;
    }


    selectEnterprise(selctedEnterprise) {
        this.selectedEnterpriseName = selctedEnterprise.name;
        this.currentEnterprise = selctedEnterprise;
    }

    validateControl(userProfileForm, controlName) {
        switch (controlName) {
            case 'firstName':
                if (UtilityService.trim(userProfileForm.value.firstName))
                    this.isFirstNameValid = true;
                else
                    this.isFirstNameValid = false;
                break;
            case 'lastName':
                if (UtilityService.trim(userProfileForm.value.lastName))
                    this.isLastNameValid = true;
                else
                    this.isLastNameValid = false;
                break;
            case 'email':
                if (UtilityService.trim(userProfileForm.value.email)) {

                    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                    var testFormat = re.test(userProfileForm.value.email.toLowerCase());

                    if (testFormat)
                        this.isEmailValid = true;
                    else
                        this.isEmailValid = false;
                }
                else
                    this.isEmailValid = false;
                break;
        }

        this.validateForm();

    }

    validateForm() {

        if (this.isFirstNameValid && this.isLastNameValid && this.isEmailValid)
            this.isFormValid = true;
        else
            this.isFormValid = false;
    }

    validateOthers() {

        this.IsOthersValid = true;

        switch (this.selectedUserRole) {
            case UserRole.PartnerAdmin:
                if (!(this.currentPartner && this.currentPartner.id)) {
                    this.IsOthersValid = false;
                    this.otherErrorMessage = 'Partner has to be selected for a partner admin.'
                }
                break;
            case UserRole.EnterpriseAdmin:
                if (!(this.currentEnterprise && this.currentEnterprise.id) ||
                    !(this.currentPartner && this.currentPartner.id)) {

                    this.IsOthersValid = false;
                    this.otherErrorMessage = 'Partner and Enterprise has to be selected for a enterprise admin.'
                }
                break;
        }

        return this.IsOthersValid;
    }

    save(userProfileForm) {

        if (!this.validateOthers())
            return;

        this.currentUser.role = this.selectedUserRole;
        this.currentUser.partner = this.currentPartner;
        this.currentUser.enterprise = this.currentEnterprise;
        this.currentUser.domain = document.location.protocol + '//' + document.location.host;

        this.waitingForResponse = true;

            this.userService.createUser(this.currentUser).subscribe(
                (data : any) => {

                    if (data.c === glob.SuccessCode) {
                        this.toastrService.success('User is added.')
                        this.getUsersWithPage(this.currentPageNumber);
                        this.modalService.close('user-insert-modal');
                    } else {
                        var errorMessage = this.userService.getServerErrorByCode(data.c);
                        if (errorMessage)
                            this.alertService.error(errorMessage);
                    }

                    this.waitingForResponse = false;
                },
                (err: HttpErrorResponse) => {

                    this.alertService.error('Error, failed to insert the user.');

                    this.waitingForResponse = false;
                }
            );


    }

    selectSearchUserRole(userRoleMapping: EnumMapping) {

        this.selectedSearchRoleDescription = userRoleMapping.description == '&nbsp' ? '' : userRoleMapping.description;
        this.selectedSearchUserRole = userRoleMapping.id;

        this.userFilter.role = userRoleMapping.id;
    }


    selectUserRole(userRoleMapping: EnumMapping) {

        this.selectedRoleDescription = userRoleMapping.description;
        this.selectedUserRole = userRoleMapping.id;

        if (userRoleMapping.id == UserRole.entAdmin) {
            this.hidePartner = true;
            this.hideEnterprise = true;
        } else if (userRoleMapping.id == UserRole.PartnerAdmin) {
            this.hidePartner = false;
            this.hideEnterprise = true;
        } else if (userRoleMapping.id == UserRole.EnterpriseAdmin) {
            this.hidePartner = false;
            this.hideEnterprise = false;
        }

        this.isRoleNotToched = false;
        if (this.selectedRoleDescription && this.selectedUserRole)
            this.isRoleValid = true;
        else
            this.isRoleValid = false;
    }

    addUser(userProfileForm) {

        this.selectedPartnerName = "";
        this.currentPartner = {};
        this.selectedEnterpriseName = "";
        this.currentEnterprise = <Enterprise>{};

        this.selectedRoleDescription = "";
        this.selectedUserRole = <UserRole>{};

        this.isFirstNameValid = false;
        this.isLastNameValid = false;
        this.isEmailValid = false;
        this.isFormValid = false;
        userProfileForm.resetForm();

        this.alertService.reset();

        this.modalService.open('user-insert-modal');


    }

    resend(user) {

            this.waitingForResponse = true;

            this.userService
                .sendWelcome(user.username)
                .subscribe(
                (data: ApiResult) => {

                    if (data.c === glob.SuccessCode) {
                        this.toastrService.success('Email has been sent to ' + user.username);
                        this.waitingForResponse = false;
                    } else {
                        var errorMessage = this.userService.getServerErrorByCode(data.c);
                        if (errorMessage)
                            this.toastrService.error(errorMessage);
                    }

                },
                (err: HttpErrorResponse) => {

                    this.toastrService.error('Error resending email to ' + user.username);

                    this.waitingForResponse = false;
                }
        );


    }


    disableEnable(user) {

        if (user.status == 'disabled')
            this.enable(user);
        else
            this.disable(user);
    }

    disable(user) {

        var message = "Disabling a user will prevent them from accessing the platform. "

        swal({
            title: 'Are you sure?',
            text: message,
            type: 'warning',
            showCancelButton: true,
            confirmButtonClass: 'btn',
            cancelButtonClass: 'btn',
            confirmButtonText: 'Yes, disable user',
            cancelButtonText: 'No',
            buttonsStyling: false
        }).then(function (result) {

            if (result.value)
                window['my']['user']['publicDisableUser'](user.username);

        }).catch(swal.noop);


    }

    enable(user) {

        this.waitingForResponse = true;

        this.userService.unlockUser(user.username)
            .subscribe(
            (data: ApiResult) => {

                if (data.c === glob.SuccessCode) {

                    this.getUsersWithPage(this.currentPageNumber);

                    this.toastrService.success("User has been enabled.");

                } else {
                    var errorMessage = this.userService.getServerErrorByCode(data.c);
                    if (errorMessage)
                        this.alertService.error(errorMessage);
                }

                this.waitingForResponse = false;

            },
            (err: HttpErrorResponse) => {

                this.alertService.error('Failed to enable user.');
            }
            );


    }



    delete(user) {

        var message = "Deleting a user will remove them completely. "

        swal({
            title: 'Are you sure?',
            text: message,
            type: 'warning',
            showCancelButton: true,
            confirmButtonClass: 'btn',
            cancelButtonClass: 'btn',
            confirmButtonText: 'Yes, delete user',
            cancelButtonText: 'No',
            buttonsStyling: false
        }).then(function (result) {

            if (result.value)
                window['my']['user']['publicDeleteUser'](user.username);

        }).catch(swal.noop);


    }

    search(searchForm) {

    }



}