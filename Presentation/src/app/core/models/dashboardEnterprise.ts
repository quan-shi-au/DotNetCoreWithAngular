import { Subscription } from '../../core/models/subscription';

export interface DashboardEnterprise {
    enterpriseName?: string;
    enterpriseId?: number;
    subscriptions?: Subscription[];

}