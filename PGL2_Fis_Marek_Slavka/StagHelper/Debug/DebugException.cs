using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.Debug
{
    [Serializable]
    public class DebugException : Exception
    {
        public DebugException()
        {
        }

        public DebugException(string message) : base(message)
        {
        }

        public DebugException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DebugException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    } 


}
