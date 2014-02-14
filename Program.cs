using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UnlockCruisePhotos
{
    public class Program
    {
        public const string ImageDir = @"C:\Users\jdaigle\AppData\Roaming\com.image.getthepicture\Local Store\albums\e2cfe97b926709052765544df78c06fb437dd61df9fd36eeb729e53625d8bb3a\images";
        public const string DecryptKey = "cPGyDWIpiEc=";
        public const string OutputDir = @"C:\temp\cruise-photos\";

        public static void Main(string[] args)
        {
            var encodedImageFilePaths = Directory.GetFiles(ImageDir, "*.enc", SearchOption.TopDirectoryOnly);
            Trace.WriteLine("Found " + encodedImageFilePaths.Length + " encoded image files");
            foreach (var encodedImageFilePath in encodedImageFilePaths)
            {
                var encodedImageFileBuffer = File.ReadAllBytes(encodedImageFilePath);
                if (encodedImageFileBuffer.Length < 500 * 1024)
                {
                    continue;
                }
                var imageFileBuffer = Decrypt(encodedImageFileBuffer);
                Trace.WriteLine("Decrypted: " + Path.GetFileName(encodedImageFilePath));
                var newFileName = Path.GetFileNameWithoutExtension(encodedImageFilePath) + ".jpg";
                File.WriteAllBytes(Path.Combine(OutputDir, newFileName), imageFileBuffer);
            }
        }

        public static byte[] DecodeEncryptionKey()
        {
            return Convert.FromBase64String(DecryptKey);
        }

        public static byte[] Decrypt(byte[] cipherBuffer)
        {
            var desProvider = new DESCryptoServiceProvider();
            desProvider.Mode = CipherMode.ECB;
            desProvider.Padding = PaddingMode.None;
            desProvider.Key = DecodeEncryptionKey();
            using (var cipherStream = new MemoryStream(cipherBuffer))
            using (var output = new MemoryStream())
            {
                using (var cryptStream = new CryptoStream(cipherStream, desProvider.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    var buffer = new byte[1024];
                    var read = cryptStream.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        output.Write(buffer, 0, read);
                        read = cryptStream.Read(buffer, 0, buffer.Length);
                    }
                    cryptStream.Flush();
                }
                return output.ToArray();
            }
        }
    }
}
