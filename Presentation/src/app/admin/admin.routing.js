"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var admin_report_component_1 = require("./report/admin-report.component");
exports.AdminRoutes = [{
        path: '',
        children: [
            {
                path: 'report',
                component: admin_report_component_1.AdminReportComponent
            }
        ]
    }];
//# sourceMappingURL=admin.routing.js.map