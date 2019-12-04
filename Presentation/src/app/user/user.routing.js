"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var user_list_component_1 = require("./userlist/user-list.component");
exports.UserRoutes = [{
        path: '',
        children: [{
                path: 'list',
                component: user_list_component_1.UserListComponent
            }]
    }];
//# sourceMappingURL=user.routing.js.map