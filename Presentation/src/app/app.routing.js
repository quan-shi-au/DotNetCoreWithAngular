"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var admin_layout_component_1 = require("./layouts/admin/admin-layout.component");
var auth_layout_component_1 = require("./layouts/auth/auth-layout.component");
var auth_guard_1 = require("./core/services/auth.guard");
var scope_guard_service_1 = require("./core/services/scope-guard.service");
var environmentSpecific_resolver_1 = require("./core/services/environmentSpecific.resolver");
exports.AppRoutes = [{
        path: '',
        redirectTo: 'dashboard/overview',
        pathMatch: 'full'
    }, {
        path: '',
        component: admin_layout_component_1.AdminLayoutComponent,
        resolve: { envSpecific: environmentSpecific_resolver_1.EnvironmentSpecificResolver },
        children: [
            {
                path: 'dashboard',
                loadChildren: './dashboard/dashboard.module#DashboardModule',
                canActivate: [auth_guard_1.AuthGuard, scope_guard_service_1.ScopeGuardService],
                data: { expectedScopes: ['Enterprise Admin'] }
            }, {
                path: 'partner',
                loadChildren: './partner/partner.module#PartnerModule',
                canActivate: [auth_guard_1.AuthGuard, scope_guard_service_1.ScopeGuardService],
                data: { expectedScopes: ['ent Admin'] }
            }, {
                path: 'enterprise',
                loadChildren: './enterprise/enterprise.module#EnterpriseModule',
                canActivate: [auth_guard_1.AuthGuard, scope_guard_service_1.ScopeGuardService],
                data: { expectedScopes: ['Partner Admin'] }
            }, {
                path: 'subscription',
                loadChildren: './subscription/subscription.module#SubscriptionModule',
                canActivate: [auth_guard_1.AuthGuard, scope_guard_service_1.ScopeGuardService],
                data: { expectedScopes: ['Enterprise Admin'] }
            }, {
                path: 'user',
                loadChildren: './user/user.module#UserModule',
                canActivate: [auth_guard_1.AuthGuard, scope_guard_service_1.ScopeGuardService],
                data: { expectedScopes: ['Enterprise Admin'] }
            }, {
                path: 'admin',
                loadChildren: './admin/admin.module#AdminModule',
                canActivate: [auth_guard_1.AuthGuard, scope_guard_service_1.ScopeGuardService],
                data: { expectedScopes: ['Enterprise Admin'] }
            }
        ]
    }, {
        path: '',
        component: auth_layout_component_1.AuthLayoutComponent,
        children: [{
                path: 'pages',
                loadChildren: './pages/pages.module#PagesModule'
            }],
        resolve: { envSpecific: environmentSpecific_resolver_1.EnvironmentSpecificResolver }
    }
];
//# sourceMappingURL=app.routing.js.map