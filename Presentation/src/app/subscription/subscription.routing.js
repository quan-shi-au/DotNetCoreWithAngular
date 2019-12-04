"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var subscription_list_component_1 = require("./list/subscription-list.component");
exports.SubscriptionRoutes = [{
        path: '',
        children: [
            {
                path: 'list',
                component: subscription_list_component_1.SubscriptionListComponent
            }
        ]
    }];
//# sourceMappingURL=subscription.routing.js.map