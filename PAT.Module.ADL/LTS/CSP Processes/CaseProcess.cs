using System;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using System.Text;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using BDDExpression = PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class CaseProcess : Process
    {
        public Process[] Processes;
        public Expression[] Conditions;

        public CaseProcess(Process[] processes, Expression[] conds)
        {
            Processes = processes;
            Conditions = conds;

            //generate the process ID
            StringBuilder IDBuilder = new StringBuilder(Constants.CASE);
            for (int i = 0; i < Processes.Length; i++)
            {
                IDBuilder.Append(Conditions[i].ExpressionID);
                IDBuilder.Append(Constants.CASECONDITIONAL);
                IDBuilder.Append(Processes[i].ProcessID);
                IDBuilder.Append(";");
            }

            ProcessID = DataStore.DataManager.InitializeProcessID(IDBuilder.ToString());
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = new List<string>();

            for (int i = 0; i < Conditions.Length; i++)
            {
                Common.Classes.Ultility.Ultility.AddList(Variables, Processes[i].GetGlobalVariables());
                Common.Classes.Ultility.Ultility.AddList(Variables, Conditions[i].GetVars());
            }

            return Variables;
        }

        public override List<string> GetChannels()
        {
            List<string> channels = new List<string>();

            for (int i = 0; i < Conditions.Length; i++)
            {
                List<string> vars = Processes[i].GetChannels();
                foreach (string var in vars)
                {
                    if (!channels.Contains(var))
                    {
                        channels.Add(var);
                    }
                }
            }

            return channels;
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            for (int i = 0; i < Processes.Length; i++)
            {
                Expression con = Conditions[i];
                ExpressionValue v = EvaluatorDenotational.Evaluate(con, GlobalEnv);

                if ((v as BoolConstant).Value)
                {
                    list.Add(new Configuration(Processes[i], Constants.TAU, "[" + con + "]", GlobalEnv, false));
                    return;
                }
            }

            //if there is no condition is true, return a skip action
            list.Add(new Configuration(new Skip(), Constants.TAU, "[default]", GlobalEnv, false));
        }

        public override string ToString()
        {
            if (Processes.Length == 1)
            {
                return "if" + Conditions[0] + "{" + Processes[0] + "}";
            }

            StringBuilder s = new StringBuilder();
            s.AppendLine("case {");
            for (int i = 0; i < Processes.Length; i++)
            {
                Expression con = Conditions[i];
                Process process = Processes[i];
                s.AppendLine(con + ":" + process);
            }
            s.AppendLine("}");
            return s.ToString();
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            HashSet<string> toReturn = new HashSet<string>();

            for (int i = 0; i < Processes.Length; i++)
            {
                Process process = Processes[i];
                toReturn.UnionWith(process.GetAlphabets(visitedDefinitionRefs));
            }

            return toReturn;
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            List<Process> newProcesses = new List<Process>(Processes.Length);
            List<Expression> newConditions = new List<Expression>(Processes.Length);

            for (int i = 0; i < Processes.Length; i++)
            {
                Expression newCon = Conditions[i].ClearConstant(constMapping);
                newConditions.Add(newCon);
                Process newProc = Processes[i].ClearConstant(constMapping);
                newProcesses.Add(newProc);
            }

            return new CaseProcess(newProcesses.ToArray(), newConditions.ToArray());
        }
#if BDD
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            int min = Constants.BDD_LTS_COMPOSITION_3;
            for (int i = 0; i < Processes.Length; i++)
            {
                if (Conditions[i].HasExternalLibraryCall())
                {
                    return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0); 
                }

                min = Math.Min(min, Processes[i].IsBDDEncodable(calledProcesses));
            }

            return (IsBDDEncodableProp = min);
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            Expression allCondition = new BoolConstant(false);
            for (int i = 0; i < Processes.Length; i++)
            {
                guards.Add(Expression.AND(Expression.NOT(allCondition), Conditions[i]));
                events.Add(new Event(Constants.TAU));
                programBlocks.Add(null);
                processes.Add(Processes[i]);

                if (i == 0)
                {
                    allCondition = Conditions[0];
                }
                else
                {
                    allCondition = Expression.OR(allCondition, Conditions[i]);
                }
            }

            guards.Add(Expression.NOT(allCondition));
            events.Add(new Event(Constants.TAU));
            programBlocks.Add(null);
            processes.Add(new Skip());
        }

        /// <summary>
        /// P = [] {[guardi] tau -> Pi}
        /// </summary>
        /// <param name="encoder"></param>
        /// <returns></returns>
        public override AutomataBDD  EncodeComposition(BDDEncoder encoder)
        {
            List<AutomataBDD> processAutomataBDDs = new List<AutomataBDD>();

            Expression previousguardsOR = null;
            Expression eventUpdateExpression = encoder.GetEventExpression(new Event(Constants.TAU));
            for (int i = 0; i < Processes.Length; i++)
            {
                Expression guard = null;

                if (i == 0)
                {
                    guard = Conditions[i];
                }
                else //if (previousguardsOR != null)
                {
                    guard = Expression.AND(guard, Expression.NOT(previousguardsOR));
                }

                AutomataBDD processAutomataBDd = Processes[i].Encode(encoder);

                AutomataBDD guardProcessAutomataBDD = AutomataBDD.EventPrefix(guard, eventUpdateExpression, processAutomataBDd, encoder.model);
                processAutomataBDDs.Add(guardProcessAutomataBDD);
                previousguardsOR = Expression.OR(previousguardsOR, guard);
            }

            //if all conditions are not satisfied, it becomes Skip
            Expression allConditions = new BoolConstant(true);
            foreach (var condition in Conditions)
            {
                allConditions = Expression.OR(allConditions, condition);
            }

            processAutomataBDDs.Add(AutomataBDD.EventPrefix(Expression.NOT(allConditions), eventUpdateExpression, new Skip().Encode(encoder), encoder.model));

            return AutomataBDD.Choice(processAutomataBDDs, encoder.model);
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            if (!allEvents.Contains(Constants.TAU))
            {
                allEvents.Add(Constants.TAU);
            }

            foreach (Process process in this.Processes)
            {
                process.CollectEvent(allEvents, calledProcesses);
            }
        }

        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            List<Process> newProcesses = new List<Process>(Processes.Length);
            List<Expression> newConditions = new List<Expression>(Processes.Length);

            for (int i = 0; i < Processes.Length; i++)
            {
                Expression newCon = Conditions[i].ClearConstant(constMapping);
                newConditions.Add(newCon);
                Process newProc = Processes[i].Rename(constMapping, newDefNames, renamedProcesses);
                newProcesses.Add(newProc);
            }

            Process result = new CaseProcess(newProcesses.ToArray(), newConditions.ToArray());
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }
#endif
        public override bool MustBeAbstracted()
        {
            for (int i = 0; i < Processes.Length; i++)
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