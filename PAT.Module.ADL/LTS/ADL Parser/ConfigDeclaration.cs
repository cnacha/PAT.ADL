using System;
using System.Text;

namespace ADLParser.Classes
{
    public class ConfigDeclaration
    {
        public String LeftName;
        public String RightName;
        public SysEvent LeftFunction;
        public SysEvent RightFunction;

        public ConfigDeclaration(String leftName, String rightName)
        {
            LeftName = leftName;
            RightName = rightName;
        }

        public ConfigDeclaration(String leftName, SysEvent leftFunction, String rightName, SysEvent rightFunction)
        {
            LeftName = leftName;
            LeftFunction = leftFunction;
            RightName = rightName;
            RightFunction = rightFunction;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(LeftName + ((LeftFunction ==null)?"":"." + LeftFunction.ToString()) + "=" + RightName + ((RightFunction==null)?"":"." + RightFunction.ToString()));
            return sb.ToString();
        }
    }
}