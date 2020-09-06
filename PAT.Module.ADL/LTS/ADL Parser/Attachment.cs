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

        public bool isSingleInterface()
        {
            // has either a provider or consumer
            if (HeadFunction.getCountEventAfter() == 1)
                return true;
            else
            {
                Console.Write("     ##HeadFunction.countRole" + HeadFunction.countRole.Count);
                // has 1 provider and 1 consumer
                foreach (KeyValuePair<string, int> entry in HeadFunction.countRole)
                {
                    if (entry.Value > 1)
                        return false;
                }
                return true;
            }
        }
    }
}