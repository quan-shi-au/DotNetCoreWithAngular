"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Observable_1 = require("rxjs/Observable");
var platform_browser_1 = require("@angular/platform-browser");
var http_1 = require("@angular/http");
var http_2 = require("@angular/common/http");
var core_1 = require("@ngx-translate/core");
var http_loader_1 = require("@ngx-translate/http-loader");
var common_1 = require("@angular/common");
var forms_1 = require("@angular/forms");
var lookup_service_1 = require("../../core/services/lookup.service");
var subscription_service_1 = require("../../core/services/subscription.service");
var enterprise_service_1 = require("../../core/services/enterprise.service");
var auth_guard_1 = require("../../core/services/auth.guard");
var scope_guard_service_1 = require("../../core/services/scope-guard.service");
var token_interceptor_1 = require("../../core/services/token.interceptor");
var http_3 = require("@angular/common/http");
var authentication_service_1 = require("../../core/services/authentication.service");
var user_service_1 = require("../../core/services/user.service");
var dashboard_service_1 = require("../../core/services/dashboard.service");
var report_service_1 = require("../../core/services/report.service");
var spinner_service_1 = require("../../core/services/spinner.service");
var language_service_1 = require("../../core/services/language.service");
var environmentSpecific_service_1 = require("../../core/services/environmentSpecific.service");
var environmentSpecific_resolver_1 = require("../../core/services/environmentSpecific.resolver");
var app_component_1 = require("../../app.component");
var sidebar_module_1 = require("../../sidebar/sidebar.module");
var footer_module_1 = require("../../shared/footer/footer.module");
var navbar_module_1 = require("../../shared/navbar/navbar.module");
var admin_layout_component_1 = require("../../layouts/admin/admin-layout.component");
var auth_layout_component_1 = require("../../layouts/auth/auth-layout.component");
var ngx_loading_1 = require("ngx-loading");
var testing_1 = require("@angular/core/testing");
var platform_browser_2 = require("@angular/platform-browser");
var partner_service_1 = require("../../core/services/partner.service");
var alert_service_1 = require("../../core/services/alert.service");
var toastr_service_1 = require("../../core/services/toastr.service");
var utility_service_1 = require("../../core/services/utility.service");
var modal_service_1 = require("../../core/services/modal.service");
var partner_list_component_1 = require("./partner-list.component");
var router_1 = require("@angular/router");
var common_2 = require("@angular/common");
var alert_module_1 = require("../../shared/alert/alert.module");
var pagination_module_1 = require("../../shared/pagination/pagination.module");
var modal_module_1 = require("../../shared/simplemodal/modal.module");
var modalheader_module_1 = require("../../shared/modalheader/modalheader.module");
var app_routing_1 = require("../../app.routing");
var PartnerServiceStub = {
    getPartners: function () {
        return Observable_1.Observable.of({
            c: '01',
            d: [{ name: 'partner1', location: 'location1' }, { name: 'partner2', location: 'location2' }]
        });
    }
};
function HttpLoaderFactory(http) {
    return new http_loader_1.TranslateHttpLoader(http);
    //return new TranslateHttpLoader(http, 'public/assets/i18n', '.json')
}
exports.HttpLoaderFactory = HttpLoaderFactory;
describe('partner list component', function () {
    var comp;
    var fixture;
    var de;
    var el;
    var partnerLinesDe;
    var partnerLinesNe;
    var spyInsertPartner;
    beforeEach(testing_1.async(function () {
        testing_1.TestBed.configureTestingModule({
            imports: [
                common_2.CommonModule,
                router_1.RouterModule.forRoot(app_routing_1.AppRoutes),
                forms_1.FormsModule,
                alert_module_1.AlertModule,
                pagination_module_1.PaginationModule,
                modal_module_1.ModalModule,
                modalheader_module_1.ModalheaderModule,
                platform_browser_1.BrowserModule,
                forms_1.FormsModule,
                router_1.RouterModule.forRoot(app_routing_1.AppRoutes),
                http_1.HttpModule,
                http_2.HttpClientModule,
                sidebar_module_1.SidebarModule,
                navbar_module_1.NavbarModule,
                footer_module_1.FooterModule,
                core_1.TranslateModule.forRoot({
                    loader: {
                        provide: core_1.TranslateLoader,
                        useFactory: HttpLoaderFactory,
                        deps: [http_2.HttpClient]
                    }
                }),
                ngx_loading_1.LoadingModule
            ],
            declarations: [
                partner_list_component_1.PartnerListComponent,
                app_component_1.AppComponent,
                admin_layout_component_1.AdminLayoutComponent,
                auth_layout_component_1.AuthLayoutComponent,
            ],
            providers: [
                { provide: common_1.APP_BASE_HREF, useValue: '/' },
                auth_guard_1.AuthGuard,
                scope_guard_service_1.ScopeGuardService,
                {
                    provide: http_3.HTTP_INTERCEPTORS,
                    useClass: token_interceptor_1.TokenInterceptor,
                    multi: true
                },
                partner_service_1.PartnerService,
                subscription_service_1.SubscriptionService,
                enterprise_service_1.EnterpriseService,
                authentication_service_1.AuthenticationService,
                user_service_1.UserService,
                alert_service_1.AlertService,
                toastr_service_1.ToastrService,
                modal_service_1.ModalService,
                utility_service_1.UtilityService,
                lookup_service_1.LookupService,
                dashboard_service_1.DashboardService,
                spinner_service_1.SpinnerService,
                report_service_1.ReportService,
                environmentSpecific_service_1.EnvironmentSpecificService,
                environmentSpecific_resolver_1.EnvironmentSpecificResolver,
                language_service_1.LanguageService
            ]
        }).compileComponents();
    }));
    beforeEach(function () {
        fixture = testing_1.TestBed.createComponent(partner_list_component_1.PartnerListComponent);
        comp = fixture.componentInstance;
        de = fixture.debugElement.query(platform_browser_2.By.css('h4'));
        el = de.nativeElement;
        var partnerSevice = fixture.debugElement.injector.get(partner_service_1.PartnerService);
        var spy = spyOn(partnerSevice, 'getPartners').and
            .returnValue(Observable_1.Observable.of({
            c: '01',
            d: [{ name: 'partner1', location: 'location1' }, { name: 'partner2', location: 'location2' }]
        }));
        spyInsertPartner = spyOn(partnerSevice, 'insertPartner');
    });
    it('h4 is list', function () {
        expect(el.textContent).toContain('List');
    });
    it('there are 2 partners', testing_1.async(function () {
        fixture.detectChanges();
        fixture.whenStable().then(function () {
            fixture.detectChanges();
            partnerLinesDe = fixture.debugElement.queryAll(platform_browser_2.By.css('table.manager-table-list tbody tr'));
            expect(partnerLinesDe.length).toEqual(2);
        });
    }));
    it('Create a new partner', testing_1.async(function () {
        var deName = fixture.debugElement.query(platform_browser_2.By.css('#name'));
        deName.nativeElement.value = 'name1';
        var deLocation = fixture.debugElement.query(platform_browser_2.By.css('#location'));
        deLocation.nativeElement.value = 'location1';
        var insertButton = fixture.debugElement.query(platform_browser_2.By.css('button[type=submit]')).nativeElement;
        insertButton.click();
        //insertButton.triggerEventHandler('submitPartner', null);
        //insertButton.dispatchEvent(new Event('click'));
        fixture.detectChanges();
        fixture.whenStable().then(function () {
            fixture.detectChanges();
            expect(spyInsertPartner.calls.any()).toBe(true, 'insertPartner should be called');
        });
    }));
});
//# sourceMappingURL=partner-list.spec.js.map