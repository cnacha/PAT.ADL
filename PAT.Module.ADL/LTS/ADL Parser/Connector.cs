using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ADLParser.Classes
{
    public class Connector
    {
        public string Name;
        public String ConfigName;
        public List<Feature> roleList;
        //public Feature Glue;

        public Connector(string name)
        {
            this.Name = name;
            roleList = new List<Feature>();
        }

        public void setConfigName(string configName)
        {
            this.ConfigName = configName;
            //Glue.setConfigName(configName);
            foreach (var i in roleList)
                i.setConfigName(configName);
        }

        public Feature getRoleByName(string roleName)
        {
            foreach( var role in roleList)
            {
                if (role.Name == roleName)
                    return role;
            }
            return null;
        }


        public string getName()
        {
            return this.Name + "_" + this.ConfigName;
        }

        public override String ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("connector: {");
            result.Append("Name:" + Name + ",");
            result.Append("Role: ");
            foreach (var e in roleList)
                result.Append(e.ToString());
            result.Append("}");
            return result.ToString();
        }
    }
}
