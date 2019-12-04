import { UserRole } from "./userRole"

export interface UserFilter {
    firstName?: string;
    lastName?: string;
    userName?: string;

    enterpriseId?: number;
    partnerId?: number;

    role?: UserRole;
}