export interface Book {
    id: string;
    title: string;
    authorName: string;
    description: string;
    price: number;
    imageUrl?: string; // Optional if some books don't have images
    category?: string;
}