using System.Security.Cryptography;
using OpenDorm.Domain.Abstractions;

namespace OpenDorm.Infrastructure.Services;

public class AesGcmEncryptionService : IEncryptionService
{
    private const int TagSize = 16;
    private readonly byte[] _key;
    
    public AesGcmEncryptionService(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        
        if (key.Length != 32) 
            throw new ArgumentException("Key must be exactly 32 bytes (256 bits) for AES-256.", nameof(key));
        
        _key = key;
    }
    
    public byte[] Encrypt(byte[]? plainData)
    {
        if (plainData == null || plainData.Length == 0)
            return [];

        // Nonce должен быть ровно 12 байт для GCM
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plainData.Length];
        var tag = new byte[TagSize]; // Tag всегда 16 байт для GCM

        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Encrypt(nonce, plainData, ciphertext, tag);
        }

        // Формируем итоговый массив: [Nonce (12)] + [Ciphertext (N)] + [Tag (16)]
        var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);

        return result;
    }
    
    public byte[] Decrypt(byte[]? cipherData)
    {
        if (cipherData == null || cipherData.Length == 0)
            return [];

        // Минимальная длина: 12 (Nonce) + 16 (Tag) = 28 байт
        if (cipherData.Length < 28)
            throw new ArgumentException("Cipher data is too short to be valid.", nameof(cipherData));

        var nonce = new byte[12];
        var tag = new byte[16];
        var ciphertextLength = cipherData.Length - 12 - 16;
        var ciphertext = new byte[ciphertextLength];

        // Разбираем массив на части
        Buffer.BlockCopy(cipherData, 0, nonce, 0, 12);
        Buffer.BlockCopy(cipherData, 12, ciphertext, 0, ciphertextLength);
        Buffer.BlockCopy(cipherData, 12 + ciphertextLength, tag, 0, 16);

        var plaintext = new byte[ciphertextLength];

        using (AesGcm aes = new AesGcm(_key, TagSize))
        {
            // Если данные были повреждены или ключ неверный, здесь выбросится CryptographicException
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }

        return plaintext;
    }
}