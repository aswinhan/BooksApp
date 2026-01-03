import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';

const ShopDetails = () => {
    const { id } = useParams(); // Get book ID from URL
    const [quantity, setQuantity] = useState(1);
    const [activeImgTab, setActiveImgTab] = useState('thumb1');
    const [activeInfoTab, setActiveInfoTab] = useState('description');

    // Dummy Data for the current book (In real app, fetch using 'id')
    const book = {
        title: "Castle The Sky",
        price: 30.00,
        oldPrice: 39.99,
        description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec pulvinar, tortor quis varius pretium est felis scelerisque nulla.",
        images: {
            thumb1: '/assets/img/shop-details/01.png',
            thumb2: '/assets/img/shop-details/02.png',
            thumb3: '/assets/img/shop-details/03.png',
            thumb4: '/assets/img/shop-details/04.png',
            thumb5: '/assets/img/shop-details/05.png',
        },
        thumbnails: {
            thumb1: '/assets/img/shop-details/sm-1.png',
            thumb2: '/assets/img/shop-details/sm-2.png',
            thumb3: '/assets/img/shop-details/sm-3.png',
            thumb4: '/assets/img/shop-details/sm-4.png',
            thumb5: '/assets/img/shop-details/sm-5.png',
        }
    };

    // Dummy Data for Related Products
    const relatedProducts = [
        { id: '1', title: 'Simple Things You To Save BOOK', price: 30.00, img: '/assets/img/book/01.png' },
        { id: '2', title: 'How Deal With Very Bad BOOK', price: 30.00, img: '/assets/img/book/02.png' },
        { id: '3', title: 'Qple GPad With Retina Display', price: 30.00, img: '/assets/img/book/03.png' },
        { id: '4', title: 'Design Low Book', price: 30.00, img: '/assets/img/book/04.png' },
    ];

    const handleQuantityChange = (amount: number) => {
        setQuantity(prev => (prev + amount > 0 ? prev + amount : 1));
    };

    return (
        <Layout>
            <Breadcrumb title="Shop Details" activePage="Shop Details" />

            {/* Shop Details Section Start */}
            <section className="shop-details-section fix section-padding">
                <div className="container">
                    <div className="shop-details-wrapper">
                        <div className="row g-4">
                            {/* Left Side: Image Gallery */}
                            <div className="col-lg-5">
                                <div className="shop-details-image">
                                    <div className="tab-content">
                                        <div className="tab-pane fade show active">
                                            <div className="shop-details-thumb">
                                                {/* Dynamic Main Image based on state */}
                                                <img src={book.images[activeImgTab as keyof typeof book.images]} alt="img" />
                                            </div>
                                        </div>
                                    </div>
                                    <ul className="nav">
                                        {Object.keys(book.thumbnails).map((key) => (
                                            <li className="nav-item" key={key}>
                                                <button 
                                                    className={`nav-link ${activeImgTab === key ? 'active' : ''}`}
                                                    onClick={() => setActiveImgTab(key)}
                                                >
                                                    <img src={book.thumbnails[key as keyof typeof book.thumbnails]} alt="img" />
                                                </button>
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            </div>

                            {/* Right Side: Content */}
                            <div className="col-lg-7">
                                <div className="shop-details-content">
                                    <div className="title-wrapper">
                                        <h2>{book.title}</h2>
                                        <h5>Stock availability.</h5>
                                    </div>
                                    <div className="star">
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fa-regular fa-star"></i>
                                        <span>(1 Customer Reviews)</span>
                                    </div>
                                    <p>{book.description}</p>
                                    <div className="price-list">
                                        <h3>${book.price.toFixed(2)}</h3>
                                    </div>
                                    <div className="cart-wrapper">
                                        <div className="quantity-basket">
                                            <p className="qty">
                                                <button className="qtyminus" onClick={() => handleQuantityChange(-1)}>−</button>
                                                <input type="number" name='qty' value={quantity} min={1} max={1000} readOnly />
                                                <button className="qtyplus" onClick={() => handleQuantityChange(1)}>+</button>
                                            </p>
                                        </div>
                                        <button type="button" className="theme-btn style-2" data-bs-toggle="modal" data-bs-target="#readMoreModal">
                                            Read A Little
                                        </button>
                                        <Link to="/cart" className="theme-btn">
                                            <i className="fa-solid fa-basket-shopping"></i> Add To Cart
                                        </Link>
                                        <div className="icon-box">
                                            <Link to="/wishlist" className="icon"><i className="far fa-heart"></i></Link>
                                            <Link to="/cart" className="icon-2"><img src="/assets/img/icon/shuffle.svg" alt="svg-icon" /></Link>
                                        </div>
                                    </div>
                                    <div className="category-box">
                                        <div className="category-list">
                                            <ul>
                                                <li><span>SKU:</span> FTC1020B65D</li>
                                                <li><span>Category:</span> Kids Toys</li>
                                            </ul>
                                            <ul>
                                                <li><span>Tags:</span> Design Low Book</li>
                                                <li><span>Format:</span> Hardcover</li>
                                            </ul>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Description / Info Tabs */}
                        <div className="single-tab section-padding pb-0">
                            <ul className="nav mb-5">
                                {['description', 'additional', 'review'].map(tab => (
                                    <li className="nav-item" key={tab}>
                                        <button 
                                            className={`nav-link ${activeInfoTab === tab ? 'active' : ''} ps-0`}
                                            onClick={() => setActiveInfoTab(tab)}
                                        >
                                            <h6 style={{textTransform: 'capitalize'}}>{tab === 'additional' ? 'Additional Information' : tab}</h6>
                                        </button>
                                    </li>
                                ))}
                            </ul>
                            <div className="tab-content">
                                {activeInfoTab === 'description' && (
                                    <div className="description-items">
                                        <p>
                                            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque quis erat interdum, tempor turpis in, sodales ex.
                                            In hac habitasse platea dictumst. Etiam accumsan scelerisque urna, a lobortis velit vehicula ut.
                                        </p>
                                    </div>
                                )}
                                {activeInfoTab === 'additional' && (
                                    <div className="table-responsive">
                                        <table className="table table-bordered">
                                            <tbody>
                                                <tr><td className="text-1">Availability</td><td className="text-2">Available</td></tr>
                                                <tr><td className="text-1">Format</td><td className="text-2">Hardcover</td></tr>
                                                <tr><td className="text-1">Pages</td><td className="text-2">330</td></tr>
                                            </tbody>
                                        </table>
                                    </div>
                                )}
                                {activeInfoTab === 'review' && (
                                    <div className="review-items">
                                        <div className="review-wrap-area d-flex gap-4">
                                            <div className="review-thumb">
                                                <img src="/assets/img/shop-details/review.png" alt="img" />
                                            </div>
                                            <div className="review-content">
                                                <div className="head-area d-flex flex-wrap gap-2 align-items-center justify-content-between">
                                                    <div className="cont">
                                                        <h5>Leslie Alexander</h5>
                                                        <span>February 10, 2024 at 2:37 pm</span>
                                                    </div>
                                                    <div className="star">
                                                        <i className="fa-solid fa-star"></i>
                                                        <i className="fa-solid fa-star"></i>
                                                        <i className="fa-solid fa-star"></i>
                                                        <i className="fa-solid fa-star"></i>
                                                        <i className="fa-regular fa-star"></i>
                                                    </div>
                                                </div>
                                                <p className="mt-30 mb-4">Neque porro est qui dolorem ipsum quia quaed inventor veritatis et quasi architecto var sed efficitur turpis gilla sed sit amet finibus eros.</p>
                                            </div>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {/* Related Products Section */}
            <section className="top-ratting-book-section fix section-padding pt-0">
                <div className="container">
                    <div className="section-title text-center">
                        <h2 className="mb-3 wow fadeInUp" data-wow-delay=".3s">Related Products</h2>
                        <p className="wow fadeInUp" data-wow-delay=".5s">Interdum et malesuada fames ac ante ipsum primis in faucibus.</p>
                    </div>
                    <div className="row">
                        {relatedProducts.map(product => (
                            <div className="col-xl-3 col-lg-4 col-md-6 mb-4" key={product.id}>
                                <div className="shop-box-items style-2">
                                    <div className="book-thumb center">
                                        <Link to={`/books/${product.id}`}><img src={product.img} alt="img" /></Link>
                                        <ul className="shop-icon d-grid justify-content-center align-items-center">
                                            <li><Link to="/cart"><i className="far fa-heart"></i></Link></li>
                                            <li><Link to="/cart"><img className="icon" src="/assets/img/icon/shuffle.svg" alt="svg-icon" /></Link></li>
                                            <li><Link to={`/books/${product.id}`}><i className="far fa-eye"></i></Link></li>
                                        </ul>
                                    </div>
                                    <div className="shop-content">
                                        <h5>Author Name</h5>
                                        <h3><Link to={`/books/${product.id}`}>{product.title}</Link></h3>
                                        <ul className="price-list">
                                            <li>${product.price.toFixed(2)}</li>
                                        </ul>
                                        <div className="shop-button">
                                            <Link to="/cart" className="theme-btn"><i className="fa-solid fa-basket-shopping"></i> Add To Cart</Link>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </section>
        </Layout>
    );
};

export default ShopDetails;