import { useState } from 'react';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';

// 1. Data Structure (Easy to edit later)
const faqData = [
    {
        id: 'trust',
        label: 'Trust & Safety',
        questions: [
            { q: "What skills will my child learn by using kinder?", a: "Nunc tincidunt cursus lectus ac semper. Aenean ullamcorper quis arcu molestie consequat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Ut nec lobortis elit, eu ultrices justo." },
            { q: "What is included in your services?", a: "Fusce auctor erat est, non fringilla nibh tempus quis. Aenean dignissim turpis ut interdum interdum. Nam molestie sed ex non tempus." },
            { q: "What type of company is measured?", a: "Donec sodales aliquam orci non imperdiet. Quisque tempus dolor id nisi blandit tempor ut id lacus." },
            { q: "Are the tours included with meals?", a: "Nunc tincidunt cursus lectus ac semper. Aenean ullamcorper quis arcu molestie consequat." },
            { q: "What Activities are Done in the Development?", a: "Interdum et malesuada fames ac ante ipsum primis in faucibus. Ut nec lobortis elit, eu ultrices justo." },
            { q: "What ages is Prodigies designed for?", a: "Aenean dignissim turpis ut interdum interdum. Nam molestie sed ex non tempus." },
        ]
    },
    {
        id: 'general',
        label: 'General',
        questions: [
            { q: "How do I create an account?", a: "Creating an account is simple. Click the 'Login' button in the header and select 'Register'." },
            { q: "Is shipping free?", a: "We offer free shipping on orders over $100. Standard shipping rates apply for smaller orders." },
            { q: "Can I return a book?", a: "Yes, we have a 30-day return policy for books in their original condition." }
        ]
    },
    {
        id: 'programs',
        label: 'Mamaya Shop',
        questions: [
            { q: "Do you sell e-books?", a: "Yes, many of our titles are available in digital formats compatible with most readers." },
            { q: "How do I use a coupon code?", a: "You can enter your coupon code on the Cart page before proceeding to checkout." }
        ]
    },
    {
        id: 'kindergarten',
        label: 'Kids Toys',
        questions: [
            { q: "Are the toys safe for toddlers?", a: "All our toys are certified safe and non-toxic. Age recommendations are listed on each product page." },
            { q: "Do toys come with batteries?", a: "Most electronic toys require batteries which are sold separately unless stated otherwise." }
        ]
    }
];

const Faq = () => {
    // State for Active Tab
    const [activeTab, setActiveTab] = useState('trust');
    
    // State for Open Accordion (Tracked by Index)
    const [openAccordionIndex, setOpenAccordionIndex] = useState<number | null>(0);

    const toggleAccordion = (index: number) => {
        if (openAccordionIndex === index) {
            setOpenAccordionIndex(null); // Close if already open
        } else {
            setOpenAccordionIndex(index); // Open new one
        }
    };

    // Get current tab data
    const currentCategory = faqData.find(cat => cat.id === activeTab);

    return (
        <Layout>
            <Breadcrumb title="Faq's" activePage="Faq's" />

            <section className="faq-section fix section-padding">
                <div className="container">
                    <div className="faq-wrapper">
                        <div className="row g-4">
                            {/* Left Side: Tabs */}
                            <div className="col-lg-3">
                                <div className="faq-left">
                                    <ul className="nav" style={{ display: 'block' }}> {/* Force block display for vertical look */}
                                        {faqData.map((cat) => (
                                            <li className="nav-item" key={cat.id} style={{ marginBottom: '10px' }}>
                                                <button 
                                                    className={`nav-link ${activeTab === cat.id ? 'active' : ''} w-100 text-start`}
                                                    onClick={() => {
                                                        setActiveTab(cat.id);
                                                        setOpenAccordionIndex(0); // Reset accordion when switching tabs
                                                    }}
                                                >
                                                    {cat.label}
                                                </button>
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            </div>

                            {/* Right Side: Accordion Content */}
                            <div className="col-lg-9">
                                <div className="tab-content">
                                    <div className="tab-pane fade show active">
                                        <div className="faq-content">
                                            <div className="faq-accordion">
                                                <div className="accordion" id="accordion">
                                                    {currentCategory?.questions.map((item, index) => (
                                                        <div className="accordion-item mb-3" key={index}>
                                                            <h5 className="accordion-header">
                                                                <button 
                                                                    className={`accordion-button ${openAccordionIndex === index ? '' : 'collapsed'}`} 
                                                                    type="button"
                                                                    onClick={() => toggleAccordion(index)}
                                                                >
                                                                    {item.q}
                                                                </button>
                                                            </h5>
                                                            <div 
                                                                className={`accordion-collapse collapse ${openAccordionIndex === index ? 'show' : ''}`}
                                                            >
                                                                <div className="accordion-body">
                                                                    {item.a}
                                                                </div>
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </Layout>
    );
};

export default Faq;