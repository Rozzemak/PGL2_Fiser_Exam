using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions
{
    [Serializable]
    public class ClientException : HandledException
    {
        public ClientException()
        {

        }

        public ClientException(string message) : base(message)
        {
        }

        public ClientException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ClientException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    } 
}
