import type { Book } from '../../types/Book';
import { Link } from 'react-router-dom';

interface BookCardProps {
    book: Book;
}

const BookCard = ({ book }: BookCardProps) => {
    // Fallback image if none provided
    const imageSrc = book.imageUrl ? book.imageUrl : '/assets/img/book/01.png';

    return (
        <div className="col-xl-3 col-lg-4 col-md-6 wow fadeInUp" data-wow-delay=".2s">
            <div className="shop-box-items">
                <div className="book-thumb center">
                    <Link to={`/books/${book.id}`}>
                        <img src={imageSrc} alt={book.title} />
                    </Link>
                    <ul className="post-box">
                        <li>Hot</li>
                        <li>-30%</li>
                    </ul>
                    <ul className="shop-icon d-grid justify-content-center align-items-center">
                        <li>
                            <Link to="/wishlist"><i className="far fa-heart"></i></Link>
                        </li>
                        <li>
                            <Link to="/cart">
                                <img className="icon" src="/assets/img/icon/shuffle.svg" alt="svg-icon" />
                            </Link>
                        </li>
                        <li>
                            <Link to={`/books/${book.id}`}><i className="far fa-eye"></i></Link>
                        </li>
                    </ul>
                </div>
                <div className="shop-content">
                    <h3><Link to={`/books/${book.id}`}>{book.title}</Link></h3>
                    <ul className="price-list">
                        <li>${book.price.toFixed(2)}</li>
                        <li>
                            <i className="fa-solid fa-star"></i>
                            {/* Hardcoded rating for now, backend needs to send this */}
                            4.5 (25)
                        </li>
                    </ul>
                    <div className="shop-button">
                        <Link to="/cart" className="theme-btn">
                            <i className="fa-solid fa-basket-shopping"></i> Add To Cart
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BookCard;