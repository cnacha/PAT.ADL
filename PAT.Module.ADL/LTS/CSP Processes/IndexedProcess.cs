using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.Ultility;

namespace PAT.ADL.LTS
{
    public class IndexedProcess
    {
        public Process Process;
        public List<ParallelDefinition> Definitions;
        public List<DefinitionRef> ContainedDefRefs;
        
        public IndexedProcess(Process process, List<ParallelDefinition> definitions, List<DefinitionRef> drefs)
        {
            Process = process;
            Definitions = definitions;
            ContainedDefRefs = drefs;
        }

        public List<string> GetGlobalVariables()
        {
            List<string> Variables = new List<string>();

            foreach (ParallelDefinition definition in Definitions)
            {
                Common.Classes.Ultility.Ultility.AddList(Variables, definition.GetGlobalVariables());
            }

            Common.Classes.Ultility.Ultility.AddList(Variables, Process.GetGlobalVariables());

            foreach (ParallelDefinition definition in Definitions)
            {
                Variables.Remove(definition.Parameter);
            }

            return Variables;
        }
#if BDD
        public int IsBDDEncodable(List<string> calledProcesses)
        {
            for (int i = 0; i < Definitions.Count; i++)
            {
                if (Definitions[i].HasExternalLibraryAccess())
                {
                    return Constants.BDD_NON_ENCODABLE_0;
                }  
            }

            List<Process> newnewListProcess = GetIndexedProcesses(new Dictionary<string, Expression>());

            return (newnewListProcess.Count == 0) ? Constants.BDD_NON_ENCODABLE_0 : newnewListProcess[0].IsBDDEncodable(calledProcesses);
        }

        public void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            this.Process.CollectEvent(allEvents, calledProcesses);
        }
#endif

        public void GlobalVariablesAsIndex(List<string> visitedDef)
        {
            foreach (ParallelDefinition definition in Definitions)
            {
                if(definition.GetGlobalVariables().Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("ERROR - PAT FAILED to calculate the alphabet of process " + this.ToString() + ".");
                    sb.AppendLine("CAUSE - The process definition contains global variables!");
                    sb.AppendLine("REMEDY - 1) Avoid using global variables in process 2) Or manually specify the alphabet of the relevant process using the following syntax: \n\r\t #alphabet someProcess {X}; \n\rwhere X is a set of event names.");
                    throw new RuntimeException(sb.ToString());
                }
            }
        }

        public List<Process> GetIndexedProcesses(Dictionary<string, Expression> constMapping)
        {
            List<ParallelDefinition> newDefinitions = new List<ParallelDefinition>();
            foreach (ParallelDefinition definition in Definitions)
            {
                ParallelDefinition newPD = definition.ClearConstant(constMapping);
                newDefinitions.Add(newPD);
            }

            List<Process> processes = new List<Process>(16);

            foreach (ParallelDefinition pd in newDefinitions)
            {
                pd.DomainValues.Sort();
            }

            List<List<Expression>> list = new List<List<Expression>>();
            foreach (int v in newDefinitions[0].DomainValues)
            {
                List<Expression> l = new List<Expression>(newDefinitions.Count);
                l.Add(new IntConstant(v));
                list.Add(l);
            }

            for (int i = 1; i < newDefinitions.Count; i++)
            {
                List<List<Expression>> newList = new List<List<Expression>>();
                List<int> domain = newDefinitions[i].DomainValues;

                for (int j = 0; j < list.Count; j++)
                {
                    foreach (int i1 in domain)
                    {
                        List<Expression> cList = new List<Expression>(list[j]);
                        cList.Add(new IntConstant(i1));
                        newList.Add(cList);
                    }
                }
                list = newList;
            }

            foreach (List<Expression> constants in list)
            {
                Dictionary<string, Expression> constMappingNew = new Dictionary<string, Expression>(constMapping);
                //Dictionary<string, Expression> constMappingNew = new Dictionary<string, Expression>();
                for (int i = 0; i < constants.Count; i++)
                {
                    Expression constant = constants[i];

                    if (constMappingNew.ContainsKey(newDefinitions[i].Parameter))
                    {
                        constMappingNew[newDefinitions[i].Parameter] = constant;
                    }
                    else
                    {
                        constMappingNew.Add(newDefinitions[i].Parameter, constant);
                    }
                }

                Process newProcess = Process.ClearConstant(constMappingNew);
                processes.Add(newProcess);
            }

            return processes;
        }


        public List<DefinitionRef> GetIndexedDefRef(Dictionary<string, Expression> constMapping)
        {
            if(ContainedDefRefs == null)
            {
                return null;
            }

            List<ParallelDefinition> newDefinitions = new List<ParallelDefinition>();
            foreach (ParallelDefinition definition in Definitions)
            {
                ParallelDefinition newPD = definition.ClearConstant(constMapping);
                newDefinitions.Add(newPD);
            }

            List<DefinitionRef> processes = new List<DefinitionRef>(16);

            foreach (ParallelDefinition pd in newDefinitions)
            {
                pd.DomainValues.Sort();
            }

            List<List<Expression>> list = new List<List<Expression>>();
            foreach (int v in newDefinitions[0].DomainValues)
            {
                List<Expression> l = new List<Expression>(newDefinitions.Count);
                l.Add(new IntConstant(v));
                list.Add(l);
            }

            for (int i = 1; i < newDefinitions.Count; i++)
            {
                List<List<Expression>> newList = new List<List<Expression>>();
                List<int> domain = newDefinitions[i].DomainValues;

                for (int j = 0; j < list.Count; j++)
                {
                    foreach (int i1 in domain)
                    {
                        List<Expression> cList = new List<Expression>(list[j]);
                        cList.Add(new IntConstant(i1));
                        newList.Add(cList);
                    }
                }
                list = newList;
            }

            foreach (List<Expression> constants in list)
            {
                //Dictionary<string, Expression> constMappingNew = new Dictionary<string, Expression>(constMapping);
                Dictionary<string, Expression> constMappingNew = new Dictionary<string, Expression>();
                for (int i = 0; i < constants.Count; i++)
                {
                    Expression constant = constants[i];
                    constMappingNew.Add(newDefinitions[i].Parameter, constant);
                }

                foreach (DefinitionRef containedDefRef in ContainedDefRefs)
                {
                    DefinitionRef newProcess = containedDefRef.ClearConstant(constMappingNew) as DefinitionRef;
                    processes.Add(newProcess);
                }

            }

            return processes;
        }

        public IndexedProcess ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Process newProcess = Process.ClearConstant(constMapping);

            List<ParallelDefinition> newDefinitions = new List<ParallelDefinition>(Definitions.Count);

            foreach (ParallelDefinition definition in Definitions)
            {
                newDefinitions.Add(definition.ClearConstant(constMapping));
            }

            if (ContainedDefRefs != null)
            {
                List<DefinitionRef> newContainedDefRefs = new List<DefinitionRef>(ContainedDefRefs.Count);
                foreach (DefinitionRef definition in ContainedDefRefs)
                {
                    newContainedDefRefs.Add(definition.ClearConstant(constMapping) as DefinitionRef);
                }
                return new IndexedProcess(newProcess, newDefinitions, newContainedDefRefs);
            }
            else
            {
                return new IndexedProcess(newProcess, newDefinitions, ContainedDefRefs);
            }
        }

        public override string ToString()
        {
            string returnString = "";
            foreach (ParallelDefinition list in Definitions)
            {
                returnString += list.ToString() + ";";
            }

            return returnString.TrimEnd(';') + "@" + Process;
        }
    }
}
