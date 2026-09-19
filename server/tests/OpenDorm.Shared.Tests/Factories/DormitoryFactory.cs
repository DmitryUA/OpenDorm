using OpenDorm.Domain.Aggregates.Dormitory;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Shared.Tests.Factories;

public static class DormitoryFactory
{
    /// <summary>
    /// Создаёт общежитие без комнат.
    /// </summary>
    public static Dormitory Create(Address? address = null, int floorCount = 1)
    {
        var addr = address ?? new Address(
            new City("Тестовый город"),
            new Street("Тестовая улица"),
            new HouseNumber("Тестовый номер дома"));
        
        return new Dormitory(Guid.NewGuid(), addr, floorCount);
    }

    /// <summary>
    /// Создаёт общежитие с указанным количеством комнат.
    /// Возвращает кортеж (общежитие, список Id созданных комнат).
    /// </summary>
    public static (Dormitory Dormitory, IReadOnlyList<Guid> RoomIds) CreateWithRooms(
        Address? address = null,
        int floorCount = 2,
        int roomsCount = 2,
        int capacity = 2,
        Gender gender = Gender.Male)
    {
        var dormitory = Create(address, floorCount);
        var roomIds = new List<Guid>(roomsCount);

        for (var i = 1; i <= roomsCount; i++)
        {
            var floorNumber = (i - 1) % floorCount + 1;
            
            var roomName = new RoomName($"{floorNumber}0{i:D2}");
            var roomId = dormitory.AddRoom(roomName, gender, capacity, floorNumber);
            roomIds.Add(roomId);
        }

        return (dormitory, roomIds);
    }
    
    /// <summary>
    /// Создаёт общежитие с одной комнатой.
    /// </summary>
    /// <returns>Вернёт кортеж (общежитие, id комнаты)</returns>
    public static (Dormitory Dormitory, Guid RoomId) CreateWithOneRoom()
    {
        var (dormitory, roomsIds) = CreateWithRooms(roomsCount: 1);
        return (dormitory, roomsIds[0]);
    }
}