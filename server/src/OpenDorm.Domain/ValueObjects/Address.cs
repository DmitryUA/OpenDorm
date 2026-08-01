using System.Diagnostics.CodeAnalysis;

namespace OpenDorm.Domain.ValueObjects;

public sealed record Address
{
    public City City { get; }
    public Street Street { get; }
    public HouseNumber House { get; }
    
    public Address(City city, Street street, HouseNumber house)
    {
        ArgumentNullException.ThrowIfNull(street);
        ArgumentNullException.ThrowIfNull(house);
        ArgumentNullException.ThrowIfNull(city);

        City = city;
        House = house;
        Street = street;
    }
    
    public override string ToString() => $"{City}, {Street}, {House}";
}