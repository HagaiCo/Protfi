using System.Security.Cryptography;

namespace Protfi.Common
{
    class AesEncryption
    {
        public static void Main()
        {
            var original = "Here is some data to encrypt!";

            // Create a new instance of the Aes
            // class.  This generates a new key and initialization
            // vector (IV).
            using var myAes = Aes.Create();
            // Encrypt the string to an array of bytes.
            var encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

            File.WriteAllBytes("/Users/hagaicohen/Downloads/encrypted.txt", encrypted);
            File.WriteAllBytes("/Users/hagaicohen/Downloads/key.txt", myAes.Key);
            File.WriteAllBytes("/Users/hagaicohen/Downloads/iv.txt", myAes.IV);
            
            encrypted = File.ReadAllBytes("/Users/hagaicohen/Downloads/encrypted.txt");
            var key = File.ReadAllBytes("/Users/hagaicohen/Downloads/key.txt");
            var iv = File.ReadAllBytes("/Users/hagaicohen/Downloads/iv.txt");
            
            // Decrypt the bytes to a string.
            var roundtrip = DecryptStringFromBytes_Aes(encrypted, key, iv);

            //Display the original data and the decrypted data.
            Console.WriteLine("Original:   {0}", original);
            Console.WriteLine("Round Trip: {0}", roundtrip);
        }

        static byte[] EncryptStringToBytes_Aes(string dataToEncrypt, Guid userIdentifier)
        {
            using var myAes = Aes.Create();
            var keyContainer = new KeyContainer(myAes.Key, myAes.IV, userIdentifier);

            var encryptedData = EncryptStringToBytes_Aes(dataToEncrypt, myAes.Key, myAes.IV);
            return encryptedData;
        }
        
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            // Create an Aes object
            // with the specified key and IV.
            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                //Write all data to the stream.
                swEncrypt.Write(plainText);
            }
            var encrypted = msEncrypt.ToArray();

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            // Declare the string used to hold
            // the decrypted text.

            // Create an Aes object
            // with the specified key and IV.
            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using var msDecrypt = new MemoryStream(cipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            var plaintext = srDecrypt.ReadToEnd();

            return plaintext;
        }
    }
}