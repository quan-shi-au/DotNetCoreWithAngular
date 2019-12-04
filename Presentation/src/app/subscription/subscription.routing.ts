import { Routes } from '@angular/router';

import { SubscriptionListComponent } from './list/subscription-list.component';

export const SubscriptionRoutes: Routes = [{
    path: '',
    children:
    [
        {
            path: 'list',
            component: SubscriptionListComponent
        }
    ]
}];
