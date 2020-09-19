using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using ASP_MVC.EF;

namespace ASP_MVC.Models
{
    public class LoginModel
    {
        private QLMayLanhEntities db = null;
        public LoginModel()
        {
            db = new QLMayLanhEntities();
        }
        public bool Login(string UserName, string PassWord)
        {
            object[] sqlparam =
            {
                new SqlParameter ("@username", UserName),
                new SqlParameter ("@password", PassWord)
            };
            var rs = db.Database.SqlQuery<bool>("Acc_Login @username,@password", sqlparam).SingleOrDefault();
            return rs;
        }
        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}