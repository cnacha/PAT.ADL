using System;
using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class IndexInternalChoice:Process
    {
        public List<Process> Processes;
        public IndexedProcess IndexedProcessDefinition;

        public IndexInternalChoice(IndexedProcess definition)
        {
            IndexedProcessDefinition = definition;
        }

        public IndexInternalChoice(List<Process> processes)
        {
            Processes = new List<Process>();
            StringBuilder ID = new StringBuilder();

            foreach (Process proc in processes)
            {
                if (proc is IndexInternalChoice && (proc as IndexInternalChoice).Processes != null)
                {
                    List<Process> processes1 = (proc as IndexInternalChoice).Processes;

                    Processes.AddRange(processes1);

                    foreach (Process processe in processes1)
                    {
                        ID.Append(Constants.INTERNAL_CHOICE);
                        ID.Append(processe.ProcessID);
                    }
                }
                else
                {
                    Processes.Add(proc);
                    ID.Append(Constants.INTERNAL_CHOICE);
                    ID.Append(proc.ProcessID);
                }
            }

            ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            for (int i = 0; i < Processes.Count; i++)
            {
                list.Add(new Configuration(Processes[i], Constants.TAU, "[int_choice]", GlobalEnv, false));
            }

            //return list;
        }


        public override string ToString()
        {
            if (Processes == null)
            {
                return "<>" + IndexedProcessDefinition;
            }

            string result = "(" + Processes[0];
            for (int i = 1; i < Processes.Count; i++)
            {
                result += "<>";
                result += Processes[i].ToString();
            }
            result += ")";
            return result;
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
                Common.Classes.Ultility.Ultility.AddList<string>(Variables, Processes[i].GetGlobalVariables());
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
            for (int i = 0; i < this.Processes.Count; i++)
            {
                Common.Classes.Ultility.Ultility.AddList<string>(channels, this.Processes[i].GetChannels());
            }
            return channels;
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
                list.UnionWith(this.Processes[i].GetAlphabets(visitedDefinitionRefs));
            }
            return list;
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            List<Process> newnewListProcess = Processes;
            if (Processes == null)
            {
                if (Specification.IsParsing)
                {
                    return new IndexInternalChoice(IndexedProcessDefinition.ClearConstant(constMapping));
                }

                newnewListProcess = IndexedProcessDefinition.GetIndexedProcesses(constMapping);
            }

            List<Process> newListProcess = new List<Process>();
            for (int i = 0; i < newnewListProcess.Count; i++)
            {
                Process newProc = newnewListProcess[i].ClearConstant(constMapping);
                newListProcess.Add(newProc);
            }
            return new IndexInternalChoice(newListProcess);
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
                newListProcess.Add(newProc);
            }
            Process result = new IndexInternalChoice(newListProcess);
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
                guards.Add(null);
                events.Add(new Event(Constants.TAU));
                programBlocks.Add(null);
                processes.Add(Process);
            }
        }

        /// <summary>
        /// 
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
            return AutomataBDD.InternalChoice(processAutomataBDDs, encoder.model);
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

    }
}