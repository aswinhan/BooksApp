import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';

const NotFound = () => {
    return (
        <Layout>
            {/* Optional: You can keep or remove the breadcrumb for 404 pages */}
            <Breadcrumb title="Error 404" activePage="404" />

            <section className="Error-section section-padding fix">
                <div className="container">
                    <div className="row justify-content-center">
                        <div className="col-lg-9">
                            <div className="error-items">
                                <div className="error-image wow fadeInUp" data-wow-delay=".3s">
                                    {/* Ensure the image path starts with / */}
                                    <img src="/assets/img/404.png" alt="404 Error" />
                                </div>
                                <h2 className="wow fadeInUp" data-wow-delay=".5s">
                                    <span>Oops!</span> Page not found
                                </h2>
                                <p className="wow fadeInUp" data-wow-delay=".7s">
                                    The page you are looking for does not exist
                                </p>
                                <Link to="/" className="theme-btn wow fadeInUp" data-wow-delay=".8s">
                                    Back to Home Pages
                                    <i className="fa-solid fa-arrow-right-long"></i>
                                </Link>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </Layout>
    );
};

export default NotFound;