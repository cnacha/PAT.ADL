using System;
using System.Collections.Generic;
using System.Text;

namespace ADLParser.Classes
{
    class SystemConfig
    {
        public String Name;
        public List<ConfigDeclaration> defineList;
        public List<Attachment> attachList;
        public List<Linkage> linkList;
        public SysProcess Glue;

        public SystemConfig(String name)
        {
            Name = name;
            defineList = new List<ConfigDeclaration>();
            attachList = new List<Attachment>();
            linkList = new List<Linkage>();
        }

        public Linkage FindLinkByInstanceName(String name)
        {
            foreach(var link in linkList)
            {
                if (link.LeftProcess.Super == name)
                    return link;
            }
            return null;
        }

        public override String ToString()
        {

            StringBuilder result = new StringBuilder();
            result.Append("system: {");
            result.AppendLine("Name:" + Name + ",");
            result.AppendLine("define: ");
            foreach (var o in defineList)
                result.AppendLine(o.ToString());
            result.AppendLine("attach: ");
            foreach (var o in attachList)
                result.AppendLine(o.ToString());
            result.AppendLine("link: ");
            foreach (var o in linkList)
                result.AppendLine(o.ToString());
            result.Append("}");
            return result.ToString();
        }
    }
}
