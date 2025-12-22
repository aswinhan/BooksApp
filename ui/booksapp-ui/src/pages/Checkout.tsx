// src/pages/Checkout.tsx
import { useState, type ChangeEvent, type FormEvent } from 'react';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';

const Checkout = () => {
    // 1. Form State
    const [billingDetails, setBillingDetails] = useState({
        firstName: '',
        lastName: '',
        companyName: '',
        country: '',
        address1: '',
        address2: '',
        city: '',
        phone: '',
        email: '',
        notes: ''
    });

    const [paymentMethod, setPaymentMethod] = useState('bank'); // Default to bank transfer

    // 2. Handle Inputs
    const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setBillingDetails(prev => ({
            ...prev,
            [name]: value
        }));
    };

    // 3. Handle Submit
    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        alert(`Order Placed! 
               Name: ${billingDetails.firstName} ${billingDetails.lastName}
               Payment: ${paymentMethod}
               Total: $55.00 (Dummy Total)`);
    };

    return (
        <Layout>
            <Breadcrumb title="Checkout" activePage="Checkout" />

            {/* Checkout Section Start */}
            <section className="checkout-section fix section-padding">
                <div className="container">
                    <form onSubmit={handleSubmit}>
                        <div className="row g-5">
                            {/* Left Side: Billing Details Form */}
                            <div className="col-lg-9">
                                <div className="checkout-single-wrapper">
                                    <div className="checkout-single boxshado-single">
                                        <h4>Billing Details</h4>
                                        <div className="checkout-single-form">
                                            <div className="row g-4">
                                                <div className="col-lg-6">
                                                    <div className="input-single">
                                                        <span>First Name*</span>
                                                        <input type="text" name="firstName" required placeholder="First Name" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-6">
                                                    <div className="input-single">
                                                        <span>Last Name*</span>
                                                        <input type="text" name="lastName" required placeholder="Last Name" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Company name (optional)</span>
                                                        <input name="companyName" placeholder="Company Name" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Country*</span>
                                                        <input name="country" required placeholder="Select a country" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Street Address*</span>
                                                        <input name="address1" required placeholder="Home number and street name" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Address 2 (Optional)</span>
                                                        <input name="address2" placeholder="Apartment, suite, unit, etc." onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Town/ City*</span>
                                                        <input name="city" required placeholder="Town / City" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Phone*</span>
                                                        <input name="phone" required placeholder="Phone Number" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Email Address*</span>
                                                        <input name="email" required type="email" placeholder="Email Address" onChange={handleChange} />
                                                    </div>
                                                </div>
                                                
                                                {/* Checkboxes */}
                                                <div className="col-lg-12">
                                                    <div className="input-check payment-save">
                                                        <input type="checkbox" className="form-check-input" id="saveForNext" />
                                                        <label htmlFor="saveForNext">Save for my next payment</label>
                                                    </div>
                                                    <div className="input-check payment-save style-2">
                                                        <input type="checkbox" className="form-check-input" id="shipDifferent" />
                                                        <label htmlFor="shipDifferent">Ship to a different address?</label>
                                                    </div>
                                                </div>

                                                <div className="col-lg-12">
                                                    <div className="input-single">
                                                        <span>Order notes (optional)</span>
                                                        <textarea name="notes" placeholder="Notes about your order, e.g special notes for delivery." onChange={handleChange}></textarea>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Right Side: Order Summary & Payment */}
                            <div className="col-lg-3">
                                <div className="checkout-order-area">
                                    <h3>Our Order</h3>
                                    <div className="product-checout-area">
                                        <div className="checkout-item d-flex align-items-center justify-content-between">
                                            <p>Product</p>
                                            <p>Subtotal</p>
                                        </div>
                                        {/* Dummy Item - Later map this from Cart Context */}
                                        <div className="checkout-item d-flex align-items-center justify-content-between">
                                            <p>Fashion Women’s 1</p>
                                            <p>$29.00</p>
                                        </div>
                                        <div className="checkout-item d-flex justify-content-between">
                                            <p>Shipping</p>
                                            <div className="shopping-items">
                                                <div className="form-check d-flex align-items-center from-customradio">
                                                    <input className="form-check-input" type="radio" name="shipping" id="free" defaultChecked />
                                                    <label className="form-check-label" htmlFor="free">Free Shipping</label>
                                                </div>
                                                <div className="form-check d-flex align-items-center from-customradio">
                                                    <input className="form-check-input" type="radio" name="shipping" id="local" />
                                                    <label className="form-check-label" htmlFor="local">Local: $15.00</label>
                                                </div>
                                                <div className="form-check d-flex align-items-center from-customradio">
                                                    <input className="form-check-input" type="radio" name="shipping" id="flat" />
                                                    <label className="form-check-label" htmlFor="flat">Flat rate: $10.00</label>
                                                </div>
                                            </div>
                                        </div>
                                        <div className="checkout-item d-flex align-items-center justify-content-between">
                                            <p>Total</p>
                                            <p>$55.00</p>
                                        </div>
                                        
                                        {/* Payment Methods */}
                                        <div className="checkout-item-2">
                                            <div className="form-check-2 d-flex align-items-center from-customradio-2">
                                                <input 
                                                    className="form-check-input" 
                                                    type="radio" 
                                                    name="payment" 
                                                    id="bank" 
                                                    checked={paymentMethod === 'bank'}
                                                    onChange={() => setPaymentMethod('bank')} 
                                                />
                                                <label className="form-check-label" htmlFor="bank">Direct bank transfer</label>
                                            </div>
                                            <p>Make your payment directly into our bank account please use your Order ID as reference.</p>
                                            
                                            <div className="form-check-3 d-flex align-items-center from-customradio-2 mt-3">
                                                <input 
                                                    className="form-check-input" 
                                                    type="radio" 
                                                    name="payment" 
                                                    id="cod"
                                                    checked={paymentMethod === 'cod'}
                                                    onChange={() => setPaymentMethod('cod')} 
                                                />
                                                <label className="form-check-label" htmlFor="cod">Cash on delivery</label>
                                            </div>

                                            <div className="form-check-3 d-flex align-items-center from-customradio-2 mt-3">
                                                <input 
                                                    className="form-check-input" 
                                                    type="radio" 
                                                    name="payment" 
                                                    id="paypal"
                                                    checked={paymentMethod === 'paypal'}
                                                    onChange={() => setPaymentMethod('paypal')} 
                                                />
                                                <label className="form-check-label" htmlFor="paypal">Paypal</label>
                                            </div>

                                            <ul className="brand-logo mt-3">
                                                <li><img src="/assets/img/PayPal.png" alt="PayPal" /></li>
                                                <li><img src="/assets/img/GooglePay.png" alt="GPay" /></li>
                                                <li><img src="/assets/img/Mastercard2.png" alt="Mastercard" /></li>
                                            </ul>
                                            
                                            <button type="submit" className="theme-btn w-100 mt-4 text-center">
                                                Place Order
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </section>
        </Layout>
    );
};

export default Checkout;