using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.DAO
{
    class StagUser
    {
        public string UserName { get; private set; }
        public SecureString Password { get; private set; }
        public string StagOsId { get; set; }

        public StagUser(string userName, SecureString password, string stagOsId = "")
        {
            this.UserName = userName;
            this.Password = password;
            this.StagOsId = stagOsId;
        }

        public string FormatCredentials()
        {
            return UserName + ":" + Password;
        }

    }
}
