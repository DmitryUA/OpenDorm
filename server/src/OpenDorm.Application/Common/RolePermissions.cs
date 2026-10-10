using OpenDorm.Domain.Enums;

namespace OpenDorm.Application.Common;

public static class RolePermissions
{
    private static readonly Dictionary<RoleName, IReadOnlyCollection<string>> Map = new()
    {
        [RoleName.Administrator] =
        [
            // Users
            Permissions.Users.Create,
            Permissions.Users.Read,
            Permissions.Users.Update,
            Permissions.Users.Deactivate,

            // Dormitories
            Permissions.Dormitories.Create,
            Permissions.Dormitories.Read,
            Permissions.Dormitories.Update,

            // Rooms
            Permissions.Rooms.Read,
            Permissions.Rooms.Create,
            Permissions.Rooms.Update,
            Permissions.Rooms.Activate,
            Permissions.Rooms.Deactivate,

            // Occupants
            Permissions.Occupants.Create,
            Permissions.Occupants.Read,
            Permissions.Occupants.Update,
            Permissions.Occupants.Activate,
            Permissions.Occupants.Deactivate,

            // Accommodations
            Permissions.Accommodations.CheckIn,
            Permissions.Accommodations.CheckOut,
            Permissions.Accommodations.Transfer,
            Permissions.Accommodations.Read
        ],

        [RoleName.Manager] =
        [
            // Dormitories
            Permissions.Dormitories.Read,

            // Rooms
            Permissions.Rooms.Read,
            Permissions.Rooms.Create,
            Permissions.Rooms.Update,
            Permissions.Rooms.Activate,
            Permissions.Rooms.Deactivate,

            // Occupants
            Permissions.Occupants.Create,
            Permissions.Occupants.Read,
            Permissions.Occupants.Update,
            Permissions.Occupants.Activate,
            Permissions.Occupants.Deactivate,

            // Accommodations
            Permissions.Accommodations.CheckIn,
            Permissions.Accommodations.CheckOut,
            Permissions.Accommodations.Transfer,
            Permissions.Accommodations.Read
        ]
    };

    public static IReadOnlyCollection<string> GetPermissions(RoleName roleName)
    {
        return Map.TryGetValue(roleName, out var permissions)
            ? permissions
            : [];
    }

    public static IEnumerable<string> GetAllPermissions()
    {
        return Map.Values.SelectMany(p => p).Distinct();
    }
}