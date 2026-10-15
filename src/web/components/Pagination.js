export default function Pagination({ pageNumber, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;

  return (
    <nav className="pagination" aria-label="Paginación">
      <button
        type="button"
        className="button secondary small"
        disabled={pageNumber <= 1}
        onClick={() => onPageChange(pageNumber - 1)}
      >
        Anterior
      </button>
      <span>
        Página <strong>{pageNumber}</strong> de <strong>{totalPages}</strong>
      </span>
      <button
        type="button"
        className="button secondary small"
        disabled={pageNumber >= totalPages}
        onClick={() => onPageChange(pageNumber + 1)}
      >
        Siguiente
      </button>
    </nav>
  );
}
