using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PAT.Common.Classes.CUDDLib;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.LTS;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class IndexExternalChoice : Process
    {
        public List<Process> Processes;

        //interleave based on the indexed definitions.
        public IndexedProcess IndexedProcessDefinition;

        public IndexExternalChoice(IndexedProcess definition)
        {
            IndexedProcessDefinition = definition;
        }

        public IndexExternalChoice(List<Process> processes)
        {
            Processes = new List<Process>();
            SortedDictionary<string, bool> existed = new SortedDictionary<string, bool>();

            foreach (Process proc in processes)
            {
                if (proc is IndexExternalChoice && (proc as IndexExternalChoice).Processes != null)
                {
                    List<Process> processes1 = (proc as IndexExternalChoice).Processes;

                    foreach (Process processe in processes1)
                    {
                        if (!existed.ContainsKey(processe.ProcessID))
                        {
                            Processes.Add(processe);
                            existed.Add(processe.ProcessID, false);
                        }
                    }
                }
                else
                {
                    if (!existed.ContainsKey(proc.ProcessID))
                    {
                        Processes.Add(proc);
                        existed.Add(proc.ProcessID, false);
                    }
                }
            }

            Debug.Assert(Processes.Count > 0);

            if (Processes.Count > 1)
            {
                StringBuilder ID = new StringBuilder();
                foreach (string id in existed.Keys)
                {
                    ID.Append(Constants.EXTERNAL_CHOICE);
                    ID.Append(id);
                }
                ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
            }
            else
            {
                ProcessID = Processes[0].ProcessID;
            }
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            for (int i = 0; i < Processes.Count; i++)
            {
                List<Configuration> list1 = new List<Configuration>();
                Processes[i].MoveOneStep(GlobalEnv, list1);

                foreach (Configuration configuration in list1)
                {
                    if (configuration.Event == Constants.TAU)
                    {
                        List<Process> newProcess = new List<Process>(Processes);
                        newProcess[i] = configuration.Process;

                        IndexExternalChoice choice = new IndexExternalChoice(newProcess);
                        configuration.Process = choice;

                    }

                    list.Add(configuration);
                }
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

        public override string ToString()
        {
            if (Processes == null)
            {
                return "[*]" + IndexedProcessDefinition.ToString();
            }

            string result = "(" + Processes[0].ToString();
            for (int i = 1; i < Processes.Count; i++)
            {
                result += "[*]";
                result += Processes[i].ToString();
            }
            result += ")";
            return result;
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
                    return new IndexExternalChoice(IndexedProcessDefinition.ClearConstant(constMapping));
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

            return new IndexExternalChoice(newListProcess);
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
                Process stopProcess = new Stop();
                stopProcess.IsBDDEncodableProp = Constants.BDD_LTS_COMPOSITION_3;
                return stopProcess;
            }

            Process result = new IndexExternalChoice(newListProcess);
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
            for(int i = 0; i < Processes.Count; i++)
            {
                List<Expression> guardsTemp = new List<Expression>();
                List<Event> eventsTemp = new List<Event>();
                List<Expression> programBlocksTemp = new List<Expression>();
                List<Process> processesTemp = new List<Process>();
                Processes[i].MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);

                for (int j = 0; j < eventsTemp.Count; j++)
                {
                    if (eventsTemp[j].BaseName != Constants.TAU)
                    {
                        guards.Add(guardsTemp[j]);
                        events.Add(eventsTemp[j]);
                        programBlocks.Add(programBlocksTemp[j]);
                        processes.Add(processesTemp[j]);
                    }
                    else
                    {
                        //choice is not resolved
                        List<Process> newChoice = new List<Process>(Processes);
                        newChoice[i] = processesTemp[j];

                        guards.Add(guardsTemp[j]);
                        events.Add(eventsTemp[j]);
                        programBlocks.Add(programBlocksTemp[j]);
                        processes.Add(new IndexExternalChoice(newChoice));
                    }
                }
            }
        }
        
        /// <summary>
        /// Could not support GenerateSymbolicLTS because if tau transition happen, choice is not resolve. Diffcult to track current state
        /// </summary>
        /// <param name="encoder"></param>
        /// <returns></returns>
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
            return AutomataBDD.ExternalChoice(processAutomataBDDs, encoder.model);
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

    }
}