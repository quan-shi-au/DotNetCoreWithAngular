import { Partner } from '../../core/models/partner';
import { Enterprise } from './enterprise';

export interface Subscription {
    id?: number;
    name?: string;
    partner?: Partner;
    enterprise?: Enterprise;

    product?: number;
    productName?: string;
    licencingEnvironment?: string;
    brandId?: string;
    campaign?: string;
    seatCount?: number;
    authUsername?: string;
    authPassword?: string;
    clientDownloadLocation?: string;
    created?: Date;
    status?: string;
    creationTime?: Date;
    licenceKey?: string;

}