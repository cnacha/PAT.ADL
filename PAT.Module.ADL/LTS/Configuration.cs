using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.ModuleInterface;

namespace PAT.ADL.LTS{
    public class Configuration : ConfigurationBase
    {
        public Process Process;
        private string ConfigID;

        public Configuration(Process p, string e, string displayName, Valuation globalEnv, bool isDataOperation)
        {
            Process = p;
            Event = e;
            GlobalEnv = globalEnv;
            DisplayName = displayName;
            IsDataOperation = isDataOperation;
        }

        public override IEnumerable<ConfigurationBase> MakeOneMove()
        {
            List<Configuration> list = new List<Configuration>();
            Process.MoveOneStep(GlobalEnv, list);

            if (SpecificationBase.HasAtomicEvent)
            {
                List<Configuration> returnList = new List<Configuration>();
                foreach (Configuration configuration in list)
                {
                    if (configuration.IsAtomic)
                    {
                        returnList.Add(configuration);
                    }
                }

                if (returnList.Count > 0)
                {
                    return returnList;
                }    
            }
            
            IsDeadLock = list.Count == 0;
            return list;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("The process is:");
            sb.AppendLine(Process.ToString());            
            if (!GlobalEnv.IsEmpty())
            {
                sb.AppendLine();
                sb.AppendLine("The environment is:");
                sb.AppendLine(GlobalEnv.ToString());
            }
            return sb.ToString();
        }

        public override string GetID()
        {
            if (ConfigID == null)
            {
                if (GlobalEnv.IsEmpty())
                {
                    ConfigID = Process.ProcessID;
                }
                else
                {
                    ConfigID = GlobalEnv.GetID(Process.ProcessID);
                }
            }

            //if we can make sure ConfigID is not changed, we can calculate one time only
            System.Diagnostics.Debug.Assert(ConfigID == Process.ProcessID || ConfigID == GlobalEnv.GetID(Process.ProcessID));

            return ConfigID;
        }
    }
}