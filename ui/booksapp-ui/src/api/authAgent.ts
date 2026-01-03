import type { User } from '../types/User';

const API_URL = import.meta.env.VITE_API_URL;

export const AuthAgent = {
    login: async (email: string, password: string): Promise<User> => {
        const response = await fetch(`${API_URL}/api/users/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password }),
        });

        if (!response.ok) {
            let errorMsg = 'Login failed';
            try {
                const error = await response.json();
                errorMsg = error.detail || error.title || errorMsg;
            } catch { /* ignore json parse error */ }
            throw new Error(errorMsg);
        }

        return response.json();
    },

    register: async (email: string, password: string, displayName: string, role: string = "User"): Promise<User> => {
        const response = await fetch(`${API_URL}/api/users/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                email, 
                password, 
                displayName, 
                role 
            }),
        });

        if (!response.ok) {
            let errorMsg = 'Registration failed';
            try {
                const error = await response.json();
                errorMsg = error.detail || error.title || errorMsg;
            } catch { /* ignore json parse error */ }
            throw new Error(errorMsg);
        }

        return response.json();
    }
};