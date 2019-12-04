import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginationModule } from '../shared/pagination/pagination.module';
import { ModalModule } from '../shared/simplemodal/modal.module';

import { AlertModule } from '../shared/alert/alert.module';
import { SearchbarModule } from '../shared/searchbar/searchbar.module';
import { PiechartModule } from '../shared/piechart/piechart.module';

import { OverviewComponent } from './overview/overview.component';
import { LicenceComponent } from './licence/licence.component';
import { SeatComponent } from './seat/seat.component';
import { DeviceeventsAndroidComponent } from './deviceeventsandroid/deviceeventsandroid.component';
import { DeviceeventsIosComponent } from './deviceeventsios/deviceeventsios.component';
import { DashboardRoutes } from './dashboard.routing';

import { TranslateModule } from '@ngx-translate/core';

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(DashboardRoutes),
        FormsModule,
        PaginationModule,
        ModalModule,
        AlertModule,
        SearchbarModule,
        PiechartModule,
        TranslateModule,
    ],
    declarations: [
        OverviewComponent,
        LicenceComponent,
        SeatComponent,
        DeviceeventsAndroidComponent,
        DeviceeventsIosComponent
    ]
})

export class DashboardModule {}
