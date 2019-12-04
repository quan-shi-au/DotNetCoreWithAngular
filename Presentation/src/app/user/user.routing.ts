import { Routes } from '@angular/router';

import { UserListComponent } from './userlist/user-list.component';

export const UserRoutes: Routes = [{
    path: '',
    children: [ {
            path: 'list',
            component: UserListComponent
        }]
}];
