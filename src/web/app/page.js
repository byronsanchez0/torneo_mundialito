"use client";

import { useEffect, useState } from "react";

const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8081";

export default function Home() {
  const [apiStatus, setApiStatus] = useState("Comprobando...");

  useEffect(() => {
    fetch(`${apiUrl}/health`)
      .then((response) => {
        if (!response.ok) throw new Error();
        setApiStatus("Conectada");
      })
      .catch(() => setApiStatus("No disponible"));
  }, []);

  return (
    <main>
      <header>
        <p className="eyebrow">Torneo de fútbol</p>
        <h1>INFATLAN Mundialito</h1>
        <p>Base inicial para administrar equipos, jugadores, partidos y goles.</p>
      </header>

      <section className="cards" aria-label="Módulos">
        <article><h2>Equipos</h2><p>Registro de equipos y entrenadores.</p></article>
        <article><h2>Jugadores</h2><p>Plantillas, dorsales y posiciones.</p></article>
        <article><h2>Partidos</h2><p>Calendario y goles del torneo.</p></article>
      </section>

      <p className="status">Estado de la API: <strong>{apiStatus}</strong></p>
    </main>
  );
}

