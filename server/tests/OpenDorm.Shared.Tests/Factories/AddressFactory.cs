using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Shared.Tests.Factories;

public static class AddressFactory
{
    private static readonly string[] Cities =
    [
        "Москва",
        "Санкт-Петербург",
        "Казань",
        "Новосибирск",
        "Екатеринбург",
        "Нижний Новгород",
        "Самара",
        "Омск",
        "Челябинск",
        "Ростов-на-Дону"
    ];

    private static readonly string[] Streets =
    [
        "Ленина",
        "Советская",
        "Пушкина",
        "Гагарина",
        "Мира",
        "Кирова",
        "Баумана",
        "Тверская",
        "Арбат",
        "Невский проспект"
    ];

    private static readonly Random RandomGenerator = new();

    public static Address Create(
        string city = "Москва",
        string street = "Ленина",
        string house = "10")
    {
        return new Address(
            new City(city),
            new Street(street),
            new HouseNumber(house));
    }

    public static Address Random()
    {
        var city = Cities[RandomGenerator.Next(Cities.Length)];
        var street = Streets[RandomGenerator.Next(Streets.Length)];
        var house = (RandomGenerator.Next(1, 101)).ToString();
        
        return Create(city, street, house);
    }

    public static Address Moscow() => Create("Москва", "Тверская", "1");
    public static Address Kazan() => Create("Казань", "Баумана", "5");
}