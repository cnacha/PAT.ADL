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
        public Dictionary<string, int> countRole = new Dictionary<string, int>();

        public int getCountEventAfter()
        {
            countRole = new Dictionary<string, int>();
            int count = 1;
            Console.Write(this.Name);
            SysProcess nextEve = this.next;
            
            countRole.Add(this.Name, 1);
            while (nextEve != null)
            {
                Console.Write("###"+nextEve.Name);
                if (countRole.ContainsKey(nextEve.Name))
                {
                    countRole.TryGetValue(nextEve.Name, out int currentCount);
                    countRole[nextEve.Name] =  currentCount + 1;
                } else
                    countRole.Add(nextEve.Name, 1);

            
                count++;
                nextEve = nextEve.next;
            }
            return count;
        }

        public override string ToString()
        {
            return Super + "." + Name;
        }
    }
}
