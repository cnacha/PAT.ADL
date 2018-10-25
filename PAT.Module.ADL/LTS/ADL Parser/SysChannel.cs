using System;
using System.Collections.Generic;
using System.Text;

namespace ADLParser.Classes
{
    [Serializable]
    public class SysChannel: SysEvent
    {
        public enum Type
        {
            Input,
            Output,
        }

        public Type ChannelType ;

        public SysChannel(String name, Type type ): base(name)
        {
            Parameters = new List<String>();
            this.ChannelType = type; 
        }

    }
}
