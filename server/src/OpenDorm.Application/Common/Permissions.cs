namespace OpenDorm.Application.Common;

public static class Permissions
{
    public static class Users
    {
        public const string Create = "users.create";
        public const string Read = "users.read";
        public const string Update = "users.update";
        public const string Deactivate = "users.deactivate";
    }

    public static class Dormitories
    {
        public const string Create = "dormitories.create";
        public const string Read = "dormitories.read";
        public const string Update = "dormitories.update";
    }

    public static class Rooms
    {
        public const string Read = "rooms.read";
        public const string Create = "rooms.create";
        public const string Update = "rooms.update";
        public const string Activate = "rooms.activate";
        public const string Deactivate = "rooms.deactivate";
    }

    public static class Occupants
    {
        public const string Create = "occupants.create";
        public const string Read = "occupants.read";
        public const string Update = "occupants.update";
        public const string Activate = "occupants.activate";
        public const string Deactivate = "occupants.deactivate";
    }

    public static class Accommodations
    {
        public const string Read = "accommodations.read";
        public const string CheckIn = "accommodations.checkin";
        public const string CheckOut = "accommodations.checkout";
        public const string Transfer = "accommodations.transfer";
    }
}