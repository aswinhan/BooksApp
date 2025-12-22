// src/pages/Contact.tsx
import { useState, type ChangeEvent, type FormEvent } from 'react';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';

const Contact = () => {
    // 1. State to manage form inputs
    const [formData, setFormData] = useState({
        name: '',
        email: '',
        message: ''
    });

    const [isSubmitting, setIsSubmitting] = useState(false);

    // 2. Handle Input Changes
    const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    // 3. Handle Form Submission
    const handleSubmit = (e: FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);

        // Simulate API Call
        setTimeout(() => {
            alert(`Thank you ${formData.name}! We have received your message.`);
            setFormData({ name: '', email: '', message: '' }); // Reset form
            setIsSubmitting(false);
        }, 1500);
    };

    return (
        <Layout>
            <Breadcrumb title="Contact Us" activePage="Contact" />

            {/* Contact Section Start */}
            <section className="contact-section fix section-padding">
                <div className="container">
                    <div className="contact-wrapper">
                        <div className="row g-4 align-items-center">
                            {/* Left Column: Contact Info */}
                            <div className="col-lg-4">
                                <div className="contact-left-items">
                                    <div className="contact-info-area-2">
                                        <div className="contact-info-items mb-4">
                                            <div className="icon">
                                                <i className="icon-icon-10"></i>
                                            </div>
                                            <div className="content">
                                                <p>Call Us 7/24</p>
                                                <h3><a href="tel:+2085550112">+208-555-0112</a></h3>
                                            </div>
                                        </div>
                                        <div className="contact-info-items mb-4">
                                            <div className="icon">
                                                <i className="icon-icon-11"></i>
                                            </div>
                                            <div className="content">
                                                <p>Make a Quote</p>
                                                <h3><a href="mailto:example@gmail.com">example@gmail.com</a></h3>
                                            </div>
                                        </div>
                                        <div className="contact-info-items border-none">
                                            <div className="icon">
                                                <i className="icon-icon-12"></i>
                                            </div>
                                            <div className="content">
                                                <p>Location</p>
                                                <h3>4517 Washington ave.</h3>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    {/* Video Section */}
                                    <div className="video-image">
                                        <img src="/assets/img/contact.jpg" alt="img" />
                                        <div className="video-box">
                                            <a 
                                                href="https://www.youtube.com/watch?v=Cn4G2lZ_g2I" 
                                                className="video-btn ripple video-popup"
                                                target="_blank"
                                                rel="noreferrer"
                                            >
                                                <i className="fa-solid fa-play"></i>
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Right Column: Form */}
                            <div className="col-lg-8">
                                <div className="contact-content">
                                    <h2>Ready to Get Started?</h2>
                                    <p>
                                        Nunc tincidunt cursus lectus ac semper. Aenean ullamcorper quis arcu molestie consequat.
                                        Interdum et malesuada fames ac ante ipsum primis in faucibus.
                                    </p>
                                    <form onSubmit={handleSubmit} className="contact-form-items">
                                        <div className="row g-4">
                                            <div className="col-lg-6 wow fadeInUp" data-wow-delay=".3s">
                                                <div className="form-clt">
                                                    <span>Your name*</span>
                                                    <input 
                                                        type="text" 
                                                        name="name" 
                                                        value={formData.name}
                                                        onChange={handleChange}
                                                        placeholder="Your Name" 
                                                        required
                                                    />
                                                </div>
                                            </div>
                                            <div className="col-lg-6 wow fadeInUp" data-wow-delay=".5s">
                                                <div className="form-clt">
                                                    <span>Your Email*</span>
                                                    <input 
                                                        type="email" 
                                                        name="email" 
                                                        value={formData.email}
                                                        onChange={handleChange}
                                                        placeholder="Your Email" 
                                                        required
                                                    />
                                                </div>
                                            </div>
                                            <div className="col-lg-12 wow fadeInUp" data-wow-delay=".7s">
                                                <div className="form-clt">
                                                    <span>Write Message*</span>
                                                    <textarea 
                                                        name="message" 
                                                        value={formData.message}
                                                        onChange={handleChange}
                                                        placeholder="Write Message"
                                                        required
                                                    ></textarea>
                                                </div>
                                            </div>
                                            <div className="col-lg-7 wow fadeInUp" data-wow-delay=".9s">
                                                <button type="submit" className="theme-btn" disabled={isSubmitting}>
                                                    {isSubmitting ? 'Sending...' : (
                                                        <>Send Message <i className="fa-solid fa-arrow-right-long"></i></>
                                                    )}
                                                </button>
                                            </div>
                                        </div>
                                    </form>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {/* Map Section */}
            <div className="map-section">
                <div className="map-items">
                    <div className="googpemap">
                        {/* I replaced the placeholder link with a valid Google Maps Embed for a generic location */}
                        <iframe 
                            src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3022.1841339343685!2d-73.9857062242491!3d40.7579786347963!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x89c25855c6480299%3A0x55194ec5a1ae072e!2sTimes%20Square!5e0!3m2!1sen!2sus!4v1709666666666!5m2!1sen!2sus"
                            style={{ border: 0 }} 
                            allowFullScreen={true} 
                            loading="lazy"
                            referrerPolicy="no-referrer-when-downgrade"
                            title="Google Map"
                        ></iframe>
                    </div>
                </div>
            </div>
        </Layout>
    );
};

export default Contact;