using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ADLParser.Classes
{
    public class Component
    {
        public string Name;
        public List<Feature> portList;

        public Component(string name)
        {
            this.Name = name;
            portList = new List<Feature>();
        }

        public Feature getPortByName(string portName)
        {
            foreach (var port in portList)
            {
                if (port.Name == portName)
                    return port;
            }
            return null;
        }

        public override String ToString()
        {
           
            StringBuilder result = new StringBuilder();
            result.Append("component: {");
            result.Append("Name:"+Name+",");
            result.AppendLine("Port: ");
            foreach (var port in portList) 
                result.AppendLine(port.ToString());
            result.Append("}");
            return result.ToString();
        }
    }
}
