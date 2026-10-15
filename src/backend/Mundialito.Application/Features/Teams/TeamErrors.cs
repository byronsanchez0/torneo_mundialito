using Mundialito.Application.Common.Results;

namespace Mundialito.Application.Features.Teams;

public static class TeamErrors
{
    public static Error NameIsRequired => new(
        "Teams.NameIsRequired",
        "El nombre del equipo es obligatorio.",
        ErrorType.Validation);

    public static Error NotFound(Guid id) => new(
        "Teams.NotFound",
        $"No se encontró el equipo con id '{id}'.",
        ErrorType.NotFound);

    public static Error NameAlreadyExist(string name) => new(
        "Teams.NameAlreadyExist",
        $"Ya existe un equipo con el nombre '{name}'.",
        ErrorType.Conflict);

    public static Error HasRelatedRecords(Guid id) => new(
        "Teams.HasRelatedRecords",
        $"El equipo con id '{id}' tiene jugadores o partidos relacionados.",
        ErrorType.Conflict);
}
