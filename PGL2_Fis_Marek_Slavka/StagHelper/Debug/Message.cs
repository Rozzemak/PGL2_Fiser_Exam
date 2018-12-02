using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGL2_Fis_Marek_Slavka.StagHelper.Debug
{
    public enum MessageTypeEnum
    {
        Standard,
        Warning,
        Error,
        Exception,
        Indifferent,
        DefaultWriteAll,
        Event,
        Rest,
        HttpClient,
        ________
    }

    class Message<T>
    {
        public T MessageContent;
        public MessageTypeEnum MessageType;
        public bool shown;

        public Message(T messageContent, MessageTypeEnum messageType = MessageTypeEnum.Standard)
        {
            this.MessageContent = messageContent;
            this.MessageType = messageType;
        }

    }
}