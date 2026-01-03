import { useParams } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';
import BlogSidebar from '../components/blog/BlogSidebar';

// Mock Data (In a real app, you would fetch this from your API using the ID)
const blogPost = {
    id: 1,
    title: "Eu parturient Dictumst Frames quam Temper",
    author: "Admin",
    date: "Feb 10, 2024",
    comments: 2,
    category: "Book Store",
    image: "/assets/img/news/post-4.jpg",
    content: `
        <p class="mb-3">
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris efficitur et ipsum ut volutpat. Morbi a mollis felis. Nam consectetur lectus vel lorem facilisis, quis viverra purus pharetra. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce dui lacus, tempor a metus vel, varius rhoncus nunc. Suspendisse luctus feugiat dictum. Curabitur ipsum velit, viverra in pretium eget, molestie maximus magna. Aliquam elementum vel turpis non bibendum. Cras in consequat neque.
        </p>
        <p class="mb-3">
            Nunc tincidunt cursus lectus ac semper. Aenean ullamcorper quis arcu molestie consequat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Ut nec lobortis elit, eu ultrices justo. Fusce auctor erat est, non fringilla nibh tempus quis. Aenean dignissim turpis ut interdum interdum. Nam molestie sed ex non tempus. Donec sodales aliquam orci non imperdiet. Quisque tempus dolor id nisi blandit tempor ut id lacus. Aliquam mattis tempor posuere. Sed ut sollicitudin velit.
        </p>
        <p>
            Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris efficitur et ipsum ut volutpat. Morbi a mollis felis. Nam consectetur lectus vel lorem facilisis, quis viverra purus pharetra. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce dui lacus, tempor a metus vel, varius rhoncus nunc. Suspendisse luctus feugiat dictum. Curabitur ipsum velit, viverra in pretium eget, molestie maximus magna. Aliquam elementum vel turpis non bibendum. Cras in consequat neque.
        </p>
    `
};

const BlogDetails = () => {
    const { id } = useParams(); // Use this to fetch specific post later

    return (
        <Layout>
            <Breadcrumb title="Blog Details" activePage="Blog Details" />

            <section className="news-details fix section-padding">
                <div className="container">
                    <div className="news-details-area">
                        <div className="row g-5">
                            {/* Left Side: Post Content */}
                            <div className="col-xl-9 col-lg-8">
                                <div className="blog-post-details">
                                    <div className="single-blog-post">
                                        <div 
                                            className="post-featured-thumb bg-cover" 
                                            style={{ backgroundImage: `url(${blogPost.image})` }}
                                        ></div>
                                        <div className="post-content">
                                            <ul className="post-list d-flex align-items-center">
                                                <li><i className="fa-light fa-user"></i> By {blogPost.author}</li>
                                                <li><i className="fa-sharp fa-regular fa-comments"></i> {blogPost.comments} Comments</li>
                                                <li><i className="fa-light fa-tag"></i> {blogPost.category}</li>
                                            </ul>
                                            <h3>{blogPost.title}</h3>
                                            
                                            {/* Render HTML Content safely */}
                                            <div dangerouslySetInnerHTML={{ __html: blogPost.content }} />

                                            <div className="row g-4 mt-4">
                                                <div className="col-lg-6">
                                                    <div className="details-image">
                                                        <img src="/assets/img/news/post-5.jpg" alt="img" />
                                                    </div>
                                                </div>
                                                <div className="col-lg-6">
                                                    <div className="details-image">
                                                        <img src="/assets/img/news/post-6.jpg" alt="img" />
                                                    </div>
                                                </div>
                                            </div>

                                            <p className="pt-5 mb-5">
                                                Consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore of magna aliqua. Ut enim ad minim veniam, made of owl the quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea dolor commodo consequat.
                                            </p>

                                            <div className="hilight-text mt-4 mb-5">
                                                <p>
                                                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris efficitur et ipsum ut volutpat. Morbi a mollis felis. Nam consectetur lectus vel lorem facilisis, quis viverra purus pharetra.
                                                </p>
                                                <svg width="36" height="36" viewBox="0 0 36 36" fill="none" xmlns="http://www.w3.org/2000/svg">
                                                    <path d="M7.71428 20.0711H0.5V5.64258H14.9286V20.4531L9.97665 30.3568H3.38041L8.16149 20.7947L8.5233 20.0711H7.71428Z" stroke="#543EE8" />
                                                    <path d="M28.2846 20.0711H21.0703V5.64258H35.4989V20.4531L30.547 30.3568H23.9507L28.7318 20.7947L29.0936 20.0711H28.2846Z" stroke="#543EE8" />
                                                </svg>
                                            </div>

                                            {/* Tags & Share */}
                                            <div className="row tag-share-wrap mt-4 mb-5">
                                                <div className="col-lg-8 col-12">
                                                    <div className="tagcloud">
                                                        <span className="me-3">Tags:</span>
                                                        <a href="#">Adventure</a>
                                                        <a href="#">Education</a>
                                                        <a href="#">Store</a>
                                                    </div>
                                                </div>
                                                <div className="col-lg-4 col-12 mt-3 mt-lg-0 text-lg-end">
                                                    <div className="social-share">
                                                        <span className="me-3">Share:</span>
                                                        <a href="#"><i className="fab fa-facebook-f"></i></a>
                                                        <a href="#"><i className="fab fa-twitter"></i></a>
                                                        <a href="#"><i className="fab fa-linkedin-in"></i></a>
                                                    </div>
                                                </div>
                                            </div>

                                            {/* Comments Section */}
                                            <div className="comments-area">
                                                <div className="comments-heading">
                                                    <h3>02 Comments</h3>
                                                </div>
                                                
                                                {/* Single Comment 1 */}
                                                <div className="blog-single-comment d-flex gap-4 pt-4 pb-5">
                                                    <div className="image">
                                                        <img src="/assets/img/news/comment.png" alt="image" />
                                                    </div>
                                                    <div className="content">
                                                        <div className="head d-flex flex-wrap gap-2 align-items-center justify-content-between">
                                                            <div className="con">
                                                                <h5><a href="#">Leslie Alexander</a></h5>
                                                                <span>March 20, 2024 at 2:37 pm</span>
                                                            </div>
                                                            <div className="star">
                                                                {[1,2,3,4,5].map(i => <i key={i} className="fa-solid fa-star"></i>)}
                                                            </div>
                                                        </div>
                                                        <p className="mt-30 mb-4">Neque porro est qui dolorem ipsum quia quaed inventor veritatis et quasi architecto var sed efficitur turpis gilla sed sit amet finibus eros.</p>
                                                        <a href="#" className="reply">Reply</a>
                                                    </div>
                                                </div>

                                                {/* Single Comment 2 */}
                                                <div className="blog-single-comment d-flex gap-4 pt-5 pb-5">
                                                    <div className="image">
                                                        <img src="/assets/img/news/comment-2.png" alt="image" />
                                                    </div>
                                                    <div className="content">
                                                        <div className="head d-flex flex-wrap gap-2 align-items-center justify-content-between">
                                                            <div className="con">
                                                                <h5><a href="#">Alex Flores</a></h5>
                                                                <span>March 20, 2024 at 2:37 pm</span>
                                                            </div>
                                                            <div className="star">
                                                                {[1,2,3,4].map(i => <i key={i} className="fa-solid fa-star"></i>)}
                                                                <i className="fa-regular fa-star"></i>
                                                            </div>
                                                        </div>
                                                        <p className="mt-30 mb-4">Neque porro est qui dolorem ipsum quia quaed inventor veritatis et quasi architecto var sed efficitur turpis gilla sed sit amet finibus eros.</p>
                                                        <a href="#" className="reply">Reply</a>
                                                    </div>
                                                </div>
                                            </div>

                                            {/* Comment Form */}
                                            <div className="comment-form-wrap pt-5">
                                                <h3>Leave a comments</h3>
                                                <form onSubmit={(e) => e.preventDefault()}>
                                                    <div className="row g-4">
                                                        <div className="col-lg-6">
                                                            <div className="form-clt">
                                                                <span>Your Name*</span>
                                                                <input type="text" name="name" placeholder="Your Name" />
                                                            </div>
                                                        </div>
                                                        <div className="col-lg-6">
                                                            <div className="form-clt">
                                                                <span>Your Email*</span>
                                                                <input type="text" name="email" placeholder="Your Email" />
                                                            </div>
                                                        </div>
                                                        <div className="col-lg-12">
                                                            <div className="form-clt">
                                                                <span>Message*</span>
                                                                <textarea name="message" placeholder="Write Message"></textarea>
                                                            </div>
                                                        </div>
                                                        <div className="col-lg-6">
                                                            <button type="submit" className="theme-btn">
                                                                post comment <i className="fa-solid fa-arrow-right-long"></i>
                                                            </button>
                                                        </div>
                                                    </div>
                                                </form>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Right Side: Sidebar (Reused Component) */}
                            <div className="col-xl-3 col-lg-4">
                                <BlogSidebar />
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </Layout>
    );
};

export default BlogDetails;