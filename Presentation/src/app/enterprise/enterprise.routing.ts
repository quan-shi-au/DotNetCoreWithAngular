import { Routes } from '@angular/router';

import { EnterpriseListComponent } from './list/enterprise-list.component';

export const EnterpriseRoutes: Routes = [{
    path: '',
    children:
    [
        {
            path: 'list',
            component: EnterpriseListComponent
        }
    ]
}];
