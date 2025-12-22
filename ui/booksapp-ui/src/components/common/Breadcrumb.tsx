import { Link } from 'react-router-dom';

interface BreadcrumbProps {
    title: string;
    activePage: string;
}

const Breadcrumb = ({ title, activePage }: BreadcrumbProps) => {
    return (
        <div className="breadcrumb-wrapper">
            <div className="book1">
                <img src="/assets/img/hero/book1.png" alt="book" />
            </div>
            <div className="book2">
                <img src="/assets/img/hero/book2.png" alt="book" />
            </div>
            <div className="container">
                <div className="page-heading">
                    <h1>{title}</h1>
                    <div className="page-header">
                        <ul className="breadcrumb-items wow fadeInUp" data-wow-delay=".3s">
                            <li>
                                <Link to="/">Home</Link>
                            </li>
                            <li>
                                <i className="fa-solid fa-chevron-right"></i>
                            </li>
                            <li>
                                {activePage}
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Breadcrumb;