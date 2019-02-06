using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ADLParser.Classes
{
    [Serializable]
    public class Feature 
    {
        public String Name;
        public List<String> Params;
        public List<SysEvent> process;
        public string ConfigName;
        public Feature(String name)
        {
            this.Name = name;
            this.Params = new List<String>();
            this.process = new List<SysEvent>();
        }

       public void setConfigName(string configName)
        {
            this.ConfigName = configName;
            foreach (var i in process) { 
                i.ConfigName = configName;
                if(i is SysProcess)
                {
                    ((SysProcess)i).setConfigName(configName);
                }
            }
        }

        public string getName()
        {
            return this.ConfigName + "_" + this.Name ;
        }

        public override String ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("feature: {");
            result.Append("Name:" + Name + ",");
            result.Append("Sequence: ");
            int count = 0;
            foreach (var e in process)
            {
                if(e!=null)
                    result.Append(e.ToString() + ((count < process.Count - 1) ? "->" : ""));
                count++;
            }
            result.Append("}");
            return result.ToString();
        }
    }
}
