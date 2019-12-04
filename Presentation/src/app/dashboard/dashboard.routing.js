"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var overview_component_1 = require("./overview/overview.component");
var licence_component_1 = require("./licence/licence.component");
var seat_component_1 = require("./seat/seat.component");
var deviceeventsandroid_component_1 = require("./deviceeventsandroid/deviceeventsandroid.component");
var deviceeventsios_component_1 = require("./deviceeventsios/deviceeventsios.component");
var auth_guard_1 = require("../core/services/auth.guard");
exports.DashboardRoutes = [{
        path: '',
        children: [{
                path: 'overview',
                component: overview_component_1.OverviewComponent,
                canActivate: [auth_guard_1.AuthGuard]
            }, {
                path: 'licence',
                component: licence_component_1.LicenceComponent,
                canActivate: [auth_guard_1.AuthGuard]
            }, {
                path: 'seat',
                component: seat_component_1.SeatComponent,
                canActivate: [auth_guard_1.AuthGuard]
            }, {
                path: 'deviceeventsandroid',
                component: deviceeventsandroid_component_1.DeviceeventsAndroidComponent,
                canActivate: [auth_guard_1.AuthGuard]
            }, {
                path: 'deviceeventsios',
                component: deviceeventsios_component_1.DeviceeventsIosComponent,
                canActivate: [auth_guard_1.AuthGuard]
            }
        ]
    }];
//# sourceMappingURL=dashboard.routing.js.map