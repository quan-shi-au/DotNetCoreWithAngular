import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PartnerService } from './services/partner.service';
import { LookupService } from './services/lookup.service';
import { SubscriptionService } from './services/subscription.service';
import { EnterpriseService } from './services/enterprise.service';
import { AuthGuard } from './services/auth.guard';
import { ScopeGuardService } from './services/scope-guard.service';
import { TokenInterceptor } from './services/token.interceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthenticationService } from './services/authentication.service';
import { UserService } from './services/user.service';
import { AlertService } from './services/alert.service'
import { ToastrService } from './services/toastr.service';
import { ModalService } from './services/modal.service';
import { UtilityService } from './services/utility.service';
import { DashboardService } from './services/dashboard.service';
import { ReportService } from './services/report.service';
import { SpinnerService } from './services/spinner.service';
import { LanguageService } from './services/language.service';
import { EnvironmentSpecificService } from './services/environmentSpecific.service';
import { EnvironmentSpecificResolver } from './services/environmentSpecific.resolver';
import { LicenceService } from './services/licence.service';
import { PageService } from './services/page.service';
import { AesService } from './services/aes.service';
import { AdminReportService } from './services/adminreport.service';
import { DeviceeventsService } from './services/deviceevents.service';


@NgModule({
    imports: [
        CommonModule
    ],
    providers: [
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
        LanguageService,
        LicenceService,
        PageService,
        AesService,
        AdminReportService,
        DeviceeventsService
  ]
})

export class CoreModule {}
