using OpenDorm.Domain.Aggregates.System;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Domain.Tests.EntityTests;

public class SystemFlagTests
{
    [Theory]
    [InlineData("feature:dark-mode", "enabled")]
    [InlineData("maintenance-mode", "false")]
    [InlineData("a", "b")]
    public void Constructor_ValidArguments_CreatesInstanceAndPreservesValues(string name, string value)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var flag = new SystemFlag(id, name, value);

        // Assert
        Assert.Equal(id, flag.Id);
        Assert.Equal(name, flag.Name);
        Assert.Equal(value, flag.Value);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsCreatedAtCloseToUtcNow()
    {
        // Arrange
        var id = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var flag = new SystemFlag(id, "some-flag", "some-value");

        // Assert
        var after = DateTime.UtcNow;
        Assert.InRange(flag.CreatedAt, before, after);
    }

    [Fact]
    public void Constructor_PreservesValueWithoutTrimming()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string nameWithSpaces = "  flag-name  ";
        const string valueWithSpaces = "  value  ";

        // Act
        var flag = new SystemFlag(id, nameWithSpaces, valueWithSpaces);

        // Assert
        Assert.Equal(nameWithSpaces, flag.Name);
        Assert.Equal(valueWithSpaces, flag.Value);
    }

    [Fact]
    public void Constructor_NameExactlyMaxLength_CreatesInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var maxLengthName = new string('a', SystemFlag.MaxNameLength);

        // Act
        var flag = new SystemFlag(id, maxLengthName, "value");

        // Assert
        Assert.Equal(maxLengthName, flag.Name);
    }

    [Fact]
    public void Constructor_ValueExactlyMaxLength_CreatesInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var maxLengthValue = new string('b', SystemFlag.MaxValueLength);

        // Act
        var flag = new SystemFlag(id, "name", maxLengthValue);

        // Assert
        Assert.Equal(maxLengthValue, flag.Value);
    }

    [Fact]
    public void Constructor_NullName_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        const string expectedMessage = "Flag name cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), null!, "value"));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        const string expectedMessage = "Flag name cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), string.Empty, "value"));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        const string expectedMessage = "Flag name cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), "   ", "value"));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_NameExceedsMaxLength_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        var overlyLongName = new string('a', SystemFlag.MaxNameLength + 1);
        var expectedMessage = $"Flag name cannot exceed {SystemFlag.MaxNameLength} characters.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), overlyLongName, "value"));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_NullValue_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        const string expectedMessage = "Flag value cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), "name", null!));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_EmptyValue_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        const string expectedMessage = "Flag value cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), "name", string.Empty));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WhitespaceValue_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        const string expectedMessage = "Flag value cannot be empty.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), "name", "   "));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_ValueExceedsMaxLength_ThrowsInvalidSystemFlagException()
    {
        // Arrange
        var overlyLongValue = new string('b', SystemFlag.MaxValueLength + 1);
        var expectedMessage = $"Flag value cannot exceed {SystemFlag.MaxValueLength} characters.";

        // Act & Assert
        var exception = Assert.Throws<InvalidSystemFlagException>(
            () => new SystemFlag(Guid.NewGuid(), "name", overlyLongValue));
        Assert.Equal(expectedMessage, exception.Message);
    }
}