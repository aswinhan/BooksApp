import { useEffect, useState } from 'react';

function App() {
  const [message, setMessage] = useState('Loading...');

  useEffect(() => {
    // using the environment variable we just set
    const apiUrl = import.meta.env.VITE_API_URL; 
    
    // Simple fetch to a health check or public endpoint
    // We'll try fetching books. If auth is required, this might return 401, 
    // which still proves connection!
    fetch(`${apiUrl}/api/catalog/books`)
      .then(response => {
        if (response.ok) return "Connected to API! 🚀";
        return `Connected, but API returned ${response.status}`;
      })
      .then(data => setMessage(data))
      .catch(error => setMessage(`Error connecting: ${error.message}`));
  }, []);

  return (
    <div style={{ padding: '2rem', fontFamily: 'sans-serif' }}>
      <h1>BooksApp UI</h1>
      <div style={{ 
        padding: '1rem', 
        backgroundColor: '#f0f0f0', 
        borderRadius: '8px',
        marginTop: '1rem' 
      }}>
        <strong>Backend Status:</strong> {message}
      </div>
    </div>
  );
}

export default App;