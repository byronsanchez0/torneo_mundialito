"use client";

import { useEffect, useState } from "react";
import { getHealth, getTeams } from "../lib/api";
import MatchesView from "./MatchesView";
import ResultForm from "./ResultForm";
import StatisticsView from "./StatisticsView";
import TeamsView from "./TeamsView";

export default function Dashboard() {
  const [apiStatus, setApiStatus] = useState("checking");
  const [teams, setTeams] = useState([]);
  const [selectedMatch, setSelectedMatch] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    Promise.all([
      getHealth(controller.signal),
      getTeams(
        { pageNumber: 1, pageSize: 100, sortBy: "name", sortDirection: "asc" },
        controller.signal,
      ),
    ])
      .then(([, teamResult]) => {
        setApiStatus("online");
        setTeams(teamResult.data);
      })
      .catch((error) => {
        if (error.name !== "AbortError") setApiStatus("offline");
      });

    return () => controller.abort();
  }, []);

  function openResultForm(match) {
    setSelectedMatch(match);
    requestAnimationFrame(() => {
      document.getElementById("registrar")?.scrollIntoView({ behavior: "smooth" });
    });
  }

  function refreshTournament() {
    setRefreshKey((value) => value + 1);
  }

  return (
    <>
      <header className="topbar">
        <div className="topbar-inner">
          <a className="brand" href="#inicio" aria-label="Ir al inicio">
            <span className="brand-mark" aria-hidden="true">M</span>
            <span><strong>INFATLAN</strong><small>Mundialito</small></span>
          </a>
          <nav className="main-nav" aria-label="Navegación principal">
            <a href="#equipos">Equipos</a>
            <a href="#partidos">Partidos</a>
            <a href="#posiciones">Posiciones</a>
            <a href="#goleadores">Goleadores</a>
            <a className="nav-action" href="#registrar">Registrar gol</a>
          </nav>
        </div>
      </header>

      <main id="inicio" className="app-shell">
        <section className="hero">
          <div className="hero-copy">
            <span className="hero-tag">Temporada actual</span>
            <h1>El torneo, claro.<br /><em>El fútbol, primero.</em></h1>
            <p>Administra el Mundialito desde un solo lugar: equipos, calendario, resultados y clasificación en tiempo real.</p>
            <div className="hero-actions">
              <a className="button accent" href="#partidos">Ver partidos</a>
              <a className="button hero-secondary" href="#registrar">Registrar resultado</a>
            </div>
          </div>
          <div className="hero-board" aria-label="Resumen de funcionalidades">
            <div className="pitch-lines" aria-hidden="true"><span /></div>
            <div className="hero-score">
              <span>Panel oficial</span>
              <strong>MUNDIALITO</strong>
              <small>Resultados y estadísticas</small>
            </div>
            <div className={`connection-status connection-${apiStatus}`}>
              <span aria-hidden="true" />
              {apiStatus === "online" && "API conectada"}
              {apiStatus === "offline" && "API no disponible"}
              {apiStatus === "checking" && "Comprobando API"}
            </div>
          </div>
        </section>

        <div className="quick-nav" aria-label="Accesos rápidos">
          <a href="#equipos"><strong>01</strong><span>Equipos</span></a>
          <a href="#partidos"><strong>02</strong><span>Partidos</span></a>
          <a href="#posiciones"><strong>03</strong><span>Clasificación</span></a>
          <a href="#registrar"><strong>04</strong><span>Resultados</span></a>
        </div>

        <TeamsView refreshKey={refreshKey} />
        <MatchesView teams={teams} refreshKey={refreshKey} onRegister={openResultForm} />

        <div className="operations-grid">
          <ResultForm initialMatch={selectedMatch} refreshKey={refreshKey} onSaved={refreshTournament} />
          <aside className="panel tournament-note">
            <span className="section-kicker light">Cómo funciona</span>
            <h2>Un gol actualiza todo.</h2>
            <p>Selecciona un partido, el equipo anotador y el jugador. El marcador, la tabla y el ranking de goleadores se refrescan al confirmar.</p>
            <ol>
              <li><span>1</span>Elige el partido</li>
              <li><span>2</span>Selecciona al jugador</li>
              <li><span>3</span>Registra el minuto</li>
            </ol>
          </aside>
        </div>

        <StatisticsView refreshKey={refreshKey} />
      </main>

      <footer>
        <div><strong>INFATLAN Mundialito</strong><span>Panel operativo del torneo</span></div>
        <a href="#inicio">Volver arriba ↑</a>
      </footer>
    </>
  );
}
