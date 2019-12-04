"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var partner_list_component_1 = require("./partnerlist/partner-list.component");
exports.PartnerRoutes = [{
        path: '',
        children: [
            {
                path: 'list',
                component: partner_list_component_1.PartnerListComponent
            }
        ]
    }];
//# sourceMappingURL=partner.routing.js.map