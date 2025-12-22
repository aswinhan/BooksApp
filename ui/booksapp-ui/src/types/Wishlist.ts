// src/types/Wishlist.ts
export interface WishlistItem {
    bookId: string;
    title: string;
    price: number;
    imageUrl?: string;
    inStock: boolean; // We'll need this logic later, assume true for now
}