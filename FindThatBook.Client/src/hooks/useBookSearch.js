import { useState } from 'react';
import axios from 'axios';

const API_URL = 'http://localhost:5063/api/v1/books/search';

export const useBookSearch = () => {
    const [books, setBooks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [hasSearched, setHasSearched] = useState(false);

    const searchBooks = async (query) => {
        if (!query.trim()) return;

        setLoading(true);
        setError('');
        setBooks([]);
        setHasSearched(true);

        try {
            const { data } = await axios.get(`${API_URL}?query=${encodeURIComponent(query)}`);

            if (data.success) {
                setBooks(data.data);
            } else {
                setError(data.message || 'Unknown error occurred.');
            }
        } catch (err) {
            setError('Could not reach the server. Is the backend running?');
        } finally {
            setLoading(false);
        }
    };

    return { books, loading, error, hasSearched, searchBooks };
};