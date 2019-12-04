import { UserRole } from "./userRole"
import { Partner } from "./partner"
import { Enterprise } from "./enterprise"

export interface User {
    id?: number;
    firstName?: string;
    lastName?: string;
    password?: string;
    email?: string;
    role?: UserRole;
    contactNumber?: string;

    partner?: Partner;
    enterprise?: Enterprise;
    userId?: string;
    domain?: string;
}