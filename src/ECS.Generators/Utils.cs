using System;
using System.Security.Cryptography;
using System.Text;

namespace Friflo.Engine.ECS.Generators;

public static class Utils
{
    public static string GetMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}