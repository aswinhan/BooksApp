import { useEffect, useState } from 'react';
import type { Book } from './types/Book';
import Layout from './components/layout/Layout';
import BookCard from './components/catalog/BookCard';
import Breadcrumb from './components/common/Breadcrumb';

function App() {
  const [books, setBooks] = useState<Book[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const apiUrl = import.meta.env.VITE_API_URL;

    fetch(`${apiUrl}/api/catalog/books?page=1&pageSize=10`)
      .then(response => {
        if (!response.ok) throw new Error('Failed to fetch books');
        return response.json();
      })
      .then(data => {
        // Handle both PagedList { items: [] } and direct Array []
        const bookList = data.items ? data.items : data;
        setBooks(bookList);
        setLoading(false);
      })
      .catch(err => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  return (
    <Layout>
      {/* 1. The Breadcrumb Section */}
      <Breadcrumb title="Books Shop" activePage="Shop" />

      {/* 2. The Main Shop Section */}
      <section className="shop-section section-padding fix">
        <div className="container">
          <div className="row g-4">
             {/* Loading State */}
            {loading && (
              <div className="col-12 text-center">
                <h3>Loading your library...</h3>
              </div>
            )}

            {/* Error State */}
            {error && (
              <div className="col-12 text-center text-danger">
                <h3>Error: {error}</h3>
              </div>
            )}

            {/* 3. The Book Grid */}
            {!loading && !error && books.map(book => (
              <BookCard key={book.id} book={book} />
            ))}
            
            {/* Empty State */}
            {!loading && !error && books.length === 0 && (
                <div className="col-12 text-center">
                    <h3>No books found in the catalog.</h3>
                </div>
            )}
          </div>
        </div>
      </section>
      
    </Layout>
  );
}

export default App;