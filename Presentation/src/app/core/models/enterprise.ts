import { Partner } from '../../core/models/partner';

export interface Enterprise {
    id?: number;
    name?: string;
    location?: string;

    partner?: Partner;
    partnerId?: number;
}