using System.Collections.Generic;
using PAT.Common.Classes.CUDDLib;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class Hiding : Process
    {
        public Process Process;
        public EventCollection HidingAlphabets;

        public Hiding(Process process, EventCollection alphabets)
        {
            if (process is Hiding)
            {
                Process = (process as Hiding).Process;
                List<Event> events = new List<Event>((process as Hiding).HidingAlphabets);
                
                foreach (Event item in alphabets)
                {
                    if (!events.Contains(item))
                    {
                        events.Add(item);
                    }
                }

                HidingAlphabets = new EventCollection(events);        
            }
            else {
                Process = process;
                HidingAlphabets = alphabets;
            }

            ProcessID = DataStore.DataManager.InitializeProcessID(Constants.HIDING + Process.ProcessID + Constants.SEPARATOR + HidingAlphabets.ProcessID); 
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            Process.MoveOneStep(GlobalEnv, list); //List<Configuration> returnlist = 
            for (int i = 0; i < list.Count; i++)
            {
                Configuration step = list[i];

                if (HidingAlphabets.ContainEventName(step.Event))
                {
                    step.DisplayName = "[" + step.Event + "]";
                    step.Event = Constants.TAU;                    
                }

                Hiding newHide = new Hiding(step.Process, HidingAlphabets);

                step.Process = newHide;
                list[i] = step;
            }

        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            Process.SyncOutput(GlobalEnv, list);
            foreach (ConfigurationWithChannelData pair in list)
            {
                Configuration step = pair;
                step.Process = new Hiding(step.Process, HidingAlphabets);
            }
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            Process.SyncInput(eStep,list);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Process = new Hiding(list[i].Process, HidingAlphabets);
            }
        }
   
//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            return Process.GetActiveProcesses(GlobalEnv);
//        }
//#endif

        public override string ToString()
        {
            return "(" + Process.ToString() + " \\ {" + Common.Classes.Ultility.Ultility.PPStringList(HidingAlphabets) + "})";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            HashSet<string> returnlist = Process.GetAlphabets(visitedDefinitionRefs);
            foreach (string alphabet in HidingAlphabets.EventNames)
            {
                returnlist.Remove(alphabet);
            }    

            return returnlist;
        }

        public override List<string> GetGlobalVariables()
        {
            return Process.GetGlobalVariables();
        }

        public override List<string> GetChannels()
        {
            return Process.GetChannels();
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            return new Hiding(Process.ClearConstant(constMapping), HidingAlphabets.ClearConstant(constMapping));
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Process result = new Hiding(Process.Rename(constMapping, newDefNames, renamedProcesses), HidingAlphabets.ClearConstant(constMapping));
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }
        
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            return Constants.BDD_NON_ENCODABLE_0;
        }

#endif
        public override bool MustBeAbstracted()
        {
            return Process.MustBeAbstracted();
        }

        public override Process GetTopLevelConcurrency(List<string> visitedDef)
        {
            return Process.GetTopLevelConcurrency(visitedDef);
        }

    }
}