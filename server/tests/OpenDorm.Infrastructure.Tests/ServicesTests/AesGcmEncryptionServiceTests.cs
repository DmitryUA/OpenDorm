using System.Security.Cryptography;
using System.Text;
using OpenDorm.Infrastructure.Services;

namespace OpenDorm.Infrastructure.Tests.ServicesTests;

public class AesGcmEncryptionServiceTests
{
    // Генерируем валидный 32-байтный ключ для тестов
    private readonly byte[] _validKey = RandomNumberGenerator.GetBytes(32);

    #region Тесты конструктора

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenKeyIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AesGcmEncryptionService(null));
    }

    [Theory]
    [InlineData(16)]  // AES-128
    [InlineData(24)]  // AES-192
    [InlineData(31)]  // На 1 байт меньше
    [InlineData(33)]  // На 1 байт больше
    public void Constructor_ThrowsArgumentException_WhenKeyLengthIsInvalid(int keyLength)
    {
        // Arrange
        byte[] invalidKey = RandomNumberGenerator.GetBytes(keyLength);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new AesGcmEncryptionService(invalidKey));
    }

    #endregion

    #region Базовая функциональность (Roundtrip)

    [Fact]
    public void EncryptAndDecrypt_Roundtrip_ReturnsOriginalData()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);
        string originalText = "Привет, мир! Это тестовая строка для шифрования.";
        byte[] originalData = Encoding.UTF8.GetBytes(originalText);

        // Act
        byte[] encrypted = service.Encrypt(originalData);
        byte[] decrypted = service.Decrypt(encrypted);

        // Assert
        Assert.Equal(originalData, decrypted);
        Assert.Equal(originalText, Encoding.UTF8.GetString(decrypted));
    }

    [Fact]
    public void EncryptAndDecrypt_WithBinaryData_ReturnsOriginalData()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);
        // Генерируем случайные бинарные данные (например, картинка или файл)
        byte[] originalData = RandomNumberGenerator.GetBytes(1024); 

        // Act
        byte[] encrypted = service.Encrypt(originalData);
        byte[] decrypted = service.Decrypt(encrypted);

        // Assert
        Assert.Equal(originalData, decrypted);
    }

    #endregion

    #region Граничные случаи (Edge Cases)

    [Fact]
    public void Encrypt_ReturnsEmptyArray_WhenInputIsNull()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);

        // Act
        byte[] result = service.Encrypt(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Encrypt_ReturnsEmptyArray_WhenInputIsEmpty()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);

        // Act
        byte[] result = service.Encrypt(Array.Empty<byte>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Decrypt_ReturnsEmptyArray_WhenInputIsNull()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);

        // Act
        byte[] result = service.Decrypt(null);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region Безопасность и целостность данных

    [Fact]
    public void Encrypt_ProducesDifferentCiphertext_WhenCalledMultipleTimes()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);
        byte[] data = Encoding.UTF8.GetBytes("Тест уникальности");

        // Act
        byte[] encrypted1 = service.Encrypt(data);
        byte[] encrypted2 = service.Encrypt(data);

        // Assert
        // Из-за случайного Nonce каждый зашифрованный массив должен быть уникальным
        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Decrypt_ThrowsCryptographicException_WhenDataIsTampered()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);
        byte[] originalData = Encoding.UTF8.GetBytes("Важные данные");
        byte[] encrypted = service.Encrypt(originalData);

        // Подменяем один байт в середине зашифрованного текста (Ciphertext)
        encrypted[15] ^= 0xFF; 

        // Act & Assert
        // GCM должен обнаружить подмену и выбросить исключение
        Assert.Throws<AuthenticationTagMismatchException>(() => service.Decrypt(encrypted));
    }

    [Fact]
    public void Decrypt_ThrowsCryptographicException_WhenKeyIsWrong()
    {
        // Arrange
        var service1 = new AesGcmEncryptionService(_validKey);
        byte[] wrongKey = RandomNumberGenerator.GetBytes(32);
        var service2 = new AesGcmEncryptionService(wrongKey);
        
        byte[] originalData = Encoding.UTF8.GetBytes("Секретные данные");
        byte[] encrypted = service1.Encrypt(originalData);

        // Act & Assert
        // Дешифровка чужим ключом должна падать
        Assert.Throws<AuthenticationTagMismatchException>(() => service2.Decrypt(encrypted));
    }

    [Fact]
    public void Decrypt_ThrowsArgumentException_WhenDataIsTooShort()
    {
        // Arrange
        var service = new AesGcmEncryptionService(_validKey);
        // Минимальная длина: 12 (Nonce) + 16 (Tag) = 28 байт. Передаем меньше.
        byte[] tooShortData = RandomNumberGenerator.GetBytes(20); 

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.Decrypt(tooShortData));
    }

    #endregion
}