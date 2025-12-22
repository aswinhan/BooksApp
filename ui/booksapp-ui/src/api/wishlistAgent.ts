// src/api/wishlistAgent.ts
const apiUrl = import.meta.env.VITE_API_URL;

export const WishlistAgent = {
    // Get all items
    getWishlist: async (userId: string) => {
        const response = await fetch(`${apiUrl}/api/wishlist/${userId}`);
        if (!response.ok) throw new Error('Failed to fetch wishlist');
        return response.json();
    },

    // Remove item
    removeItem: async (userId: string, bookId: string) => {
        const response = await fetch(`${apiUrl}/api/wishlist/${userId}/items/${bookId}`, {
            method: 'DELETE'
        });
        if (!response.ok) throw new Error('Failed to remove item');
    }
};