﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ADLParser.Classes
{
    class Component
    {
        public string Name;
        public List<Feature> portList;

        public Component(string name)
        {
            this.Name = name;
            portList = new List<Feature>();
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