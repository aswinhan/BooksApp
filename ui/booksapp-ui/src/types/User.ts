// src/types/User.ts
export interface User {
    displayName: string;
    token: string;
    // We keep these optional in case you decode them later, 
    // but for now, they won't be populated by Login.
    email?: string; 
    role?: string;
    id?: string;
}

export interface AuthResponse {
    token: string;
    user: User; // Adjust based on your actual API response structure
}