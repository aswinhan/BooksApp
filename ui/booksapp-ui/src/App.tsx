// src/App.tsx
import { Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import Wishlist from './pages/Wishlist';
import Contact from './pages/Contact';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import Faq from './pages/Faq';
import NotFound from './pages/NotFound';
import ShopDetails from './pages/ShopDetails';
import About from './pages/About';
import Blog from './pages/Blog';
import BlogDetails from './pages/BlogDetails';

function App() {
  return (
    <Routes>
      {/* The "/" path loads the Home component */}
      <Route path="/" element={<Home />} />
      <Route path="/blog" element={<Blog />} />
      <Route path="/blog/:id" element={<BlogDetails />} />
      <Route path="/about" element={<About />} />
      {/* The "/wishlist" path loads the Wishlist component */}
      <Route path="/wishlist" element={<Wishlist />} />
      <Route path="/contact" element={<Contact />} />
      <Route path="/cart" element={<Cart />} />
      <Route path="/checkout" element={<Checkout />} />
      <Route path="/faq" element={<Faq />} />
      <Route path="/books/:id" element={<ShopDetails />} />

      {/* Catch-all route for 404 Not Found */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

export default App;