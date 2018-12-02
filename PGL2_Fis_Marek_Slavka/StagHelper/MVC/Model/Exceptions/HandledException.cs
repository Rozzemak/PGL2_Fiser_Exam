using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.MVC.Model.Exceptions
{
    [Serializable]
    public class HandledException : Exception
    {
        public HandledException()
        {

        }

        public HandledException(string message) : base(message)
        {

        }

        public HandledException(string message, Exception inner) : base(message, inner)
        {

        }

        protected HandledException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {

        }
    } 
}
