import { useState } from 'react';
import { useBookSearch } from '../hooks/useBookSearch';
import BookCard from './BookCard';
import './BookSearch.css';

// Small UI Helpers
const SearchIcon = () => <svg className="search-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>;

const ErrorIcon = () => <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>;

export default function BookSearch() {
  const [query, setQuery] = useState('');
  const { books, loading, error, hasSearched, searchBooks } = useBookSearch();

  const handleSearch = (e) => {
    e.preventDefault();
    if (query.trim()) searchBooks(query);
  };

  return (
    <main className="app-container">
      <div className="content-wrapper">
        <HeaderSection />

        <form onSubmit={handleSearch} className="search-container">
          <div className="input-wrapper">
            <SearchIcon />
            <input
              type="text"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="e.g. 'Dickens Tale Two Cities', 'Austen Bennet', or 'Hobbit 1937'"
              className="search-input"
              disabled={loading}
            />
          </div>
          <button type="submit" className="search-button" disabled={loading || !query.trim()}>
            {loading ? <span className="spinner" /> : "Search"}
          </button>
        </form>

        <section className="results-section">
          {error && <div className="state-message error"><ErrorIcon />{error}</div>}
          
          {loading && <LoadingSkeleton />}

          {!loading && hasSearched && books.length === 0 && !error && (
            <div className="state-message empty">
              <p>No matches found. Try describing with other words.</p>
            </div>
          )}

          {!loading && (
            <div className="books-grid">
              {books.map((book, idx) => (
                <BookCard key={`${book.title}-${idx}`} book={book} />
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}

function HeaderSection() {
  return (
    <header className="hero-section">
      <h1 className="brand-title">
        <span className="text-gradient">Find That Book</span>
      </h1>
      <p className="subtitle">Search by title, author, or details like plot, character, or idea.</p>
    </header>
  );
}

function LoadingSkeleton() {
  return (
    <div className="skeleton-grid">
      {[1, 2, 3].map((i) => <div key={i} className="skeleton-card" />)}
    </div>
  );
}