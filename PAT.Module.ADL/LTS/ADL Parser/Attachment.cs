using System;
using System.Collections.Generic;
using System.Text;

namespace ADLParser.Classes
{
    public class Attachment
    {
        public String LeftName;
        public SysEvent LeftFunction;
        public SysProcess HeadFunction;



        public Attachment(String leftName, SysEvent leftFunction, SysProcess headFunction)
        {
            LeftName = leftName;
            LeftFunction = leftFunction;
            HeadFunction = headFunction;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(LeftName + ((LeftFunction ==null)?"":"." + LeftFunction.ToString()) + "=" + HeadFunction.ToString());
            return sb.ToString();
        }
    }
}