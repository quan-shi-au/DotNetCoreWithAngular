import { Routes } from '@angular/router';

import { PartnerListComponent } from './partnerlist/partner-list.component';

export const PartnerRoutes: Routes = [{
    path: '',
    children:
    [
        {
            path: 'list',
            component: PartnerListComponent
        }
    ]
}];
