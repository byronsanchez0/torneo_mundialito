"use client";

import { useEffect, useState } from "react";
import { getTeams } from "../lib/api";
import Pagination from "./Pagination";
import { EmptyState, ErrorState, LoadingState } from "./StatePanel";

const initialFilters = { name: "", coach: "" };

export default function TeamsView({ refreshKey }) {
  const [draft, setDraft] = useState(initialFilters);
  const [filters, setFilters] = useState(initialFilters);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState("name");
  const [sortDirection, setSortDirection] = useState("asc");
  const [reloadKey, setReloadKey] = useState(0);
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError("");

    getTeams(
      {
        pageNumber,
        pageSize,
        sortBy,
        sortDirection,
        name: filters.name,
        coach: filters.coach,
      },
      controller.signal,
    )
      .then(setResult)
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [filters, pageNumber, pageSize, sortBy, sortDirection, reloadKey, refreshKey]);

  function applyFilters(event) {
    event.preventDefault();
    setPageNumber(1);
    setFilters({ ...draft });
    setReloadKey((value) => value + 1);
  }

  function clearFilters() {
    setDraft(initialFilters);
    setFilters(initialFilters);
    setPageNumber(1);
    setReloadKey((value) => value + 1);
  }

  return (
    <section id="equipos" className="panel section-panel">
      <div className="section-heading">
        <div>
          <span className="section-kicker">Planteles</span>
          <h2>Equipos</h2>
          <p>Consulta equipos, entrenadores y participación en el torneo.</p>
        </div>
        {result && <span className="record-count">{result.totalRecords} equipos</span>}
      </div>

      <form className="filters" onSubmit={applyFilters}>
        <label>
          Nombre
          <input
            value={draft.name}
            onChange={(event) => setDraft({ ...draft, name: event.target.value })}
            placeholder="Buscar equipo"
          />
        </label>
        <label>
          Entrenador
          <input
            value={draft.coach}
            onChange={(event) => setDraft({ ...draft, coach: event.target.value })}
            placeholder="Buscar entrenador"
          />
        </label>
        <label>
          Ordenar por
          <select value={sortBy} onChange={(event) => { setSortBy(event.target.value); setPageNumber(1); }}>
            <option value="name">Nombre</option>
            <option value="coach">Entrenador</option>
            <option value="playerCount">Jugadores</option>
            <option value="matchCount">Partidos</option>
          </select>
        </label>
        <label>
          Dirección
          <select value={sortDirection} onChange={(event) => { setSortDirection(event.target.value); setPageNumber(1); }}>
            <option value="asc">Ascendente</option>
            <option value="desc">Descendente</option>
          </select>
        </label>
        <div className="filter-actions">
          <button type="submit" className="button primary">Aplicar</button>
          <button type="button" className="button ghost" onClick={clearFilters}>Limpiar</button>
        </div>
      </form>

      {loading && <LoadingState label="Cargando equipos..." />}
      {!loading && error && <ErrorState message={error} onRetry={() => setReloadKey((value) => value + 1)} />}
      {!loading && !error && result?.data.length === 0 && (
        <EmptyState message="No hay equipos que coincidan con los filtros." />
      )}
      {!loading && !error && result?.data.length > 0 && (
        <>
          <div className="team-grid">
            {result.data.map((team, index) => (
              <article className="team-card" key={team.id}>
                <div className="team-mark" aria-hidden="true">{team.name.slice(0, 2).toUpperCase()}</div>
                <div className="team-copy">
                  <span className="muted-label">#{(pageNumber - 1) * pageSize + index + 1}</span>
                  <h3>{team.name}</h3>
                  <p>{team.coach || "Sin entrenador asignado"}</p>
                </div>
                <dl className="team-stats">
                  <div><dt>Jugadores</dt><dd>{team.playerCount}</dd></div>
                  <div><dt>Partidos</dt><dd>{team.matchCount}</dd></div>
                </dl>
              </article>
            ))}
          </div>
          <div className="table-footer">
            <label className="page-size">
              Mostrar
              <select value={pageSize} onChange={(event) => { setPageSize(Number(event.target.value)); setPageNumber(1); }}>
                <option value="5">5</option>
                <option value="10">10</option>
                <option value="20">20</option>
              </select>
            </label>
            <Pagination pageNumber={result.pageNumber} totalPages={result.totalPages} onPageChange={setPageNumber} />
          </div>
        </>
      )}
    </section>
  );
}
