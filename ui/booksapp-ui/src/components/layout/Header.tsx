import { Link } from "react-router-dom";
import { useEffect, useState } from "react";

interface HeaderProps {
  onOpenOffcanvas: () => void;
}

const Header = ({ onOpenOffcanvas }: HeaderProps) => {
  const [isSticky, setIsSticky] = useState(false);

  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY > 150) {
        // Slightly increased threshold
        setIsSticky(true);
      } else {
        setIsSticky(false);
      }
    };
    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  // Reusable Content Component
  const HeaderContent = () => (
    <div className="container">
      <div className="row">
        <div className="col-6 col-md-6 col-lg-10 col-xl-8 col-xxl-10">
          <div className="header-left">
            <div className="logo">
              <Link to="/" className="header-logo">
                <img src="/assets/img/logo/white-logo.svg" alt="logo-img" />
              </Link>
            </div>
            <div className="mean__menu-wrapper">
              <div className="main-menu">
                <nav>
                  <ul>
                    <li>
                      <Link to="/">Home</Link>
                    </li>
                    <li>
                      <Link to="/shop">
                        Shop <i className="fas fa-angle-down"></i>
                      </Link>
                      <ul className="submenu">
                        <li>
                          <Link to="/shop">Books Shop</Link>
                        </li>
                        <li>
                          <Link to="/cart">Cart</Link>
                        </li>
                        <li>
                          <Link to="/wishlist">Wishlist</Link>
                        </li>
                        <li>
                          <Link to="/checkout">Checkout</Link>
                        </li>
                      </ul>
                    </li>
                    <li className="has-dropdown">
                      <Link to="/about">
                        Pages <i className="fas fa-angle-down"></i>
                      </Link>
                      <ul className="submenu">
                        <li>
                          <Link to="/about">About Us</Link>
                        </li>
                        <li>
                          <Link to="/author">Author</Link>
                        </li>
                        <li>
                          <Link to="/faq">Faq's</Link>
                        </li>
                      </ul>
                    </li>
                    <li>
                      <Link to="/news">
                        Blog <i className="fas fa-angle-down"></i>
                      </Link>
                      <ul className="submenu">
                        <li>
                          <Link to="/news">Blog List</Link>
                        </li>
                        <li>
                          <Link to="/news-details">Blog Details</Link>
                        </li>
                      </ul>
                    </li>
                    <li>
                      <Link to="/contact">Contact</Link>
                    </li>
                  </ul>
                </nav>
              </div>
            </div>
          </div>
        </div>
        <div className="col-6 col-md-6 col-lg-2 col-xl-4 col-xxl-2">
          <div className="header-right">
            <div className="menu-cart">
              <Link to="/wishlist" className="cart-icon">
                <i className="fa-regular fa-heart"></i>
              </Link>
              <Link to="/cart" className="cart-icon">
                <i className="fa-regular fa-cart-shopping"></i>
              </Link>
              <div className="header-humbager ml-30">
                {/* IMPORTANT: This button triggers the Offcanvas */}
                <a
                  className="sidebar__toggle"
                  href="#"
                  onClick={(e) => {
                    e.preventDefault();
                    onOpenOffcanvas();
                  }}
                >
                  <div className="bar-icon-2">
                    <img src="/assets/img/icon/icon-13.svg" alt="img" />
                  </div>
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );

  return (
    <>
      {/* Top Header */}
      <div className="header-top-1">
        <div className="container">
          <div className="header-top-wrapper">
            <ul className="contact-list">
              <li>
                <i className="fa-regular fa-phone"></i>{" "}
                <a href="tel:+919791733140">+91 9791733140</a>
              </li>
              <li>
                <i className="far fa-envelope"></i>{" "}
                <a href="mailto:info@example.com">info@example.com</a>
              </li>
              <li>
                <i className="far fa-clock"></i>{" "}
                <span>Sunday - Fri: 9 aM - 6 pM</span>
              </li>
            </ul>
            <ul className="list">
              <li>
                <i className="fa-light fa-comments"></i>
                <Link to="/contact">Live Chat</Link>
              </li>
              <li>
                <i className="fa-light fa-user"></i>
                <button data-bs-toggle="modal" data-bs-target="#loginModal">
                  Login
                </button>
              </li>
            </ul>
          </div>
        </div>
      </div>

      {/* Main Header (Relative) */}
      <header className="header-1">
        <div className="mega-menu-wrapper">
          <div className="header-main">
            <HeaderContent />
          </div>
        </div>
      </header>

      {/* Sticky Header (Fixed) */}
      <header
        className={`header-1 sticky-header ${isSticky ? "sticky-on" : ""}`}
      >
        <div className="mega-menu-wrapper">
          <div className="header-main">
            <HeaderContent />
          </div>
        </div>
      </header>
    </>
  );
};

export default Header;
