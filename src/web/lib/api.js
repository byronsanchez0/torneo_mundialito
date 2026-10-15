const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8081";

export class ApiError extends Error {
  constructor(message, status = 0, code = null) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
  }
}

function buildQuery(parameters) {
  const query = new URLSearchParams();

  Object.entries(parameters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      query.set(key, String(value));
    }
  });

  const value = query.toString();
  return value ? `?${value}` : "";
}

async function request(path, options = {}) {
  let response;

  try {
    response = await fetch(`${apiUrl}${path}`, {
      ...options,
      headers: {
        Accept: "application/json",
        ...options.headers,
      },
    });
  } catch (error) {
    if (error.name === "AbortError") throw error;
    throw new ApiError("No fue posible conectar con la API.");
  }

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? await response.json()
    : null;

  if (!response.ok) {
    throw new ApiError(
      body?.message ?? "La solicitud no pudo completarse.",
      response.status,
      body?.code,
    );
  }

  return body;
}

export function getHealth(signal) {
  return request("/health", { signal });
}

export function getTeams(parameters, signal) {
  return request(`/api/teams${buildQuery(parameters)}`, { signal });
}

export function getPlayers(parameters, signal) {
  return request(`/api/players${buildQuery(parameters)}`, { signal });
}

export function getMatches(parameters, signal) {
  return request(`/api/matches${buildQuery(parameters)}`, { signal });
}

export function getStatistics(signal) {
  return request("/api/statistics", { signal });
}

export function registerGoal(matchId, payload) {
  return request(`/api/matches/${matchId}/goals`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Idempotency-Key": crypto.randomUUID(),
    },
    body: JSON.stringify(payload),
  });
}
