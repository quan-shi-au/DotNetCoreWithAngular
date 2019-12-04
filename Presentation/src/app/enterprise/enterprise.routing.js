"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var enterprise_list_component_1 = require("./list/enterprise-list.component");
exports.EnterpriseRoutes = [{
        path: '',
        children: [
            {
                path: 'list',
                component: enterprise_list_component_1.EnterpriseListComponent
            }
        ]
    }];
//# sourceMappingURL=enterprise.routing.js.map