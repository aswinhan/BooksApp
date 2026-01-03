import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';

const About = () => {
    return (
        <Layout>
            <Breadcrumb title="About Us" activePage="About" />

            {/* About Section Start */}
            <section className="about-section fix section-padding">
                <div className="container">
                    <div className="about-wrapper">
                        <div className="row g-4 align-items-center">
                            {/* Left Column: Image & Video */}
                            <div className="col-lg-6 wow fadeInUp" data-wow-delay=".3s">
                                <div className="about-image">
                                    <img src="/assets/img/about.jpg" alt="About Bookle" />
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

                            {/* Right Column: Content */}
                            <div className="col-lg-6">
                                <div className="about-content">
                                    <div className="section-title">
                                        <h2 className="wow fadeInUp" data-wow-delay=".3s">
                                            About the Bookle <br /> Books Store
                                        </h2>
                                    </div>
                                    <p className="mt-3 mt-md-0 wow fadeInUp" data-wow-delay=".5s">
                                        Nullam convallis ullamcorper nulla. Nam accumsan ac leo quis posuere. Nunc rutrum lorem
                                        justo, at blandit mauris ullamcorper tristique. Suspendisse vel ante venenatis,
                                        porttitor ligula sed, iaculis metus. Nullam non erat gravida, viverra leo ut, maximus
                                        tortor. Pellentesque vitae nunc rhoncus, lacinia nulla sed, commodo lectus. Curabitur at
                                        consectetur velit.
                                    </p>
                                    <p className="mt-3 wow fadeInUp" data-wow-delay=".7s">
                                        Morbi cursus enim in consequat suscipit. Quisque id dui ante. Praesent auctor sed velit
                                        ac aliquet. Morbi consectetur sem nec ipsum malesuada, ut gravida nisl molestie. Proin
                                        hendrerit ullamcorper dui, quis convallis mauris cursus nec. Interdum et malesuada fames
                                        ac ante ipsum primis in faucibus. Vivamus ac laoreet orci.
                                    </p>
                                    
                                    {/* Link to Contact or Home since 'overview' usually goes deeper or back */}
                                    <Link to="/contact" className="link-btn wow fadeInUp" data-wow-delay=".8s">
                                        Contact Us <i className="fa-regular fa-arrow-right"></i>
                                    </Link>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </Layout>
    );
};

export default About;