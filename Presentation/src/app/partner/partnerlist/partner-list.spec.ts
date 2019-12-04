
import { Observable } from 'rxjs/Observable';

import { BrowserModule } from '@angular/platform-browser';
import { HttpModule, Http } from '@angular/http';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader, TranslatePipe, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

import { APP_BASE_HREF } from '@angular/common';
import { FormsModule } from '@angular/forms';


import { LookupService } from '../../core/services/lookup.service';
import { SubscriptionService } from '../../core/services/subscription.service';
import { EnterpriseService } from '../../core/services/enterprise.service';
import { AuthGuard } from '../../core/services/auth.guard';
import { ScopeGuardService } from '../../core/services/scope-guard.service';
import { TokenInterceptor } from '../../core/services/token.interceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthenticationService } from '../../core/services/authentication.service';
import { UserService } from '../../core/services/user.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { ReportService } from '../../core/services/report.service';
import { SpinnerService } from '../../core/services/spinner.service';
import { LanguageService } from '../../core/services/language.service';
import { EnvironmentSpecificService } from '../../core/services/environmentSpecific.service';
import { EnvironmentSpecificResolver } from '../../core/services/environmentSpecific.resolver';


import { AppComponent } from '../../app.component';

import { SidebarModule } from '../../sidebar/sidebar.module';
import { FooterModule } from '../../shared/footer/footer.module';
import { NavbarModule } from '../../shared/navbar/navbar.module';

import { CoreModule } from '../../core/core.module';


import { AdminLayoutComponent } from '../../layouts/admin/admin-layout.component';
import { AuthLayoutComponent } from '../../layouts/auth/auth-layout.component';

import { LoadingModule } from 'ngx-loading';

import { ComponentFixture, TestBed, ComponentFixtureAutoDetect, async } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { Component, OnInit, AfterViewInit, HostListener, DebugElement } from '@angular/core';

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

import { PartnerListComponent } from './partner-list.component';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { AlertModule } from '../../shared/alert/alert.module';
import { PaginationModule } from '../../shared/pagination/pagination.module';
import { ModalModule } from '../../shared/simplemodal/modal.module';
import { ModalheaderModule } from '../../shared/modalheader/modalheader.module';

import { PartnerRoutes } from '../partner.routing';
import { AppRoutes } from '../../app.routing';

let PartnerServiceStub = {
    getPartners: function () {
        return Observable.of({
            c: '01',
            d: [{ name: 'partner1', location: 'location1' }, { name: 'partner2', location: 'location2' }]
        });
    }
};

export function HttpLoaderFactory(http: HttpClient) {
    return new TranslateHttpLoader(http);

    //return new TranslateHttpLoader(http, 'public/assets/i18n', '.json')
}


describe('partner list component', () => {
    let comp: PartnerListComponent;
    let fixture: ComponentFixture<PartnerListComponent>;
    let de: DebugElement;
    let el: HTMLElement;
    var partnerLinesDe: any;
    var partnerLinesNe: any;
    let spyInsertPartner: any;

    beforeEach(async(() => {

        TestBed.configureTestingModule({
            imports: [
                CommonModule,
                RouterModule.forRoot(AppRoutes),
                FormsModule,
                AlertModule,
                PaginationModule,
                ModalModule,
                ModalheaderModule,
                BrowserModule,
                FormsModule,
                RouterModule.forRoot(AppRoutes),
                HttpModule,
                HttpClientModule,
                SidebarModule,
                NavbarModule,
                FooterModule,
                TranslateModule.forRoot({
                    loader: {
                        provide: TranslateLoader,
                        useFactory: HttpLoaderFactory,
                        deps: [HttpClient]
                    }
                }),
                LoadingModule

            ],
            declarations: [
                PartnerListComponent,
                AppComponent,
                AdminLayoutComponent,
                AuthLayoutComponent,
            ],
            providers: [
                { provide: APP_BASE_HREF, useValue: '/' },
                AuthGuard,
                ScopeGuardService,
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: TokenInterceptor,
                    multi: true
                },
                PartnerService,
                SubscriptionService,
                EnterpriseService,
                AuthenticationService,
                UserService,
                AlertService,
                ToastrService,
                ModalService,
                UtilityService,
                LookupService,
                DashboardService,
                SpinnerService,
                ReportService,
                EnvironmentSpecificService,
                EnvironmentSpecificResolver,
                LanguageService
            ]
        }).compileComponents();
    }));

    beforeEach(() => {

        fixture = TestBed.createComponent(PartnerListComponent);

        comp = fixture.componentInstance;

        de = fixture.debugElement.query(By.css('h4'));
        el = de.nativeElement;

        var partnerSevice = fixture.debugElement.injector.get(PartnerService);
        var spy = spyOn(partnerSevice, 'getPartners').and
            .returnValue(Observable.of({
                c: '01',
                d: [{ name: 'partner1', location: 'location1' }, { name: 'partner2', location: 'location2' }]
            }));

        spyInsertPartner = spyOn(partnerSevice, 'insertPartner');

    });

    it('h4 is list', () => {
        expect(el.textContent).toContain('List');
    });

    it('there are 2 partners', async(() => {

        fixture.detectChanges();
        fixture.whenStable().then(() => {
            fixture.detectChanges();

            partnerLinesDe = fixture.debugElement.queryAll(By.css('table.manager-table-list tbody tr'));
            expect(partnerLinesDe.length).toEqual(2);
        });

    }));

    it('Create a new partner', async(() => {

        var deName = fixture.debugElement.query(By.css('#name'));
        deName.nativeElement.value = 'name1';

        var deLocation = fixture.debugElement.query(By.css('#location'));
        deLocation.nativeElement.value = 'location1';

        var insertButton = fixture.debugElement.query(By.css('button[type=submit]')).nativeElement;

        insertButton.click();
        //insertButton.triggerEventHandler('submitPartner', null);
        //insertButton.dispatchEvent(new Event('click'));

        fixture.detectChanges();

        fixture.whenStable().then(() => {

            fixture.detectChanges();

            expect(spyInsertPartner.calls.any()).toBe(true, 'insertPartner should be called');
        });

    }));

});
