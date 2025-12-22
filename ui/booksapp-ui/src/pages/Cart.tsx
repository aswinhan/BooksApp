// src/pages/Cart.tsx
import { useState } from 'react';
import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';
import type { CartItem } from '../types/Cart';

const Cart = () => {
    // Dummy Data (Later this will come from Context or API)
    const [cartItems, setCartItems] = useState<CartItem[]>([
        { id: '1', title: 'Simple Things You To Save Book', price: 30.00, quantity: 1, imageUrl: '/assets/img/shop-cart/01.png' },
        { id: '2', title: 'Qple GPad With Retina Display', price: 39.00, quantity: 2, imageUrl: '/assets/img/shop-cart/02.png' },
        { id: '3', title: 'Flovely and Unicom Erna', price: 19.00, quantity: 1, imageUrl: '/assets/img/shop-cart/03.png' }
    ]);

    // Update Quantity Logic
    const updateQuantity = (id: string, change: number) => {
        setCartItems(prevItems => prevItems.map(item => {
            if (item.id === id) {
                const newQuantity = item.quantity + change;
                // Prevent quantity going below 1
                return { ...item, quantity: newQuantity > 0 ? newQuantity : 1 };
            }
            return item;
        }));
    };

    // Remove Item Logic
    const removeItem = (id: string) => {
        setCartItems(prevItems => prevItems.filter(item => item.id !== id));
    };

    // Calculate Total
    const subtotal = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const shipping = 0; // Free for now
    const total = subtotal + shipping;

    return (
        <Layout>
            <Breadcrumb title="Shop Cart" activePage="Cart" />

            <div className="cart-section section-padding">
                <div className="container">
                    <div className="main-cart-wrapper">
                        <div className="row g-5">
                            {/* Left Side: Cart Items Table */}
                            <div className="col-xl-9">
                                <div className="table-responsive">
                                    <table className="table">
                                        <thead>
                                            <tr>
                                                <th>Product</th>
                                                <th>Price</th>
                                                <th>Quantity</th>
                                                <th>Subtotal</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {cartItems.map(item => (
                                                <tr key={item.id}>
                                                    <td>
                                                        <span className="d-flex gap-5 align-items-center">
                                                            <button 
                                                                className="remove-icon border-0 bg-transparent"
                                                                onClick={() => removeItem(item.id)}
                                                            >
                                                                <img src="/assets/img/icon/icon-9.svg" alt="remove" />
                                                            </button>
                                                            <span className="cart">
                                                                <img src={item.imageUrl} alt="img" />
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
                                                        <span className="quantity-basket">
                                                            <span className="qty">
                                                                <button className="qtyminus" onClick={() => updateQuantity(item.id, -1)}>−</button>
                                                                <input type="number" value={item.quantity} readOnly />
                                                                <button className="qtyplus" onClick={() => updateQuantity(item.id, 1)}>+</button>
                                                            </span>
                                                        </span>
                                                    </td>
                                                    <td>
                                                        <span className="subtotal-price">
                                                            ${(item.price * item.quantity).toFixed(2)}
                                                        </span>
                                                    </td>
                                                </tr>
                                            ))}
                                            {cartItems.length === 0 && (
                                                <tr>
                                                    <td colSpan={4} className="text-center p-5">
                                                        Your cart is empty. <Link to="/" className="text-success">Go Shopping!</Link>
                                                    </td>
                                                </tr>
                                            )}
                                        </tbody>
                                    </table>
                                </div>
                                <div className="cart-wrapper-footer">
                                    <form onSubmit={(e) => e.preventDefault()}>
                                        <div className="input-area">
                                            <input type="text" placeholder="Coupon Code" />
                                            <button type="submit" className="theme-btn">Apply</button>
                                        </div>
                                    </form>
                                    <Link to="/" className="theme-btn">Update Cart</Link>
                                </div>
                            </div>

                            {/* Right Side: Cart Totals */}
                            <div className="col-xl-3">
                                <div className="table-responsive cart-total">
                                    <table className="table">
                                        <thead>
                                            <tr>
                                                <th>Cart Total</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr>
                                                <td>
                                                    <span className="d-flex gap-5 align-items-center justify-content-between">
                                                        <span className="sub-title">Subtotal:</span>
                                                        <span className="sub-price">${subtotal.toFixed(2)}</span>
                                                    </span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <span className="d-flex gap-5 align-items-center justify-content-between">
                                                        <span className="sub-title">Shipping:</span>
                                                        <span className="sub-text">Free</span>
                                                    </span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <span className="d-flex gap-5 align-items-center justify-content-between">
                                                        <span className="sub-title">Total:</span>
                                                        <span className="sub-price sub-price-total">${total.toFixed(2)}</span>
                                                    </span>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    <Link to="/checkout" className="theme-btn w-100 text-center">
                                        Proceed to checkout
                                    </Link>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </Layout>
    );
};

export default Cart;