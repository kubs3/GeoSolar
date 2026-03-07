export interface LeadData {
    name: string;
    email: string;
    phone: string;
}

export interface RoofData {
    area: number;
    buildingAxis: number;
    roofPitch1: number;
    roofPitch2: number;
    orientationText: string;
    estimated_kWP: number;
    geometry?: number[][]; // [lon, lat] pairs from the backend
    message?: string;
}

export interface LeadPayload {
    name: string;
    email: string;
    phone: string;
    roofArea: number;
    orientation: string;
    estimatedKWp: number;
}
