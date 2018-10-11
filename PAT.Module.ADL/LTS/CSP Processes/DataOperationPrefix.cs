using System.Collections.Generic;
using PAT.Common.Classes.DataStructure;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;
using SequenceExpression = PAT.Common.Classes.Expressions.ExpressionClass.Sequence;

namespace PAT.ADL.LTS
{
    public sealed class DataOperationPrefix : Process
    {
        public Event Event;
        public Process Process;
        public Expression AssignmentExpr;
        public bool HasLocalVar;

        public DataOperationPrefix(Event e, Expression assignment, Process process, bool localvar) 
        {
            Event = e;
            AssignmentExpr = assignment;
            Process = process;
            HasLocalVar = localvar;

            ProcessID = DataStore.DataManager.InitializeProcessID(Event.GetID() + "{" + AssignmentExpr.ExpressionID + "}" + Constants.EVENTPREFIX + Process.ProcessID);              
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            string name = Event.GetEventName(GlobalEnv);

            Valuation newGlobleEnv = GlobalEnv.GetVariableClone();
       
            EvaluatorDenotational.Evaluate(AssignmentExpr, newGlobleEnv);

            //if (LocalVariables != null)
            if (HasLocalVar)
            {
                Valuation tempEnv = GlobalEnv.GetVariableClone();
                for (int i = 0; i < tempEnv.Variables._entries.Length; i++)
                {
                    StringDictionaryEntryWithKey<ExpressionValue> pair = tempEnv.Variables._entries[i];
                    if (pair != null)
                    {
                        pair.Value = newGlobleEnv.Variables[pair.Key];
                    }
                }

                newGlobleEnv = tempEnv;
            }

            string ID = Event.GetEventID(GlobalEnv);

            if (ID != name)
            {
                list.Add(new Configuration(Process, ID, name, newGlobleEnv, true));
            }
            else
            {
                list.Add(new Configuration(Process, ID, null, newGlobleEnv, true));
            }
            //return list;
        }
    
        public override string ToString()
        {
            if (Event.ToString() == Constants.TAU)
            {
                return "{" + AssignmentExpr + "}->" + Process;                
            }

            return Event + "{" + AssignmentExpr + "}->" + Process;
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            if(Specification.CollectDataOperationEvent == true && Event.ToString() != Constants.TAU)
            {
                HashSet<string> set = Process.GetAlphabets(visitedDefinitionRefs);
                set.Add(Event.BaseName);

                return set;

            }
            else
            {
                return Process.GetAlphabets(visitedDefinitionRefs);    
            }            
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = Process.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList(Variables, AssignmentExpr.GetVars());

            if (Event.ExpressionList != null)
            {
                foreach (Expression expression in Event.ExpressionList)
                {
                    Common.Classes.Ultility.Ultility.AddList(Variables, expression.GetVars());
                }
            }

            return Variables;
        }

        public override List<string> GetChannels()
        {
            return Process.GetChannels();
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression newAssign = AssignmentExpr.ClearConstant(constMapping);
            return new DataOperationPrefix(Event.ClearConstant(constMapping), newAssign, Process.ClearConstant(constMapping), HasLocalVar);
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression newAssign = AssignmentExpr.ClearConstant(constMapping);
            Process result = new DataOperationPrefix(Event.ClearConstant(constMapping), newAssign, Process.Rename(constMapping, newDefNames, renamedProcesses), HasLocalVar);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }

        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            if (AssignmentExpr.HasExternalLibraryCall())
            {
                return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
            }

            return (IsBDDEncodableProp = Process.IsBDDEncodable(calledProcesses));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            guards.Add(null);
            events.Add(Event);
            programBlocks.Add(AssignmentExpr);
            processes.Add(Process);
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD processAutomataBDD = this.Process.Encode(encoder);

            Expression eventUpdateExpression = encoder.GetEventExpression(this.Event);

            //
            return AutomataBDD.EventPrefix(new BoolConstant(true), new SequenceExpression(eventUpdateExpression, this.AssignmentExpr), processAutomataBDD, encoder.model);
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            int paraLength = 0;
            if (this.Event.ExpressionList != null)
            {
                paraLength = this.Event.ExpressionList.Length;
            }
            else if (this.Event.EventID != null && this.Event.EventID != this.Event.BaseName)
            {
                paraLength = this.Event.EventID.Split('.').Length - 1;
            }

            string eventName = this.Event.BaseName + Model.NAME_SEPERATOR + paraLength;

            if (!allEvents.Contains(eventName))
            {
                allEvents.Add(eventName);
            }
            this.Process.CollectEvent(allEvents, calledProcesses);
        }
#endif

        public override bool MustBeAbstracted()
        {
            return Process.MustBeAbstracted();
        }


    }
}