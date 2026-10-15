"use client";

import { useEffect, useMemo, useState } from "react";
import { getMatches, getPlayers, registerGoal } from "../lib/api";
import { formatDate } from "../lib/formatters";
import { ErrorState, LoadingState } from "./StatePanel";

export default function ResultForm({ initialMatch, refreshKey, onSaved }) {
  const [matches, setMatches] = useState([]);
  const [matchesError, setMatchesError] = useState("");
  const [loadingMatches, setLoadingMatches] = useState(true);
  const [matchId, setMatchId] = useState("");
  const [teamId, setTeamId] = useState("");
  const [players, setPlayers] = useState([]);
  const [playerId, setPlayerId] = useState("");
  const [minute, setMinute] = useState("");
  const [loadingPlayers, setLoadingPlayers] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoadingMatches(true);
    setMatchesError("");

    getMatches(
      {
        pageNumber: 1,
        pageSize: 100,
        sortBy: "scheduledAtUtc",
        sortDirection: "asc",
      },
      controller.signal,
    )
      .then((result) => setMatches(result.data.filter((match) => match.status !== "Finished")))
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setMatchesError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoadingMatches(false);
      });

    return () => controller.abort();
  }, [reloadKey, refreshKey]);

  useEffect(() => {
    if (!initialMatch) return;
    setMatchId(initialMatch.id);
    setTeamId("");
    setPlayerId("");
    setSuccess("");
    setError("");
  }, [initialMatch]);

  useEffect(() => {
    if (!teamId) {
      setPlayers([]);
      setPlayerId("");
      return;
    }

    const controller = new AbortController();
    setLoadingPlayers(true);
    setError("");

    getPlayers(
      {
        pageNumber: 1,
        pageSize: 100,
        sortBy: "shirtNumber",
        sortDirection: "asc",
        teamId,
      },
      controller.signal,
    )
      .then((result) => setPlayers(result.data))
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoadingPlayers(false);
      });

    return () => controller.abort();
  }, [teamId]);

  const matchOptions = useMemo(() => {
    if (initialMatch && !matches.some((match) => match.id === initialMatch.id)) {
      return [initialMatch, ...matches];
    }
    return matches;
  }, [initialMatch, matches]);

  const selectedMatch = matchOptions.find((match) => match.id === matchId);
  const availableTeams = selectedMatch
    ? [
        { id: selectedMatch.homeTeamId, name: selectedMatch.homeTeamName },
        { id: selectedMatch.awayTeamId, name: selectedMatch.awayTeamName },
      ]
    : [];

  function changeMatch(value) {
    setMatchId(value);
    setTeamId("");
    setPlayerId("");
    setSuccess("");
    setError("");
  }

  async function submitResult(event) {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (!matchId || !teamId || !playerId || !minute) {
      setError("Completa partido, equipo, jugador y minuto.");
      return;
    }

    setSubmitting(true);
    try {
      await registerGoal(matchId, {
        playerId,
        teamId,
        minute: Number(minute),
      });
      setSuccess("Gol registrado correctamente. El marcador ya fue actualizado.");
      setMinute("");
      onSaved();
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section id="registrar" className="panel section-panel result-panel">
      <div className="section-heading compact">
        <div>
          <span className="section-kicker">Mesa de control</span>
          <h2>Registrar resultado</h2>
          <p>Agrega cada gol al partido y actualiza automáticamente las estadísticas.</p>
        </div>
      </div>

      {loadingMatches && <LoadingState label="Cargando partidos disponibles..." />}
      {!loadingMatches && matchesError && (
        <ErrorState message={matchesError} onRetry={() => setReloadKey((value) => value + 1)} />
      )}
      {!loadingMatches && !matchesError && (
        <form className="result-form" onSubmit={submitResult}>
          <label className="wide-field">
            Partido
            <select value={matchId} onChange={(event) => changeMatch(event.target.value)} required>
              <option value="">Selecciona un partido</option>
              {matchOptions.map((match) => (
                <option key={match.id} value={match.id}>
                  {match.homeTeamName} vs {match.awayTeamName} · {formatDate(match.scheduledAtUtc)}
                </option>
              ))}
            </select>
          </label>

          <label>
            Equipo anotador
            <select
              value={teamId}
              onChange={(event) => { setTeamId(event.target.value); setPlayerId(""); }}
              disabled={!matchId}
              required
            >
              <option value="">Selecciona un equipo</option>
              {availableTeams.map((team) => <option key={team.id} value={team.id}>{team.name}</option>)}
            </select>
          </label>

          <label>
            Jugador
            <select value={playerId} onChange={(event) => setPlayerId(event.target.value)} disabled={!teamId || loadingPlayers} required>
              <option value="">{loadingPlayers ? "Cargando jugadores..." : "Selecciona un jugador"}</option>
              {players.map((player) => (
                <option key={player.id} value={player.id}>
                  #{player.shirtNumber} · {player.firstName} {player.lastName}
                </option>
              ))}
            </select>
          </label>

          <label>
            Minuto
            <input
              type="number"
              min="1"
              max="120"
              value={minute}
              onChange={(event) => setMinute(event.target.value)}
              placeholder="Ej. 67"
              required
            />
          </label>

          <button type="submit" className="button accent submit-result" disabled={submitting || matchOptions.length === 0}>
            {submitting ? "Registrando..." : "Registrar gol"}
          </button>

          {matchOptions.length === 0 && (
            <p className="form-message neutral">No hay partidos disponibles para registrar goles.</p>
          )}
          {error && <p className="form-message error" role="alert">{error}</p>}
          {success && <p className="form-message success" role="status">{success}</p>}
        </form>
      )}
    </section>
  );
}
