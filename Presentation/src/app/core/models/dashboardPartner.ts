import { DashboardEnterprise } from '../../core/models/dashboardEnterprise';

export interface DashboardPartner {
    partnerName?: string;
    partnerId?: number;
    dashboardEnterprises? : DashboardEnterprise[];
}