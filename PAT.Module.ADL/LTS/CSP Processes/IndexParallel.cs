using System;
using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.CUDDLib;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class IndexParallel : Process
    {
        public List<Process> Processes;
        private HashSet<string>[] Alphabets;
        public IndexedProcess IndexedProcessDefinition;

        public IndexParallel (IndexedProcess indexedProcessDefinition)
        {
            IndexedProcessDefinition = indexedProcessDefinition;
        }

        public IndexParallel(List<Process> processes)
        {
            Processes = processes;

            ProcessID = Constants.PARALLEL + Processes[0].ProcessID;
            for (int i = 1; i < Processes.Count; i++)
            {
                ProcessID += Constants.SEPARATOR + Processes[i].ProcessID;
            }
            ProcessID = DataStore.DataManager.InitializeProcessID(ProcessID);
        }

        private IndexParallel(List<Process> processes, HashSet<string>[] alphabets)
        {
            Processes = processes;
            Alphabets = alphabets;

            ProcessID = Constants.PARALLEL + Processes[0].ProcessID;
            for (int i = 1; i < Processes.Count; i++)
            {
                ProcessID += Constants.SEPARATOR + Processes[i].ProcessID;
            }
            ProcessID = DataStore.DataManager.InitializeProcessID(ProcessID);
        }

        private void IdentifySharedEventsAndVariables()
        {
            try
            {
                //SynEvents = new List<string>();
                //the following code is used to handle the special case where (e -> e{x=1;} -> P) || e -> Q;
                //the rule is that a shared event can not have the same name as a local action.
                //CheckNameConflictBetweenSynAndLocalAction(new Dictionary<string, bool>(), new List<string>());
                Dictionary<string, int> EventSharedness = new Dictionary<string, int>();

                Alphabets = new HashSet<string>[Processes.Count];
                for (int i = 0; i < Processes.Count; i++)
                {
                    //the alphabet returned shall not have duplicates!!!!
                    HashSet<string> alphabet = Processes[i].GetAlphabets(new Dictionary<string, string>());
                    alphabet.Add(Constants.TERMINATION);
                    Alphabets[i] = alphabet;

                    foreach (string s in alphabet)
                    {
                        if (!EventSharedness.ContainsKey(s))
                        {
                            EventSharedness.Add(s, 1);
                        }
                        else
                        {
                            EventSharedness[s] = EventSharedness[s] + 1;
                        }
                    }
                }

                //remove events which is not shared.
                for (int i = 0; i < Processes.Count; i++)
                {
                    HashSet<string> alphabet = Alphabets[i];
                    foreach (KeyValuePair<string, int> pair in EventSharedness)
                    {
                        if (pair.Value == 1 && alphabet.Contains(pair.Key))
                        {
                            alphabet.Remove(pair.Key);
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                Alphabets = null;
                throw;
            }
        }
  
        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            if (Alphabets == null)
            {
                IdentifySharedEventsAndVariables();
            }

            List<string> barrierEnabledEvents = new List<string>();
            List<string> disabled = new List<string>();

            System.Diagnostics.Debug.Assert(list.Count == 0);

            Dictionary<string, List<Configuration>> syncSteps = new Dictionary<string, List<Configuration>>();
            for (int i = 0; i < Processes.Count; i++)
            {
                //Process process = Processes[i];
                List<Configuration> list1 = new List<Configuration>();
                Processes[i].MoveOneStep(GlobalEnv, list1);

                List<string> enabled = new List<string>(list1.Count);

                for (int j = 0; j < list1.Count; j++)
                {
                    Configuration step = list1[j];

                    string evt = step.Event;
                    //liuyang, commented here
                    //enabled.Add(evt);

                    //if it happens that a data operation shares the same name with the sync event, treat it as an interleave case
                    if (!Alphabets[i].Contains(evt) || step.IsDataOperation)
                    {
                        if (AssertionBase.CalculateParticipatingProcess)
                        {
                            step.ParticipatingProcesses = new string[] { i.ToString() };
                        }

                        List<Process> newProcess = new List<Process>(Processes.Count);
                        newProcess.AddRange(Processes);
                        newProcess[i] = step.Process;

                        step.Process = new IndexParallel(newProcess, Alphabets);
                        list.Add(step);
                    }
                    else
                    {
                        //liuyang, added here
                        enabled.Add(evt);

                        string key = evt + Constants.SEPARATOR + i;

                        if (!syncSteps.ContainsKey(key))
                        {
                            syncSteps.Add(key, new List<Configuration>());
                        }

                        syncSteps[key].Add(step);

                        if (!barrierEnabledEvents.Contains(evt)) //Alphabets[i].Contains(evt) && 
                        {
                            barrierEnabledEvents.Add(evt);
                        }
                    } 
                }

                //int alphabetsCount = Alphabets[i].Count;
                foreach (string s in Alphabets[i])
                {
                    if (!enabled.Contains(s) && !disabled.Contains(s))
                    {
                        disabled.Add(s);
                    }
                }

                //to check whether there are synchoronous channel input/output
                if (SpecificationBase.HasSyncrhonousChannel)
                {
                    SynchronousChannelInputOutput(list, i, GlobalEnv, null);
                }
            }
            
            int disabledCount = disabled.Count;
            for (int i = 0; i < disabledCount; i++)
            {
                barrierEnabledEvents.Remove(disabled[i]);
            }
            
            List<bool> isAtomic = null;

            //move the barrier synchronization events.
            foreach (string evt in barrierEnabledEvents)
            {
                //maps an event to the list of resulting processes.
                List<List<Process>> moves = new List<List<Process>>();
                moves.Add(new List<Process>());

                if (SpecificationBase.HasAtomicEvent)
                {
                    isAtomic = new List<bool>();
                    isAtomic.Add(false);
                }

                List<string> participatingProcesses = new List<string>();

                for (int i = 0; i < Processes.Count; i++)
                {
                    if (Alphabets[i].Contains(evt))
                    {
                        participatingProcesses.Add(i.ToString());

                        List<Configuration> steps = syncSteps[evt + Constants.SEPARATOR + i]; //Processes[i].MoveOneStep(eStep, evt);

                        List<List<Process>> toAdd = new List<List<Process>>(moves.Count);

                        foreach (Configuration step in steps)
                        {
                            //if it happens that a data operation shares the same name with the sync event, ignore
                            if (!step.IsDataOperation)
                            {
                                if (moves[0].Count == i)
                                {
                                    foreach (List<Process> list2 in moves)
                                    {
                                        list2.Add(step.Process);
                                        if (step.IsAtomic)
                                        {
                                            for (int j = 0; j < isAtomic.Count; j++)
                                            {
                                                isAtomic[j] = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //If there non-determinism, clone and then add.
                                    
                                    for (int k = 0; k < moves.Count; k++)
                                    {
                                        List<Process> list2 = moves[k];

                                        List<Process> newProcList = new List<Process>();

                                        for (int j = 0; j < list2.Count - 1; j++)
                                        {
                                            newProcList.Add(list2[j]);
                                        }

                                        newProcList.Add(step.Process);
                                        toAdd.Add(newProcList);


                                        if (SpecificationBase.HasAtomicEvent)
                                        {
                                            if (!isAtomic[k])
                                            {
                                                isAtomic.Add(step.IsAtomic);
                                            }
                                            else
                                            {
                                                isAtomic.Add(true);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        moves.AddRange(toAdd);
                    }
                    else
                    {
                        foreach (List<Process> move in moves)
                        {
                            move.Add(Processes[i]);
                        }
                    }
                }

                for (int i = 0; i < moves.Count; i++)
                {
                    List<Process> list2 = moves[i];
                    IndexParallel para = new IndexParallel(list2, Alphabets);

                    Configuration tmpStep = new Configuration(para, evt, null, GlobalEnv, false);

                    if (SpecificationBase.HasAtomicEvent)
                    {
                        tmpStep.IsAtomic = isAtomic[i];
                    }

                    if (AssertionBase.CalculateParticipatingProcess)
                    {
                        tmpStep.ParticipatingProcesses = participatingProcesses.ToArray();
                    }

                    list.Add(tmpStep);
                }
            }

            //return returnList;
        }

        private void SynchronousChannelInputOutput(List<Configuration> returnList, int i, Valuation GlobalEnv, string evt)
        {
            List<ConfigurationWithChannelData> outputs = new List<ConfigurationWithChannelData>();
                Processes[i].SyncOutput(GlobalEnv,outputs);

            foreach (ConfigurationWithChannelData vm in outputs)
            {
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
                        Processes[k].SyncInput(vm, syncedProcess);

                        foreach (Configuration p in syncedProcess)
                        {
                            List<Process> newProcess = new List<Process>(Processes.Count);
                            newProcess.AddRange(Processes);
                            newProcess[i] = output;
                            newProcess[k] = p.Process;

                            IndexParallel interleave = new IndexParallel(newProcess, Alphabets);
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

                for (int j = 0; j < list1.Count; j++)
                {
                    Configuration step = list1[j];

                    List<Process> newProcess = new List<Process>(Processes.Count);
                    newProcess.AddRange(Processes);
                    newProcess[i] = step.Process;

                    step.Process = new IndexParallel(newProcess, Alphabets);
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
                    step.Process = new IndexParallel(newProcess, Alphabets);
                    list.Add(step);
                }
            }

            //return returnList;
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            if (Processes == null)
            {
                Process parallelProcess = this.ClearConstant(new Dictionary<string, Expression>());
                return parallelProcess.GetAlphabets(visitedDefinitionRefs);
                //return IndexedProcessDefinition.Process.GetAlphabets(visitedDefinitionRefs);
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

            List<string> vars = new List<string>();

            foreach (Process var in Processes)
            {
                Common.Classes.Ultility.Ultility.AddList(vars, var.GetGlobalVariables());
            }

            return vars;
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
                return "||" + IndexedProcessDefinition;
            }

            StringBuilder s = new StringBuilder();

            for (int i = 0; i < Processes.Count; i++)
            {
                Process process = Processes[i];
                s.Append(process.ToString() + "\r\n||");
            }
            return "(" + s.ToString().TrimEnd('|') + ")";
        }

//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            if (Alphabets == null)
//            {
//                IdentifySharedEventsAndVariables();
//            }

//            List<string> resultList = new List<string>(Processes.Count);

//            List<Configuration> enabled = null;
//            int processesCount = Processes.Count;

//            //local action will make the process index to be true.
//            for (int i = 0; i < processesCount; i++)
//            {
//                enabled = new List<Configuration>();
//                Processes[i].MoveOneStep(GlobalEnv, enabled);
//                //enabled = Processes[i].GetEnabled(eStep);

//                foreach (Configuration s in enabled)
//                {
//                    if (!Alphabets[i].Contains(s.Event))
//                    {
//                        //resultList.Add(i.ToString());
//                        Common.Classes.Ultility.Ultility.AddList(resultList, Processes[i].GetActiveProcesses(GlobalEnv));

//                        break;
//                    }
//                }

//                //to check whether there are synchoronous channel input/output
//                if (SpecificationBase.HasSyncrhonousChannel)
//                {
//                    List<ConfigurationWithChannelData> outputs = new List<ConfigurationWithChannelData>();
//                    Processes[i].SyncOutput(GlobalEnv, outputs);

//                    foreach (ConfigurationWithChannelData vm in outputs)
//                    {
//                        for (int k = 0; k < Processes.Count; k++)
//                        {
//                            if (k != i)
//                            {
//                                List<Configuration> syncedProcess = new List<Configuration>();
//                                Processes[k].SyncInput(vm, syncedProcess);

//                                if (syncedProcess.Count > 0)
//                                {
//                                    Common.Classes.Ultility.Ultility.AddList(resultList, Processes[i].GetActiveProcesses(GlobalEnv));
//                                    Common.Classes.Ultility.Ultility.AddList(resultList, Processes[k].GetActiveProcesses(GlobalEnv));
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            enabled = new List<Configuration>();
//            MoveOneStep(GlobalEnv, enabled);

//            //shared actions will make the process index to be true.
//            foreach (Configuration s in enabled)
//            {
//                for (int i = 0; i < Alphabets.Length; i++)
//                {
//                    if(Alphabets[i].Contains(s.Event))
//                    {
//                        Common.Classes.Ultility.Ultility.AddList(resultList, Processes[i].GetActiveProcesses(GlobalEnv));
//                    }
//                }
//            }

//            return resultList;

//        }
//#endif
        
        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            List<Process> newnewProcesses = Processes;

            List<Process> newProceses;
            int size;
            if (Specification.IsParsing)
            {
                if (Processes == null)
                {
                    return new IndexParallel(IndexedProcessDefinition.ClearConstant(constMapping));
                }
                size = Processes.Count;
                newProceses  = new List<Process>(size); 
                for (int i = 0; i < size; i++)
                {
                    newProceses.Add(Processes[i].ClearConstant(constMapping));
                }
            }

            if (Processes == null)
            {
                newnewProcesses = IndexedProcessDefinition.GetIndexedProcesses(constMapping);
            }

            size = newnewProcesses.Count;

            newProceses = new List<Process>(size); 
            for (int i = 0; i < size; i++)
            {
                Process newProc = newnewProcesses[i].ClearConstant(constMapping);

                if (newProc is IndexParallel && (newProc as IndexParallel).Processes != null)
                {
                    List<Process> processes = (newProc as IndexParallel).Processes;
                    newProceses.AddRange(processes);
                }
                else
                {
                    newProceses.Add(newProc);
                }
            }

            return new IndexParallel(newProceses);
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

            Process result = new IndexParallel(newProceses);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
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

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            List<Process> newnewListProcess = Processes;
            if (Processes == null)
            {
                newnewListProcess = IndexedProcessDefinition.GetIndexedProcesses(new Dictionary<string, Expression>());
            }

            List<AutomataBDD> processBDDs = new List<AutomataBDD>();
            List<CUDDNode> alphabets = new List<CUDDNode>();

            foreach (Process process in newnewListProcess)
            {
                processBDDs.Add(process.Encode(encoder));
                alphabets.Add(encoder.GetAlphabetInBDD(process.GetAlphabets(new Dictionary<string, string>())));
            }

            //
            return AutomataBDD.Parallel(processBDDs, alphabets, encoder.model);
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