export function LoadingState({ label = "Cargando información..." }) {
  return (
    <div className="state-panel" role="status">
      <span className="spinner" aria-hidden="true" />
      <p>{label}</p>
    </div>
  );
}

export function ErrorState({ message, onRetry }) {
  return (
    <div className="state-panel error-panel" role="alert">
      <span className="state-symbol" aria-hidden="true">!</span>
      <div>
        <strong>No pudimos cargar esta sección</strong>
        <p>{message}</p>
      </div>
      {onRetry && (
        <button type="button" className="button secondary small" onClick={onRetry}>
          Reintentar
        </button>
      )}
    </div>
  );
}

export function EmptyState({ message }) {
  return (
    <div className="state-panel">
      <span className="state-symbol muted" aria-hidden="true">0</span>
      <p>{message}</p>
    </div>
  );
}
