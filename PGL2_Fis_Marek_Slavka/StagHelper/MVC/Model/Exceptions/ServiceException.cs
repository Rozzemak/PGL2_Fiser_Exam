using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions
{
    [Serializable]
    public class ServiceException : HandledException
    {
        public ServiceException()
        {

        }

        public ServiceException(string message) : base(message)
        {

        }

        public ServiceException(string message, Exception inner) : base(message, inner)
        {

        }

        protected ServiceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {

        }
    }
}
