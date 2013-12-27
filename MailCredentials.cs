using Limilabs.Client.IMAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace MediaLib
{
    public class MailCredentials
    {
        public MailCredentials(string server, string email, string password)
        {
            Server = server;
            Email = email.ToSecureString();
            Password = password.ToSecureString();
            //encode(email, password);
        }

        public string Server { get; set; }
        public SecureString Email { get; set; }
        public SecureString Password { get; set; }

        public Imap Imap()
        {
            Imap imap = new Imap();
            
            try
            {
                imap.Connect(Server, 993, true);
                imap.Login(
                    Email.SecureStringToString(),
                    Password.SecureStringToString());
            }
            catch
            {
                imap = null;
            }
            

            return imap;
        }

        private void encode(string email, string password)
        {
            // Data to protect. Convert a string to a byte[] using Encoding.UTF8.GetBytes().
            byte[] emailtext = Encoding.UTF8.GetBytes(email);

            // Generate additional entropy (will be used as the Initialization vector)
            byte[] entropy = new byte[20];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            byte[] ciphertext = ProtectedData.Protect(emailtext, entropy,
                DataProtectionScope.CurrentUser);

            byte[] plaintext = ProtectedData.Unprotect(ciphertext, entropy,
                DataProtectionScope.CurrentUser);

            string s = Encoding.UTF8.GetString(plaintext);

            SecureString ss = Encoding.UTF8.GetString(plaintext).ToSecureString();
            s = ss.SecureStringToString();
        }
    }

    public static class StringUtil
    {
        /// <summary>
        /// Returns a Secure string from the source string
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this string Source)
        {
            if (string.IsNullOrWhiteSpace(Source))
                return null;
            else
            {
                SecureString Result = new SecureString();
                foreach (char c in Source.ToCharArray())
                    Result.AppendChar(c);
                return Result;
            }            
        }

        public static String SecureStringToString(this SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }
}
