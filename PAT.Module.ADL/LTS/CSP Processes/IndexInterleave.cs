using System;
using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class IndexInterleave : Process
    {
        public List<Process> Processes;
        public IndexedProcess IndexedProcessDefinition;

        public IndexInterleave(IndexedProcess definition)
        {
            IndexedProcessDefinition = definition;
        }

        public IndexInterleave(List<Process> processes)
        {
            Processes = processes;

            List<string> tmp = new List<string>(Processes.Count);
            bool hasStop = false;

            for (int i = 0; i < Processes.Count; i++)
            {
                if (Processes[i] is IndexInterleave)
                {
                    IndexInterleave newProc = (Processes[i] as IndexInterleave);

                    if (newProc.Processes != null)
                    {
                        foreach (Process processe in newProc.Processes)
                        {
                            if (!hasStop && processe is Stop)
                            {
                                hasStop = true;
                            }

                            if (!(processe is Stop && hasStop) && !(processe is Skip))
                            {
                                tmp.Add(processe.ProcessID);
                            }
                        }
                    }
                    else
                    {
                        tmp.Add(newProc.IndexedProcessDefinition.ToString());
                    }
                }
                else
                {
                    if (!hasStop && Processes[i] is Stop)
                    {
                        hasStop = true;
                    }

                    if (!(Processes[i] is Stop && hasStop) && !(Processes[i] is Skip))
                    {
                        tmp.Add(Processes[i].ProcessID);
                    }
                }
            }

            if (tmp.Count == 0)
            {
                if (hasStop)
                {
                    Stop stop = new Stop();
                    Processes = new List<Process>();
                    Processes.Add(stop);
                    ProcessID = stop.ProcessID;
                }
                else
                {
                    Skip skip = new Skip();
                    Processes = new List<Process>();
                    Processes.Add(skip);
                    ProcessID = skip.ProcessID;
                }
            }
            else
            {
                //tmp.Sort();

                ProcessID = Constants.INTERLEAVE + tmp[0];
                for (int i = 1; i < tmp.Count; i++)
                {
                    ProcessID += Constants.SEPARATOR + tmp[i];
                }

                ProcessID = DataStore.DataManager.InitializeProcessID(ProcessID);
            }
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            bool allTerminationCount = true;
            bool hasAtomicTermination = false;

            for (int i = 0; i < Processes.Count; i++)
            {
                Process process = Processes[i];
                List<Configuration> list1 = new List<Configuration>();
                process.MoveOneStep(GlobalEnv, list1);
                bool hasTermination = false;

                for (int j = 0; j < list1.Count; j++)
                {
                    Configuration step = list1[j];

                    if (step.Event == Constants.TERMINATION)
                    {
                        hasTermination = true;

                        if (step.IsAtomic)
                        {
                            hasAtomicTermination = true;
                        } 
                    }
                    else
                    {
                        if (AssertionBase.CalculateParticipatingProcess)
                        {
                            step.ParticipatingProcesses = new string[] {i.ToString()};
                        }

                        List<Process> newProcess = new List<Process>(Processes.Count);
                        newProcess.AddRange(Processes);
                        newProcess[i] = step.Process;

                        IndexInterleave interleave = new IndexInterleave(newProcess);
                        step.Process = interleave;
                        list.Add(step);
                    }
                }

                //to check whether there are synchoronous channel input/output
                if (SpecificationBase.HasSyncrhonousChannel)
                {
                    SynchronousChannelInputOutput(list, i, GlobalEnv, null);
                }

                if (!hasTermination)
                {
                    allTerminationCount = false;
                }
            }

            if (allTerminationCount)
            {
                Configuration temp = new Configuration(new Stop(), Constants.TERMINATION, null, GlobalEnv, false);

                if (hasAtomicTermination)
                {
                    temp.IsAtomic = true;
                }

                if (AssertionBase.CalculateParticipatingProcess)
                {
                    temp.ParticipatingProcesses = new string[Processes.Count];
                    for (int i = 0; i < Processes.Count; i++)
                    {
                        temp.ParticipatingProcesses[i] = i.ToString();
                    }  
                }
                list.Add(temp);
            }
            
            //return returnList;
        }

        private void SynchronousChannelInputOutput(List<Configuration> returnList, int i, Valuation GlobalEnv, string evt)
        {
            List<ConfigurationWithChannelData> outputs = new List<ConfigurationWithChannelData>();
            Processes[i].SyncOutput(GlobalEnv, outputs);

            foreach (ConfigurationWithChannelData pair in outputs)
            {
                Configuration vm = pair;
 
                if(evt != null && vm.Event != evt)
                {
                    continue;    
                }
                
                Process output = vm.Process;

                for (int k = 0; k < Processes.Count; k++)
                {
                    if (k != i)
                    {
                        List<Configuration> syncedProcess = new List<Configuration>();
                        Processes[k].SyncInput(pair, syncedProcess);

                        foreach (Configuration p in syncedProcess)
                        {
                            List<Process> newProcess = new List<Process>(Processes.Count);
                            newProcess.AddRange(Processes);
                            newProcess[i] = output;
                            newProcess[k] = p.Process;
                            
                            IndexInterleave interleave = new IndexInterleave(newProcess);
                            Configuration newStep = new Configuration(interleave, vm.Event, vm.DisplayName, p.GlobalEnv, false);
                            newStep.IsAtomic = vm.IsAtomic || p.IsAtomic;

                            if (AssertionBase.CalculateParticipatingProcess)
                            {
                                newStep.ParticipatingProcesses = new string[]{i.ToString(), k.ToString()};
                            }

                            returnList.Add(newStep);
                        }
                    }
                }
            }
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            //List<ConfigurationWithChannelData> returnList = new List<ConfigurationWithChannelData>();
            
            for (int i = 0; i < Processes.Count; i++)
            {
                Process process = Processes[i];
                List<ConfigurationWithChannelData> list1 = new List<ConfigurationWithChannelData>();
                process.SyncOutput(GlobalEnv, list1);

                foreach (ConfigurationWithChannelData pair in list1)
                {
                    Configuration step = pair;

                    List<Process> newProcess = new List<Process>(Processes.Count);
                    newProcess.AddRange(Processes);
                    newProcess[i] = step.Process;

                    step.Process = new IndexInterleave(newProcess);
                }

                list.AddRange(list1);
            }

            //return returnList;
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            //List<Configuration> returnList = new List<Configuration>();
            
            for (int i = 0; i < Processes.Count; i++)
            {
                Process process = Processes[i];
                List<Configuration> list1 = new List<Configuration>();
                process.SyncInput(eStep, list1);

                for (int j = 0; j < list1.Count; j++)
                {
                    Configuration step = list1[j];

                    List<Process> newProcess = new List<Process>(Processes.Count);
                    newProcess.AddRange(Processes);
                    newProcess[i] = step.Process;
                    step.Process = new IndexInterleave(newProcess);
                    list.Add(step);
                }
            }
        }

//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            List<string> resultList = new List<string>(Processes.Count);
//            int terminateCount = 0;

//            for (int i = 0; i < Processes.Count; i++)
//            {
//                List<Configuration> enabled = new List<Configuration>();
//                Processes[i].MoveOneStep(GlobalEnv, enabled);

//                foreach (Configuration s in enabled)
//                {
//                    if (s.Event == Constants.TERMINATION && terminateCount == i)
//                    {
//                        terminateCount++;
//                    }
//                    else
//                    {
//                        Common.Classes.Ultility.Ultility.AddList(resultList, Processes[i].GetActiveProcesses(GlobalEnv));
//                       // resultList.AddRange(Processes[i].GetActiveProcesses(eStep));
//                        break;
//                    }
//                }

//                //if (!resultList.Contains(i.ToString()))
//                {
//                    if (SpecificationBase.HasSyncrhonousChannel)
//                    {
//                        List<ConfigurationWithChannelData> outputs = new List<ConfigurationWithChannelData>();
//                        Processes[i].SyncOutput(GlobalEnv, outputs);

//                        foreach (ConfigurationWithChannelData pair in outputs)
//                        {
//                            for (int k = 0; k < Processes.Count; k++)
//                            {
//                                if (k != i)
//                                {
//                                    List<Configuration> syncedProcess = new List<Configuration>();
//                                    Processes[k].SyncInput(pair, syncedProcess);

//                                    if (syncedProcess.Count > 0)
//                                    {
//                                        Common.Classes.Ultility.Ultility.AddList(resultList, Processes[i].GetActiveProcesses(GlobalEnv));

//                                        //if (!resultList.Contains(i.ToString()))
//                                        //{
//                                        //    resultList.Add(i.ToString());
//                                        //}

//                                        Common.Classes.Ultility.Ultility.AddList(resultList, Processes[k].GetActiveProcesses(GlobalEnv));

//                                        //if (!resultList.Contains(k.ToString()))
//                                        //{
//                                        //    resultList.Add(k.ToString());
//                                        //}

//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            if (terminateCount == Processes.Count)
//            {
//                for (int i = 0; i < Processes.Count; i++)
//                {
//                    Common.Classes.Ultility.Ultility.AddList(resultList, Processes[i].GetActiveProcesses(GlobalEnv));

//                    //if (resultList.Contains(i.ToString()))
//                    //{
//                    //    resultList.Add(i.ToString());
//                    //}
//                }
//            }

//            return resultList;
//        }
//#endif

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.Process.GetAlphabets(visitedDefinitionRefs);
            }

            HashSet<string> toReturn = new HashSet<string>();

            for (int i = 0; i < Processes.Count; i++)
            {
                Process process = Processes[i];
                toReturn.UnionWith(process.GetAlphabets(visitedDefinitionRefs));
            }

            return toReturn;
        }

        public override List<string> GetGlobalVariables()
        {
            if (Processes == null)
            {
                return IndexedProcessDefinition.GetGlobalVariables();
            }

            List<string> Variables = new List<string>();

            foreach (Process var in Processes)
            {
                Common.Classes.Ultility.Ultility.AddList(Variables, var.GetGlobalVariables());
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

            foreach (Process var in Processes)
            {
                Common.Classes.Ultility.Ultility.AddList(channels, var.GetChannels());
            }

            return channels;
        }

        public override string ToString()
        {
            if (Processes == null)
            {
                return "|||" + IndexedProcessDefinition;
            }

            StringBuilder s = new StringBuilder();

            for (int i = 0; i < Processes.Count; i++)
            {
                Process process =  Processes[i];
                s.Append("(" + process.ToString() + ")\r\n|||");
            }
            return "(" + s.ToString().TrimEnd('|') + ")";
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            List<Process> newProceses = new List<Process>();

            if (Specification.IsParsing)
            {
                if (Processes == null)
                {
                    return new IndexInterleave(IndexedProcessDefinition.ClearConstant(constMapping));
                }

                foreach (Process procese in Processes)
                {
                    newProceses.Add(procese.ClearConstant(constMapping));
                }

                return new IndexInterleave(newProceses);
            }

            List<Process> newnewProcesses = Processes;

            if(Processes == null)
            {
                //return new IndexInterleave(IndexedProcessDefinition.GetIndexedProcesses(constMapping));                    
                newnewProcesses = IndexedProcessDefinition.GetIndexedProcesses(constMapping);
            }

            int size = newnewProcesses.Count;
            Dictionary<string, int> processCounters = new Dictionary<string, int>(size);

            for (int i = 0; i < size; i++)
            {
                Process newProc = newnewProcesses[i].ClearConstant(constMapping);

                if (newProc is IndexInterleave)
                {
                    List<Process> processes = (newProc as IndexInterleave).Processes;

                    foreach (var processe in processes)
                    {
                        string tmp = processe.ProcessID;
                        if (!processCounters.ContainsKey(tmp))
                        {
                            newProceses.Add(processe);
                            processCounters.Add(tmp, 1);
                        }
                        else
                        {
                            processCounters[tmp] = processCounters[tmp] + 1;
                        }
                    }
                }
                else if (newProc is IndexInterleaveAbstract)
                {
                    IndexInterleaveAbstract intAbs = newProc as IndexInterleaveAbstract;

                    foreach (var processe in intAbs.Processes)
                    {
                        string tmp = processe.ProcessID;
                        if (!processCounters.ContainsKey(tmp))
                        {
                            newProceses.Add(processe);
                            processCounters.Add(tmp, intAbs.ProcessesActualSize[tmp]);
                        }
                        else
                        {
                            processCounters[tmp] = processCounters[tmp] + intAbs.ProcessesCounter[tmp];
                        }
                    }
                }
                else
                {
                    if (!processCounters.ContainsKey(newProc.ProcessID))
                    {
                        newProceses.Add(newProc);
                        processCounters.Add(newProc.ProcessID, 1);
                    }
                    else
                    {
                        processCounters[newProc.ProcessID] = processCounters[newProc.ProcessID] + 1;
                    }
                }
            }

            foreach (KeyValuePair<string, int> pair in processCounters)
            {
                if (pair.Value > 1 || pair.Value == -1)
                {
                    IndexInterleaveAbstract toReturn = new IndexInterleaveAbstract(newProceses, processCounters);
                    toReturn.ProcessesActualSize = new Dictionary<string, int>(processCounters);
                    return toReturn;
                }
            }

            return new IndexInterleave(newProceses);
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            List<Process> newnewListProcess = Processes;
            if (Processes == null)
            {
                newnewListProcess = IndexedProcessDefinition.GetIndexedProcesses(constMapping);
            }

            List<Process> newProceses = new List<Process>();
            foreach (Process process in newnewListProcess)
            {
                newProceses.Add(process.Rename(constMapping, newDefNames, renamedProcesses));
            }

            Process result = new IndexInterleave(newProceses);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
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
            return AutomataBDD.Interleave(processAutomataBDDs, encoder.model);
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

        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            if (Processes == null)
            {
                int result = IndexedProcessDefinition.IsBDDEncodable(calledProcesses);

                //interleave is only encoded by composition.
                //if sub processes are not bdd encodable then the interleave is too
                //if sub process are encodable then the whole is encodable and encoded by composition
                return (IsBDDEncodableProp = Math.Min(result, Constants.BDD_COMPOSITION_1));
            }
            else
            {
                int min = Constants.BDD_COMPOSITION_1;
                for (int i = 0; i < Processes.Count; i++)
                {
                    min = Math.Min(min, Processes[i].IsBDDEncodable(calledProcesses));
                }

                return (IsBDDEncodableProp = min);
            }
        }

#endif

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

  
        public override Process GetTopLevelConcurrency(List<string> visitedDef)
        {
            return this;
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