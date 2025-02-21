using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Core
{
    public class PasswordHashing
    {
        public static byte[] CalculateHash(byte[] inputBytes)
        {
            SHA256Managed algorithm = new SHA256Managed();
            algorithm.ComputeHash(inputBytes);
            return algorithm.Hash;
        }

        public static bool SequenceEquals(byte[] originalByteArray, byte[] newByteArray)
        {
            //If either byte array is null, throw an ArgumentNullException
            if (originalByteArray == null || newByteArray == null)

                return false;
            //throw new ArgumentNullException(originalByteArray == null ? "originalByteArray" : "newByteArray","The byte arrays supplied may not be null.");

            //If byte arrays are different lengths, return false
            if (originalByteArray.Length != newByteArray.Length)
                return false;

            //If any elements in corresponding positions are not equal
            //return false
            for (int i = 0; i < originalByteArray.Length; i++)
            {
                if (originalByteArray[i] != newByteArray[i])
                    return false;
            }

            //If we've got this far, the byte arrays are equal.
            return true;
        }

    }
}
