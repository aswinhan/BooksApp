import { Link } from 'react-router-dom';

const Footer = () => {
    return (
        <footer className="footer-section footer-bg">
            <div className="container">
                <div className="contact-info-area">
                    {/* Hardcoded contact items for brevity, can be props later */}
                    <div className="contact-info-items wow fadeInUp" data-wow-delay=".2s">
                        <div className="icon"><i className="icon-icon-5"></i></div>
                        <div className="content"><p>Call Us 7/24</p><h3>+208-555-0112</h3></div>
                    </div>
                    {/* ... other contact items ... */}
                </div>
            </div>
            <div className="footer-widgets-wrapper">
                <div className="plane-shape float-bob-y">
                    <img src="/assets/img/plane-shape.png" alt="img" />
                </div>
                <div className="container">
                    <div className="row">
                        <div className="col-xl-3 col-lg-4 col-md-6 wow fadeInUp" data-wow-delay=".2s">
                            <div className="single-footer-widget">
                                <div className="widget-head">
                                    <Link to="/">
                                        <img src="/assets/img/logo/white-logo.svg" alt="logo-img" />
                                    </Link>
                                </div>
                                <div className="footer-content">
                                    <p>Phasellus ultricies aliquam volutpat ullamcorper laoreet neque, a lacinia curabitur lacinia mollis</p>
                                    <div className="social-icon d-flex align-items-center">
                                        <a href="#"><i className="fab fa-facebook-f"></i></a>
                                        <a href="#"><i className="fab fa-twitter"></i></a>
                                        <a href="#"><i className="fa-brands fa-linkedin-in"></i></a>
                                        <a href="#"><i className="fa-brands fa-youtube"></i></a>
                                    </div>
                                </div>
                            </div>
                        </div>
                        {/* Add other footer widgets here similar to above */}
                    </div>
                </div>
            </div>
            <div className="footer-bottom">
                <div className="container">
                    <div className="footer-wrapper d-flex align-items-center justify-content-between">
                        <p className="wow fadeInLeft" data-wow-delay=".3s">
                            © All Copyright 2024 by <Link to="/">Bookle</Link>
                        </p>
                    </div>
                </div>
            </div>
        </footer>
    );
};

export default Footer;