using Mundialito.Application.Common.Results;

namespace Mundialito.Application.Features.Matches;

public static class MatchErrors
{
    public static Error TeamsAreRequired => new(
        "Matches.TeamsAreRequired",
        "Los equipos local y visitante son obligatorios.",
        ErrorType.Validation);

    public static Error NotFound(Guid id) => new(
        "Matches.NotFound",
        $"No se encontró el partido con id '{id}'.",
        ErrorType.NotFound);

    public static Error TeamsMustBeDifferent => new(
        "Matches.TeamsMustBeDifferent",
        "El equipo local y el equipo visitante deben ser diferentes.",
        ErrorType.Validation);

    public static Error TeamNotFound(Guid teamId) => new(
        "Matches.TeamNotFound",
        $"No se encontró el equipo con id '{teamId}'.",
        ErrorType.NotFound);

    public static Error InvalidScheduledDate => new(
        "Matches.InvalidScheduledDate",
        "La fecha programada del partido es obligatoria.",
        ErrorType.Validation);

    public static Error OnlyScheduledCanBeUpdated(Guid id) => new(
        "Matches.OnlyScheduledCanBeUpdated",
        $"Solo se puede modificar el partido programado con id '{id}'.",
        ErrorType.Conflict);

    public static Error AlreadyFinished(Guid id) => new(
        "Matches.AlreadyFinished",
        $"El partido con id '{id}' ya finalizó.",
        ErrorType.Conflict);

    public static Error TeamDoesNotParticipate(Guid matchId, Guid teamId) => new(
        "Matches.TeamDoesNotParticipate",
        $"El equipo con id '{teamId}' no participa en el partido con id '{matchId}'.",
        ErrorType.Validation);

    public static Error PlayerNotFound(Guid playerId) => new(
        "Matches.PlayerNotFound",
        $"No se encontró el jugador con id '{playerId}'.",
        ErrorType.NotFound);

    public static Error PlayerDoesNotBelongToTeam(Guid playerId, Guid teamId) => new(
        "Matches.PlayerDoesNotBelongToTeam",
        $"El jugador con id '{playerId}' no pertenece al equipo con id '{teamId}'.",
        ErrorType.Validation);

    public static Error InvalidGoalMinute => new(
        "Matches.InvalidGoalMinute",
        "El minuto del gol debe estar entre 1 y 120.",
        ErrorType.Validation);
}
