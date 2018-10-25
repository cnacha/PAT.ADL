using System;
using System.Collections.Generic;
using System.Text;

namespace ADLParser.Classes
{
    [Serializable]
    public class SysProcess: SysEvent
    {
        public enum Operation
        {
            Interleave,
            Choice,
            Embed,
            Parallel
        }
        public string Super;
        public SysProcess next;
        public Operation operation;

        public SysProcess(String super, String name) : base(name)
        {
            this.Parameters = new List<String>();
            this.operation = Operation.Interleave;
            this.next = null;
            this.Super = super;
        }

        public SysProcess(String name) : base(name)
        {
            Parameters = new List<String>();
            operation = Operation.Interleave;
            next = null;
        }

        public void setConfigName(string configName)
        {
            this.ConfigName = configName;
            SysProcess current = this.next;
            while (current != null)
            {
                current.ConfigName = configName;
                current = current.next;
            }

        }

        public override string ToString()
        {
            return Super + "." + Name;
        }
    }
}
