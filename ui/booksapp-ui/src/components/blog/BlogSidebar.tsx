import { Link } from 'react-router-dom';

const BlogSidebar = () => {
    return (
        <div className="main-sidebar">
            {/* Search Widget */}
            <div className="single-sidebar-widget">
                <div className="wid-title">
                    <h3>Search</h3>
                </div>
                <div className="search-widget">
                    <form onSubmit={(e) => e.preventDefault()}>
                        <input type="text" placeholder="Search here" />
                        <button type="submit"><i className="fa-sharp fa-light fa-magnifying-glass"></i></button>
                    </form>
                </div>
            </div>

            {/* Categories Widget */}
            <div className="single-sidebar-widget">
                <div className="wid-title">
                    <h3>Categories</h3>
                </div>
                <div className="news-widget-categories">
                    <ul>
                        <li><Link to="/blog">Adventure</Link> <span>(5)</span></li>
                        <li><Link to="/blog">Education</Link> <span>(3)</span></li>
                        <li className="active"><Link to="/blog">Romance</Link><span>(6)</span></li>
                        <li><Link to="/blog">Modern Fiction</Link> <span>(2)</span></li>
                        <li><Link to="/blog">Contemporary</Link> <span>(4)</span></li>
                        <li><Link to="/blog">Art & Literature</Link> <span>(7)</span></li>
                    </ul>
                </div>
            </div>

            {/* Recent Posts Widget */}
            <div className="single-sidebar-widget">
                <div className="wid-title">
                    <h3>Recent Post</h3>
                </div>
                <div className="recent-post-area">
                    <div className="recent-items">
                        <div className="recent-thumb">
                            <img src="/assets/img/news/pp3.jpg" alt="img" />
                        </div>
                        <div className="recent-content">
                            <ul>
                                <li><i className="fa-solid fa-calendar-days"></i> 18 Dec, 2024</li>
                            </ul>
                            <h6>
                                <Link to="/blog/1">Top 10 Tarot Decks For The Tarot World Summit</Link>
                            </h6>
                        </div>
                    </div>
                    <div className="recent-items">
                        <div className="recent-thumb">
                            <img src="/assets/img/news/pp4.jpg" alt="img" />
                        </div>
                        <div className="recent-content">
                            <ul>
                                <li><i className="fa-solid fa-calendar-days"></i> Mar 20, 2024</li>
                            </ul>
                            <h6>
                                <Link to="/blog/2">Eu Parturient Dictumst Fames Quam Tempor</Link>
                            </h6>
                        </div>
                    </div>
                    <div className="recent-items">
                        <div className="recent-thumb">
                            <img src="/assets/img/news/pp5.jpg" alt="img" />
                        </div>
                        <div className="recent-content">
                            <ul>
                                <li><i className="fa-solid fa-calendar-days"></i> Mar 10, 2024</li>
                            </ul>
                            <h6>
                                <Link to="/blog/3">Students Intelligence in education in Building..</Link>
                            </h6>
                        </div>
                    </div>
                </div>
            </div>

            {/* Tags Widget */}
            <div className="single-sidebar-widget">
                <div className="wid-title">
                    <h3>Tags</h3>
                </div>
                <div className="news-widget-categories">
                    <div className="tagcloud">
                        <Link to="/blog">Romance</Link>
                        <Link to="/blog">Books</Link>
                        <Link to="/blog">Tips & Tricks</Link>
                        <Link to="/blog">Adventure</Link>
                        <Link to="/blog">Education</Link>
                        <Link to="/blog">Store</Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BlogSidebar;