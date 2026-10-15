import "./globals.css";

export const metadata = {
  title: "INFATLAN Mundialito | Panel del torneo",
  description: "Equipos, partidos, resultados y estadísticas del Mundialito"
};

export default function RootLayout({ children }) {
  return (
    <html lang="es">
      <body>{children}</body>
    </html>
  );
}

