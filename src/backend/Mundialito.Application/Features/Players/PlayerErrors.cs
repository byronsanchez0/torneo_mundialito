using Mundialito.Application.Common.Results;

namespace Mundialito.Application.Features.Players;

public static class PlayerErrors
{
    public static Error RequiredFields => new(
        "Players.RequiredFields",
        "El nombre, apellido y posición del jugador son obligatorios.",
        ErrorType.Validation);

    public static Error InvalidShirtNumber => new(
        "Players.InvalidShirtNumber",
        "El dorsal debe estar entre 1 y 99.",
        ErrorType.Validation);

    public static Error NotFound(Guid id) => new(
        "Players.NotFound",
        $"No se encontró el jugador con id '{id}'.",
        ErrorType.NotFound);

    public static Error ShirtNumberAlreadyExist(Guid teamId, int shirtNumber) => new(
        "Players.ShirtNumberAlreadyExist",
        $"El dorsal '{shirtNumber}' ya está asignado en el equipo con id '{teamId}'.",
        ErrorType.Conflict);

    public static Error TeamNotFound(Guid teamId) => new(
        "Players.TeamNotFound",
        $"No se encontró el equipo con id '{teamId}'.",
        ErrorType.NotFound);

    public static Error HasRegisteredGoals(Guid id) => new(
        "Players.HasRegisteredGoals",
        $"El jugador con id '{id}' tiene goles registrados.",
        ErrorType.Conflict);
}
