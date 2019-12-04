import { Routes } from '@angular/router';

import { OverviewComponent } from './overview/overview.component';
import { LicenceComponent } from './licence/licence.component';
import { SeatComponent } from './seat/seat.component';
import { DeviceeventsAndroidComponent } from './deviceeventsandroid/deviceeventsandroid.component';
import { DeviceeventsIosComponent } from './deviceeventsios/deviceeventsios.component';
import { AuthGuard } from '../core/services/auth.guard';

export const DashboardRoutes: Routes = [{
    path: '',
    children: [{
        path: 'overview',
        component: OverviewComponent,
        canActivate: [AuthGuard]
    }, {
        path: 'licence',
        component: LicenceComponent,
        canActivate: [AuthGuard]
    }, {
        path: 'seat',
        component: SeatComponent,
        canActivate: [AuthGuard]
    }, {
        path: 'deviceeventsandroid',
        component: DeviceeventsAndroidComponent,
        canActivate: [AuthGuard]
    }, {
        path: 'deviceeventsios',
        component: DeviceeventsIosComponent,
        canActivate: [AuthGuard]
        }

    ]
}];
