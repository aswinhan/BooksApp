// src/pages/Wishlist.tsx
import { useEffect, useState } from 'react';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';
import type { WishlistItem } from '../types/Wishlist';
// import { WishlistAgent } from '../api/wishlistAgent'; 

const Wishlist = () => {
    // Dummy Data for now (Until you confirm you have a User ID to pass to the API)
    // Once Auth is fully linked, we swap this with the useEffect fetch below.
    const [items, setItems] = useState<WishlistItem[]>([
        { bookId: '1', title: 'Simple Things You To Save Book', price: 30.00, inStock: true, imageUrl: '/assets/img/shop-cart/01.png' },
        { bookId: '2', title: 'Qple GPad With Retina Display', price: 39.00, inStock: true, imageUrl: '/assets/img/shop-cart/02.png' },
        { bookId: '3', title: 'Flovely and Unicom Erna', price: 19.00, inStock: false, imageUrl: '/assets/img/shop-cart/03.png' }
    ]);

    const [loading, setLoading] = useState(false); // Set to true when enabling API
    const [error, setError] = useState('');

    /* // UNCOMMENT THIS WHEN AUTH IS READY
    useEffect(() => {
        const fetchWishlist = async () => {
             // Replace 'test-user-id' with actual logged in user id
            try {
                const data = await WishlistAgent.getWishlist('test-user-id');
                setItems(data.items);
                setLoading(false);
            } catch (err: any) {
                setError(err.message);
                setLoading(false);
            }
        };
        fetchWishlist();
    }, []); 
    */

    const handleRemove = async (bookId: string) => {
        // Optimistic UI Update: Remove it from screen immediately
        setItems(items.filter(i => i.bookId !== bookId));
        
        // await WishlistAgent.removeItem('test-user-id', bookId);
        console.log(`Removed book ${bookId}`);
    };

    return (
        <Layout>
            <Breadcrumb title="Wishlist" activePage="Wishlist" />

            <div className="cart-section section-padding">
                <div className="container">
                    <div className="main-cart-wrapper">
                        <div className="row">
                            <div className="col-12">
                                <div className="table-responsive">
                                    <table className="table">
                                        <thead>
                                            <tr>
                                                <th>Product</th>
                                                <th>Price</th>
                                                <th>Stock</th>
                                                <th>Subtotal</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {loading && <tr><td colSpan={4} className="text-center p-4">Loading wishlist...</td></tr>}
                                            
                                            {error && <tr><td colSpan={4} className="text-center p-4 text-danger">{error}</td></tr>}

                                            {!loading && !error && items.length === 0 && (
                                                <tr><td colSpan={4} className="text-center p-4">Your wishlist is empty.</td></tr>
                                            )}

                                            {!loading && items.map(item => (
                                                <tr key={item.bookId}>
                                                    <td>
                                                        <span className="d-flex gap-5 align-items-center">
                                                            <button 
                                                                className="remove-icon border-0 bg-transparent" 
                                                                onClick={() => handleRemove(item.bookId)}
                                                            >
                                                                <img src="/assets/img/icon/icon-9.svg" alt="remove" />
                                                            </button>
                                                            <span className="cart">
                                                                <img src={item.imageUrl || '/assets/img/shop-cart/01.png'} alt="img" />
                                                            </span>
                                                            <span className="cart-title">
                                                                {item.title}
                                                            </span>
                                                        </span>
                                                    </td>
                                                    <td>
                                                        <span className="cart-price">${item.price.toFixed(2)}</span>
                                                    </td>
                                                    <td>
                                                        <span className={item.inStock ? "stock-title" : "stock-title-two"}>
                                                            {item.inStock ? "In Stock" : "Out Of Stock"}
                                                        </span>
                                                    </td>
                                                    <td>
                                                        {/* Subtotal is usually just price for wishlist, or we can hide this column */}
                                                        <span className="subtotal-price">${item.price.toFixed(2)}</span>
                                                    </td>
                                                </tr>
                                            ))}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </Layout>
    );
};

export default Wishlist;