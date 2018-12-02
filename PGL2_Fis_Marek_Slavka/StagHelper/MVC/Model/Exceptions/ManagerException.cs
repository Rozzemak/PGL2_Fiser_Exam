using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions
{
    [Serializable]
    public class ManagerException : HandledException
    {
        public ManagerException()
        {

        }

        public ManagerException(string message) : base(message)
        {

        }

        public ManagerException(string message, Exception inner) : base(message, inner)
        {

        }

        protected ManagerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {

        }
    }
}
