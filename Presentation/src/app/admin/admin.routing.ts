import { Routes } from '@angular/router';

import { AdminReportComponent } from './report/admin-report.component';

export const AdminRoutes: Routes = [{
    path: '',
    children:
    [
        {
            path: 'report',
                component: AdminReportComponent
        }
    ]
}];
