"use client";

import { useEffect, useState } from "react";
import { getMatches } from "../lib/api";
import { formatDate, formatStatus } from "../lib/formatters";
import Pagination from "./Pagination";
import { EmptyState, ErrorState, LoadingState } from "./StatePanel";

const initialFilters = { teamId: "", status: "", fromDate: "", toDate: "" };

export default function MatchesView({ teams, refreshKey, onRegister }) {
  const [draft, setDraft] = useState(initialFilters);
  const [filters, setFilters] = useState(initialFilters);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [sortBy, setSortBy] = useState("scheduledAtUtc");
  const [sortDirection, setSortDirection] = useState("desc");
  const [reloadKey, setReloadKey] = useState(0);
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError("");

    getMatches(
      {
        pageNumber,
        pageSize,
        sortBy,
        sortDirection,
        ...filters,
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
    <section id="partidos" className="panel section-panel">
      <div className="section-heading">
        <div>
          <span className="section-kicker">Calendario</span>
          <h2>Partidos</h2>
          <p>Revisa fechas, estados y marcadores del campeonato.</p>
        </div>
        {result && <span className="record-count">{result.totalRecords} partidos</span>}
      </div>

      <form className="filters match-filters" onSubmit={applyFilters}>
        <label>
          Equipo
          <select value={draft.teamId} onChange={(event) => setDraft({ ...draft, teamId: event.target.value })}>
            <option value="">Todos</option>
            {teams.map((team) => <option key={team.id} value={team.id}>{team.name}</option>)}
          </select>
        </label>
        <label>
          Estado
          <select value={draft.status} onChange={(event) => setDraft({ ...draft, status: event.target.value })}>
            <option value="">Todos</option>
            <option value="Scheduled">Programado</option>
            <option value="InProgress">En juego</option>
            <option value="Finished">Finalizado</option>
          </select>
        </label>
        <label>
          Desde
          <input type="date" value={draft.fromDate} onChange={(event) => setDraft({ ...draft, fromDate: event.target.value })} />
        </label>
        <label>
          Hasta
          <input type="date" value={draft.toDate} onChange={(event) => setDraft({ ...draft, toDate: event.target.value })} />
        </label>
        <label>
          Ordenar por
          <select value={sortBy} onChange={(event) => { setSortBy(event.target.value); setPageNumber(1); }}>
            <option value="scheduledAtUtc">Fecha</option>
            <option value="status">Estado</option>
            <option value="homeTeamName">Equipo local</option>
            <option value="awayTeamName">Equipo visitante</option>
            <option value="homeScore">Goles local</option>
            <option value="awayScore">Goles visitante</option>
          </select>
        </label>
        <label>
          Dirección
          <select value={sortDirection} onChange={(event) => { setSortDirection(event.target.value); setPageNumber(1); }}>
            <option value="desc">Descendente</option>
            <option value="asc">Ascendente</option>
          </select>
        </label>
        <div className="filter-actions">
          <button type="submit" className="button primary">Aplicar</button>
          <button type="button" className="button ghost" onClick={clearFilters}>Limpiar</button>
        </div>
      </form>

      {loading && <LoadingState label="Cargando partidos..." />}
      {!loading && error && <ErrorState message={error} onRetry={() => setReloadKey((value) => value + 1)} />}
      {!loading && !error && result?.data.length === 0 && (
        <EmptyState message="No hay partidos que coincidan con los filtros." />
      )}
      {!loading && !error && result?.data.length > 0 && (
        <>
          <div className="table-scroll">
            <table>
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Partido</th>
                  <th className="center">Marcador</th>
                  <th>Estado</th>
                  <th><span className="sr-only">Acciones</span></th>
                </tr>
              </thead>
              <tbody>
                {result.data.map((match) => (
                  <tr key={match.id}>
                    <td className="date-cell">{formatDate(match.scheduledAtUtc)}</td>
                    <td>
                      <div className="fixture">
                        <strong>{match.homeTeamName}</strong>
                        <span>vs</span>
                        <strong>{match.awayTeamName}</strong>
                      </div>
                    </td>
                    <td className="score-cell">{match.homeScore} <span>—</span> {match.awayScore}</td>
                    <td><span className={`status-badge status-${match.status.toLowerCase()}`}>{formatStatus(match.status)}</span></td>
                    <td className="action-cell">
                      {match.status !== "Finished" && (
                        <button type="button" className="text-button" onClick={() => onRegister(match)}>
                          Registrar gol
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
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
