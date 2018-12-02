using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Config
{

    [XmlType("item")]
    public class ConfigStagUser
    {
        public String UserName;
        public String OsId;
        public String HashedPassword = "";

        //Empty&public for serialization
        public ConfigStagUser(){}

        public ConfigStagUser(string userName, SecureString password, string osId)
        {
            this.UserName = userName;
            this.OsId = osId;
            this.HashedPassword = EncryptString(password);
        }

        public SecureString GetPassword(String hashedPassword)
        {
            return DecryptString(hashedPassword);
        }

        #region ProtectedData
        //This region is not my code at all, it was copied from stackOverflow comment, directing to 
        // https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file .
        // So the code is from there. I didn´t have my cryptology classes yet.

        /// <summary>
        /// Probly shoud generate this, but I don´t know hot to generate salt and store it. 
        /// Hashed maybe ? ... rly, idk.
        /// </summary>
        static readonly byte[] Entropy = Encoding.Unicode.GetBytes("Salt will never be a password.");

        private static string EncryptString(SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                Entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        private static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    Entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        private static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        private static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
        #endregion
    }
}
