import { Component, OnInit, ElementRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Location, LocationStrategy, PathLocationStrategy } from '@angular/common';
import {HttpErrorResponse} from '@angular/common/http';


import { AuthenticationService } from '../../core/services/authentication.service';

import * as glob from '../../core/variables/global.variable';

import { AlertService } from '../../core/services/alert.service'
import { LanguageService } from '../../core/services/language.service'
import { UserService } from '../../core/services/user.service'
import { UserRole } from '../../core/models/userRole'
import { ApiResult } from "../../core/models/apiResult";
import { TranslateService } from '@ngx-translate/core';

declare var $:any;

@Component({
    moduleId:module.id,
    selector: 'login-cmp',
    templateUrl: './login.component.html',
    styleUrls: ['login.css']
})

export class LoginComponent implements OnInit{
    test : Date = new Date();
    public toggleButton;
    public sidebarVisible: boolean;
    public nativeElement: Node;
    model: any = {};

    public waitingForResponse: boolean = false;
    public returnUrl: string;

    public languages: any = [{
        id: glob.LanguageIdChineseSimplified,
        description: '中文简体'
        }, {
            id: glob.LanguageIdChineseTraditional,
            description: '中文繁體'
    }, {
        id: glob.LanguageIdKorean,
        description: '한국어'
    },
    {
        id: glob.LanguageIdEnglish,
        description: 'English'
    }    ];

    public selectedLanguageDescription: string;
    public selectedLanguage: any;

    constructor(
        private element: ElementRef,
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService,
        private alertService: AlertService,
        private userService: UserService,
        private languageService: LanguageService,
        private translateService: TranslateService,

    ) {

        this.nativeElement = element.nativeElement;
        this.sidebarVisible = false;
    }
    checkFullPageBackgroundImage(){
        var $page = $('.full-page');
        var image_src = $page.data('image');

        if(image_src !== undefined){
            var image_container = '<div class="full-page-background" style="background-image: url(' + image_src + ') "/>'
            $page.append(image_container);
        }
    };

    ngOnInit() {

        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'];

        this.checkFullPageBackgroundImage();

        var navbar : HTMLElement = this.element.nativeElement;
        this.toggleButton = navbar.getElementsByClassName('navbar-toggle')[0];

        setTimeout(function () {
            // after 1000 ms we add the class animated to the login/register card
            $('.card').removeClass('card-hidden');
        }, 700);

        this.initializeLanguage();
    }

    initializeLanguage() {
        var storedLanguageId = this.languageService.getLanguageId();
        if (storedLanguageId) {
            var selectedLanguages = this.languages.filter(x => x.id == storedLanguageId);
            if (selectedLanguages && selectedLanguages[0]) {
                this.selectLanguage(selectedLanguages[0]);
            } else {
                this.selectEnglish();
            }
        } else {
            this.selectEnglish();
        }
    }

    selectEnglish() {
        var englishLanguage = this.languages.filter(x => x.id == glob.LanguageIdEnglish);
        this.selectLanguage(englishLanguage);
    }

    selectLanguage(language) {
        this.selectedLanguage = language;
        this.selectedLanguageDescription = language.description;
        this.languageService.storeLanguageId(this.selectedLanguage.id);
        this.languageService.configureLanguage(this.selectedLanguage.id);
    }

    login() {
        
        var inValidUserMsg = this.translateService.instant("PagesLogin.NotValidUserNamePassword");
        var disabledErrorMsg = this.translateService.instant("PagesLogin.AccountDisabled");

        this.waitingForResponse = true;

        this.authenticationService.login(this.model.username, this.model.password).subscribe(
            (data: ApiResult) => {

                if (this.authenticationService.IsLoggedIn()) {
                    this.redirect();
                } else if (data) {
                    if (data.c && data.c == '02') 
                        this.alertService.error(disabledErrorMsg);
                    else if (data.c && data.c == '03') 
                        this.alertService.error(inValidUserMsg);
                    else
                        this.alertService.error(inValidUserMsg);
                } else {
                    this.alertService.error(inValidUserMsg);
                }

                this.waitingForResponse = false;
            },
            (err: HttpErrorResponse) => {

                if (err && err.status && err.status == 400) {
                    this.alertService.error(inValidUserMsg);
                } else {
                    // err.status == 0 when server returns bad request, thus same error message.
                    this.alertService.error(inValidUserMsg);
                }

                this.waitingForResponse = false;
            }
        );

    }

    redirect() {
        if (this.authenticationService.IsLoggedIn()) {

            if (this.returnUrl) {
                this.router.navigateByUrl(this.returnUrl);
            } else {
                var currentUser = this.userService.getCurrentUser();
                this.router.navigateByUrl('/dashboard/overview');
            }
        }
        else
            this.alertService.error('Wrong user id or password');

    }

    sidebarToggle(){
        var toggleButton = this.toggleButton;
        var body = document.getElementsByTagName('body')[0];
        var sidebar = document.getElementsByClassName('navbar-collapse')[0];
        if(this.sidebarVisible == false){
            setTimeout(function(){
                toggleButton.classList.add('toggled');
            },500);
            body.classList.add('nav-open');
            this.sidebarVisible = true;
        } else {
            this.toggleButton.classList.remove('toggled');
            this.sidebarVisible = false;
            body.classList.remove('nav-open');
        }
    }
}
