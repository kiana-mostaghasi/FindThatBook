import './BookCard.css';

const Icons = {
  Book: () => <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" /><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z" /></svg>,
  External: () => <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" /><polyline points="15 3 21 3 21 9" /><line x1="10" y1="14" x2="21" y2="3" /></svg>,
  Layers: () => <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" /></svg>
};

const BookCover = ({ url, title }) => (
  <div className="cover-wrapper">
    {url ? (
      <img src={url} alt={`Cover of ${title}`} className="cover" loading="lazy" />
    ) : (
      <div className="cover placeholder"><Icons.Book /></div>
    )}
  </div>
);

const BookCard = ({ book }) => {
  const { title, author, coverUrl, openLibraryUrl, firstPublishYear, explanation } = book;

  return (
    <article className="book-card">
      <BookCover url={coverUrl} title={title} />
      
      <div className="content">
        <header>
          <h2 className="title">{title}</h2>
          <div className="meta">
            <span className="author">{author}</span>
            
            {openLibraryUrl && (
              <a href={openLibraryUrl} target="_blank" rel="noopener noreferrer" className="external-link" aria-label="View on Open Library">
                <Icons.External />
              </a>
            )}

            {firstPublishYear && (
              <>
                <span className="separator">•</span>
                <span className="year">{firstPublishYear}</span>
              </>
            )}
          </div>
        </header>
        
        {explanation && (
          <div className="reason">
            <div className="reason-label">
              <Icons.Layers /> AI Match
            </div>
            <p>{explanation}</p>
          </div>
        )}
      </div>
    </article>
  );
};

export default BookCard;