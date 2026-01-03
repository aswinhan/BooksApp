import { Link } from 'react-router-dom';
import Layout from '../components/layout/Layout';
import Breadcrumb from '../components/common/Breadcrumb';
import BlogSidebar from '../components/blog/BlogSidebar';

const blogPosts = [
    {
        id: 1,
        title: "Top 10 Tarot Decks For The Tarot world Summit",
        category: "Educations",
        date: "Feb 10, 2024",
        author: "admin",
        img: "/assets/img/news/post-1.jpg",
        excerpt: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris efficitur et ipsum ut volutpat. Morbi a mollis felis. Nam consectetur lectus vel lorem facilisis."
    },
    {
        id: 2,
        title: "Eu parturient dictumst fames quam tempor",
        category: "Books Store",
        date: "Feb 10, 2024",
        author: "admin",
        img: "/assets/img/news/post-2.jpg",
        excerpt: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris efficitur et ipsum ut volutpat. Morbi a mollis felis. Nam consectetur lectus vel lorem facilisis."
    },
    {
        id: 3,
        title: "All Inclusive Ultimate Circle Island Day with Lunch",
        category: "Activities",
        date: "Feb 10, 2024",
        author: "admin",
        img: "/assets/img/news/post-3.jpg",
        excerpt: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris efficitur et ipsum ut volutpat. Morbi a mollis felis. Nam consectetur lectus vel lorem facilisis."
    }
];

const Blog = () => {
    return (
        <Layout>
            <Breadcrumb title="Blog Standard" activePage="Blog" />

            <section className="news-standard fix section-padding">
                <div className="container">
                    <div className="row g-4">
                        {/* Left Side: Blog Posts */}
                        <div className="col-xl-9 col-lg-8">
                            <div className="news-standard-wrapper">
                                {blogPosts.map(post => (
                                    <div className="news-standard-items" key={post.id}>
                                        <div className="news-thumb">
                                            <img src={post.img} alt="img" />
                                            <div className="post">
                                                <span>{post.category}</span>
                                            </div>
                                        </div>
                                        <div className="news-content">
                                            <ul>
                                                <li>
                                                    <i className="fas fa-calendar-alt"></i> {post.date}
                                                </li>
                                                <li>
                                                    <i className="far fa-user"></i> By {post.author}
                                                </li>
                                            </ul>
                                            <h3>
                                                <Link to={`/blog/${post.id}`}>{post.title}</Link>
                                            </h3>
                                            <p>{post.excerpt}</p>
                                            <Link to={`/blog/${post.id}`} className="theme-btn mt-4">
                                                Read More <i className="fa-solid fa-arrow-right-long"></i>
                                            </Link>
                                        </div>
                                    </div>
                                ))}

                                {/* Pagination */}
                                <div className="page-nav-wrap text-center">
                                    <ul>
                                        <li><Link className="previous" to="#">Previous</Link></li>
                                        <li><Link className="page-numbers" to="#">1</Link></li>
                                        <li><Link className="page-numbers" to="#">2</Link></li>
                                        <li><Link className="page-numbers" to="#">3</Link></li>
                                        <li><Link className="page-numbers" to="#">...</Link></li>
                                        <li><Link className="next" to="#">Next</Link></li>
                                    </ul>
                                </div>
                            </div>
                        </div>

                        {/* Right Side: Sidebar */}
                        <div className="col-xl-3 col-lg-4">
                            <BlogSidebar />
                        </div>
                    </div>
                </div>
            </section>
        </Layout>
    );
};

export default Blog;