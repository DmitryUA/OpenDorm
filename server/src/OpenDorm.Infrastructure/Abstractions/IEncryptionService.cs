namespace OpenDorm.Infrastructure.Abstractions;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] plainData);
    byte[] Decrypt(byte[] cipherData);
}