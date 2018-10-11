using System;
using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using Expression = PAT.Common.Classes.Expressions.ExpressionClass.Expression;
using PAT.Common.Classes.LTS;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class IndexChoice : Process
    {
        public List<Process> Processes;
        public IndexedProcess IndexedProcessDefinition;

        #region constructors 

        /// <summary>
        /// constructor for indexed choice.
        /// </summary>
        /// <param name="definition"></param>
        public IndexChoice(IndexedProcess definition)
        {
            IndexedProcessDefinition = definition;
        }

        /// <summary>
        /// constructor for choices of multiple processes.
        /// </summary>
        /// <param name="processes"></param>
        public IndexChoice(List<Process> processes)
        {
            Processes = processes;

            StringBuilder ID = new StringBuilder();
            foreach (Process processBase in Processes)
            {
                ID.Append(Constants.GENERAL_CHOICE);
                ID.Append(processBase.ProcessID);
            }

            ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
        }
        #endregion

        #region Runtime functions
        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            for (int i = 0; i < Processes.Count; i++)
            {
                //note: here needs to create a new list1. reusing list will give error if the process[1] is a seqencial
                List<Configuration> list1 = new List<Configuration>();
                Processes[i].MoveOneStep(GlobalEnv, list1);
                list.AddRange(list1);
            }            
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                List<ConfigurationWithChannelData> list1 = new List<ConfigurationWithChannelData>();
                Processes[i].SyncOutput(GlobalEnv, list1);
                list.AddRange(list1);
            }
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            for (int i = 0; i < Processes.Count; i++)
            {
                List<Configuration> list1 = new List<Configuration>();
                Processes[i].SyncInput(eStep, list1);
                list.AddRange(list1);
            }
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.Process.GetAlphabets(visitedDefinitionRefs);
            }

            HashSet<string> list = new HashSet<string>();
            for (int i = 0; i < Processes.Count; i++)
            {
                list.UnionWith(Processes[i].GetAlphabets(visitedDefinitionRefs));
            }
            return list;
        }

//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            List<string> list = new List<string>();
//            for (int i = 0; i < Processes.Count; i++)
//            {
//                Common.Classes.Ultility.Ultility.AddList(list, Processes[i].GetActiveProcesses(GlobalEnv));
//            }
//            return list;
//        }
//#endif

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            List<Process> newnewListProcess = Processes;
            if (Processes == null)
            {
                if (Specification.IsParsing)
                {
                    return new IndexChoice(IndexedProcessDefinition.ClearConstant(constMapping));
                }

                newnewListProcess = IndexedProcessDefinition.GetIndexedProcesses(constMapping);
            }

            List<Process> newListProcess = new List<Process>();
            for (int i = 0; i < newnewListProcess.Count; i++)
            {
                Process newProc = newnewListProcess[i].ClearConstant(constMapping);
                if (!(newProc is Stop))
                {
                    newListProcess.Add(newProc);
                }
            }

            //all processes are stop processes
            if (newListProcess.Count == 0)
            {
                return new Stop();
            }

            return new IndexChoice(newListProcess);
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            List<Process> newnewListProcess = Processes;
            if (Processes == null)
            {
                newnewListProcess = IndexedProcessDefinition.GetIndexedProcesses(constMapping);
            }

            List<Process> newListProcess = new List<Process>();
            for (int i = 0; i < newnewListProcess.Count; i++)
            {
                Process newProc = newnewListProcess[i].Rename(constMapping, newDefNames, renamedProcesses);
                if (!(newProc is Stop))
                {
                    newListProcess.Add(newProc);
                }
            }

            //all processes are stop processes
            if (newListProcess.Count == 0)
            {
                return new Stop();
            }

            Process result = new IndexChoice(newListProcess);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }

        
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            if (Processes == null)
            {
                return IsBDDEncodableProp = IndexedProcessDefinition.IsBDDEncodable(calledProcesses);
            }
            else
            {
                int min = Constants.BDD_LTS_COMPOSITION_3;
                for (int i = 0; i < Processes.Count; i++)
                {
                    min = Math.Min(min, Processes[i].IsBDDEncodable(calledProcesses));
                }

                return (IsBDDEncodableProp = min);
            }
        }


        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            foreach (Process Process in Processes)
            {
                Process.MakeDiscreteTransition(guards, events, programBlocks, processes, model, lts);
            }
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            List<Process> newnewListProcess = Processes;
            if (Processes == null)
            {
                newnewListProcess = IndexedProcessDefinition.GetIndexedProcesses(new Dictionary<string, Expression>());
            }

            List<AutomataBDD> processAutomataBDDs = new List<AutomataBDD>();
            foreach (Process process in newnewListProcess)
            {
                processAutomataBDDs.Add(process.Encode(encoder));
            }

            //
            return AutomataBDD.Choice(processAutomataBDDs, encoder.model);
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            if (this.Processes == null)
            {
                this.IndexedProcessDefinition.CollectEvent(allEvents, calledProcesses);
            }
            else
            {
                foreach (Process process in this.Processes)
                {
                    process.CollectEvent(allEvents, calledProcesses);
                }
            }
        }

#endif
        #endregion

        #region static functions
        public override List<string> GetChannels()
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.Process.GetChannels();
            }

            List<string> channels = new List<string>();
            for (int i = 0; i < Processes.Count; i++)
            {
                List<string> temp = Processes[i].GetChannels();
                Common.Classes.Ultility.Ultility.AddList<string>(channels, temp);
            }
            return channels;
        }

        public override bool MustBeAbstracted()
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.Process.MustBeAbstracted();
            }

            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i].MustBeAbstracted())
                {
                    return true;
                }
            }

            return false;
        }


        public override List<string> GetGlobalVariables()
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.GetGlobalVariables();
            }

            List<string> Variables = new List<string>();
            for (int i = 0; i < Processes.Count; i++)
            {
                List<string> temp = Processes[i].GetGlobalVariables();
                Common.Classes.Ultility.Ultility.AddList<string>(Variables, temp);
            }
            return Variables;
        }

        public override bool IsSkip()
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.Process.IsSkip();
            }

            for (int i = 0; i < Processes.Count; i++)
            {
                if (!Processes[i].IsSkip())
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        public override string ToString()
        {
            if (Processes == null)
            {
                return "[]" + IndexedProcessDefinition;
            }

            string result = "(" + Processes[0];
            for (int i = 1; i < Processes.Count; i++)
            {
                result += "[]";
                result += Processes[i].ToString();
            }

            result += ")";
            return result;
        }

    }
}