import "./globals.css";

export const metadata = {
  title: "INFATLAN Mundialito",
  description: "Administración del torneo INFATLAN"
};

export default function RootLayout({ children }) {
  return (
    <html lang="es">
      <body>{children}</body>
    </html>
  );
}

