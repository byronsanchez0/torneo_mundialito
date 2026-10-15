const statusLabels = {
  Scheduled: "Programado",
  InProgress: "En juego",
  Finished: "Finalizado",
};

export function formatDate(value) {
  return new Intl.DateTimeFormat("es-GT", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export function formatStatus(value) {
  return statusLabels[value] ?? value;
}

export function formatGoalDifference(value) {
  return value > 0 ? `+${value}` : String(value);
}
