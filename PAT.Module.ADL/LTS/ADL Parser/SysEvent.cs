using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ADLParser.Classes
{
    [Serializable]
    public class SysEvent
    {
        public String Name;
        public String ConfigName;
        public List<String> Parameters;


        public SysEvent(String s) 
        {
            this.Name = s;
            Parameters = new List<String>();
        }

        public string getName()
        {
            return this.Name + "_" + this.ConfigName;
        }

        public override String ToString()
        {
            
            StringBuilder result = new StringBuilder();
            result.Append(Name+":");
            if (this is SysChannel)
                result.Append((((SysChannel)this).ChannelType == SysChannel.Type.Input )?"chin":"chou");
            else if (this is SysProcess) { 
                result.Append("proc");
            }
            else
                result.Append("evnt");

            if(this is SysChannel || this is SysProcess)
            {
                if(Parameters.Count > 0)
                {
                    result.Append("(");
                    int count = 0;
                    foreach(var param in Parameters)
                    {
                        result.Append(param + ((count<Parameters.Count-1)?",":""));
                        count++;
                    }
                    result.Append(")");
                }
            }
            if(this is SysProcess && ((SysProcess)this).next!=null)
            {
                result.Append(" ||| "+((SysProcess)this).next.ToString());
            }

            return result.ToString();
        }
    }
}
