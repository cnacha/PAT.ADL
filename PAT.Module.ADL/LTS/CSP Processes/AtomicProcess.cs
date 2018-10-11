using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class AtomicProcess : Process
    {
        public Process Process;

        public AtomicProcess(Process process)
        {
            if (process is AtomicProcess)
            {
                Process = (process as AtomicProcess).Process;
            }
            else
            {
                Process = process;                
            }

                ProcessID = DataStore.DataManager.InitializeProcessID(Constants.ATOMIC + Process.ProcessID);    
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            Process.MoveOneStep(GlobalEnv, list);

            foreach (Configuration configuration in list)
            {
                configuration.Process = new AtomicProcess(configuration.Process);
                configuration.IsAtomic = true;
            }
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            return new AtomicProcess(Process.ClearConstant(constMapping));
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Process result = new AtomicProcess(Process.Rename(constMapping, newDefNames, renamedProcesses));
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }
#endif
        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            Process.SyncOutput(GlobalEnv, list);
            foreach (ConfigurationWithChannelData step in list)
            {
                step.Process = new AtomicProcess(step.Process);
                step.IsAtomic = true;
            }
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            Process.SyncInput(eStep, list);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Process = new AtomicProcess(list[i].Process);
                list[i].IsAtomic = true;
            }
        }

        public override string ToString()
        {
            return " atomic{" + Process + "}";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            return Process.GetAlphabets(visitedDefinitionRefs);
        }

        public override List<string> GetGlobalVariables()
        {
            return Process.GetGlobalVariables();
        }

        public override List<string> GetChannels()
        {
            return Process.GetChannels();
        }

        public override bool MustBeAbstracted()
        {
            return Process.MustBeAbstracted();
        }

        public override Process GetTopLevelConcurrency(List<string> visitedDef)
        {
            return Process.GetTopLevelConcurrency(visitedDef);
        }

        public override bool IsSkip()
        {
            return Process.IsSkip();
        }
    }
}
