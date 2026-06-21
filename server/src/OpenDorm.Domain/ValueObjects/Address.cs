using System.Diagnostics.CodeAnalysis;

namespace OpenDorm.Domain.ValueObjects;

public sealed record Address
{
    public City City { get; }
    public Street Street { get; }
    
    public Address(City city, Street street)
    {
        ArgumentNullException.ThrowIfNull(street);
        ArgumentNullException.ThrowIfNull(city);

        City = city;
        Street = street;
    }
    
    public override string ToString() => $"{City.Name}, {Street.Name}";
}