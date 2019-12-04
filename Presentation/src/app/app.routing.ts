import { Routes } from '@angular/router';

import { AdminLayoutComponent } from './layouts/admin/admin-layout.component';
import { AuthLayoutComponent } from './layouts/auth/auth-layout.component';

import { AuthGuard } from './core/services/auth.guard';
import { ScopeGuardService } from './core/services/scope-guard.service';
import { EnvironmentSpecificResolver } from './core/services/environmentSpecific.resolver';

export const AppRoutes: Routes = [{
        path: '',
        redirectTo: 'dashboard/overview',
        pathMatch: 'full'
    },{
        path: '',
        component: AdminLayoutComponent,
        resolve: { envSpecific: EnvironmentSpecificResolver },
        children: [
        {
            path: 'dashboard',
            loadChildren: './dashboard/dashboard.module#DashboardModule',
            canActivate: [AuthGuard, ScopeGuardService],
            data: { expectedScopes: ['Enterprise Admin'] }
        },{
            path: 'partner',
            loadChildren: './partner/partner.module#PartnerModule',
            canActivate: [AuthGuard, ScopeGuardService],
            data: { expectedScopes: ['ent Admin'] }
        }, {
                path: 'enterprise',
                loadChildren: './enterprise/enterprise.module#EnterpriseModule',
                canActivate: [AuthGuard, ScopeGuardService],
                data: { expectedScopes: ['Partner Admin'] }
        }, {
            path: 'subscription',
            loadChildren: './subscription/subscription.module#SubscriptionModule',
            canActivate: [AuthGuard, ScopeGuardService],
            data: { expectedScopes: ['Enterprise Admin'] }
        }, {
            path: 'user',
            loadChildren: './user/user.module#UserModule',
            canActivate: [AuthGuard, ScopeGuardService],
            data: { expectedScopes: ['Enterprise Admin'] }
        }, {
            path: 'admin',
            loadChildren: './admin/admin.module#AdminModule',
            canActivate: [AuthGuard, ScopeGuardService],
            data: { expectedScopes: ['Enterprise Admin'] }
        }]
        },{
            path: '',
            component: AuthLayoutComponent,
            children: [{
                path: 'pages',
                loadChildren: './pages/pages.module#PagesModule'
            }],
            resolve: { envSpecific: EnvironmentSpecificResolver }
        }
];
