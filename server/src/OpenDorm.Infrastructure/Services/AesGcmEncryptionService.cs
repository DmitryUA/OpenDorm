using System.Security.Cryptography;
using OpenDorm.Infrastructure.Abstractions;

namespace OpenDorm.Infrastructure.Services;

public class AesGcmEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    
    public AesGcmEncryptionService(byte[] key)
    {
        if (key == null) 
            throw new ArgumentNullException(nameof(key));
        if (key.Length != 32) 
            throw new ArgumentException("Key must be exactly 32 bytes (256 bits) for AES-256.", nameof(key));
        
        _key = key;
    }
    
    public byte[] Encrypt(byte[] plainData)
    {
        if (plainData == null || plainData.Length == 0)
            return Array.Empty<byte>();

        // Nonce должен быть ровно 12 байт для GCM
        byte[] nonce = RandomNumberGenerator.GetBytes(12);
        byte[] ciphertext = new byte[plainData.Length];
        byte[] tag = new byte[16]; // Tag всегда 16 байт для GCM

        using (AesGcm aes = new AesGcm(_key))
        {
            aes.Encrypt(nonce, plainData, ciphertext, tag);
        }

        // Формируем итоговый массив: [Nonce (12)] + [Ciphertext (N)] + [Tag (16)]
        byte[] result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);

        return result;
    }
    
    public byte[] Decrypt(byte[] cipherData)
    {
        if (cipherData == null || cipherData.Length == 0)
            return Array.Empty<byte>();

        // Минимальная длина: 12 (Nonce) + 16 (Tag) = 28 байт
        if (cipherData.Length < 28)
            throw new ArgumentException("Cipher data is too short to be valid.", nameof(cipherData));

        byte[] nonce = new byte[12];
        byte[] tag = new byte[16];
        int ciphertextLength = cipherData.Length - 12 - 16;
        byte[] ciphertext = new byte[ciphertextLength];

        // Разбираем массив на части
        Buffer.BlockCopy(cipherData, 0, nonce, 0, 12);
        Buffer.BlockCopy(cipherData, 12, ciphertext, 0, ciphertextLength);
        Buffer.BlockCopy(cipherData, 12 + ciphertextLength, tag, 0, 16);

        byte[] plaintext = new byte[ciphertextLength];

        using (AesGcm aes = new AesGcm(_key))
        {
            // Если данные были повреждены или ключ неверный, здесь выбросится CryptographicException
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }

        return plaintext;
    }
}