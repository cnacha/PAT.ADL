using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class ConditionalChoiceBlocking : Process
    {
        public Process FirstProcess;
        public Expression ConditionalExpression;

        public ConditionalChoiceBlocking(Process firstProcess, Expression conditionExpression)
        {
            FirstProcess = firstProcess;
            ConditionalExpression = conditionExpression;

            ProcessID = DataStore.DataManager.InitializeProcessID(FirstProcess.ProcessID + Constants.CONDITIONAL_CHOICE + conditionExpression.ExpressionID);
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ExpressionValue v = EvaluatorDenotational.Evaluate(ConditionalExpression, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                list.Add(new Configuration(FirstProcess, Constants.TAU, "[ifb(" + ConditionalExpression + ")]", GlobalEnv, false));                
            }
        }
    
        public override string ToString()
        {
            return "ifb " + ConditionalExpression + " {" + FirstProcess.ToString() + "}";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            return FirstProcess.GetAlphabets(visitedDefinitionRefs);
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = FirstProcess.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList(Variables, ConditionalExpression.GetVars());

            return Variables;
        }

        public override List<string> GetChannels()
        {
            return FirstProcess.GetChannels();
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression newCon = ConditionalExpression.ClearConstant(constMapping);

            Process newFirstProc = FirstProcess.ClearConstant(constMapping);
            
            return new ConditionalChoiceBlocking(newFirstProc, newCon);
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression newCon = ConditionalExpression.ClearConstant(constMapping);

            Process newFirstProc = FirstProcess.Rename(constMapping, newDefNames, renamedProcesses);

            Process result = new ConditionalChoiceBlocking(newFirstProc, newCon);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }

        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            if (ConditionalExpression.HasExternalLibraryCall())
            {
                return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
            }

            return (IsBDDEncodableProp = FirstProcess.IsBDDEncodable(calledProcesses));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            guards.Add(ConditionalExpression);
            events.Add(new Event(Constants.TAU));
            programBlocks.Add(null);
            processes.Add(FirstProcess);
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD process1BDD = this.FirstProcess.Encode(encoder);

            Expression eventUpdateExpression = encoder.GetEventExpression(new Event(Constants.TAU));

            return AutomataBDD.EventPrefix(this.ConditionalExpression, eventUpdateExpression, process1BDD, encoder.model);
           
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            if (!allEvents.Contains(Constants.TAU))
            {
                allEvents.Add(Constants.TAU);
            }

            this.FirstProcess.CollectEvent(allEvents, calledProcesses);
        }
#endif
        public override bool MustBeAbstracted()
        {
            return FirstProcess.MustBeAbstracted();
        }


    }
}