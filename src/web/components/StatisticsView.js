"use client";

import { useEffect, useState } from "react";
import { getStatistics } from "../lib/api";
import { formatGoalDifference } from "../lib/formatters";
import { EmptyState, ErrorState, LoadingState } from "./StatePanel";

export default function StatisticsView({ refreshKey }) {
  const [statistics, setStatistics] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError("");

    getStatistics(controller.signal)
      .then(setStatistics)
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [reloadKey, refreshKey]);

  if (loading) return <LoadingState label="Calculando estadísticas..." />;
  if (error) return <ErrorState message={error} onRetry={() => setReloadKey((value) => value + 1)} />;

  return (
    <div className="statistics-grid">
      <section id="posiciones" className="panel section-panel standings-panel">
        <div className="section-heading compact">
          <div>
            <span className="section-kicker">Clasificación</span>
            <h2>Tabla de posiciones</h2>
          </div>
          <span className="legend">3 pts por victoria</span>
        </div>

        {statistics?.standings.length === 0 ? (
          <EmptyState message="Aún no hay equipos para clasificar." />
        ) : (
          <div className="table-scroll">
            <table className="standings-table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Equipo</th>
                  <th title="Partidos jugados">PJ</th>
                  <th title="Ganados">G</th>
                  <th title="Empatados">E</th>
                  <th title="Perdidos">P</th>
                  <th title="Goles a favor">GF</th>
                  <th title="Goles en contra">GC</th>
                  <th title="Diferencia de goles">DG</th>
                  <th>PTS</th>
                </tr>
              </thead>
              <tbody>
                {statistics.standings.map((team, index) => (
                  <tr key={team.teamId}>
                    <td><span className={`rank rank-${index + 1}`}>{index + 1}</span></td>
                    <td><strong>{team.teamName}</strong></td>
                    <td>{team.played}</td>
                    <td>{team.won}</td>
                    <td>{team.drawn}</td>
                    <td>{team.lost}</td>
                    <td>{team.goalsFor}</td>
                    <td>{team.goalsAgainst}</td>
                    <td>{formatGoalDifference(team.goalDifference)}</td>
                    <td><strong>{team.points}</strong></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section id="goleadores" className="panel section-panel scorers-panel">
        <div className="section-heading compact">
          <div>
            <span className="section-kicker">Rendimiento individual</span>
            <h2>Goleadores</h2>
          </div>
        </div>

        {statistics?.topScorers.length === 0 ? (
          <EmptyState message="Todavía no se han registrado goles." />
        ) : (
          <ol className="scorer-list">
            {statistics.topScorers.map((player, index) => (
              <li key={player.playerId}>
                <span className={`scorer-rank scorer-rank-${index + 1}`}>{index + 1}</span>
                <div>
                  <strong>{player.playerName}</strong>
                  <span>{player.teamName}</span>
                </div>
                <span className="goal-total"><strong>{player.goals}</strong> gol{player.goals === 1 ? "" : "es"}</span>
              </li>
            ))}
          </ol>
        )}
      </section>
    </div>
  );
}
