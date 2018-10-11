using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using BDDExpression = PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using SequenceExpression = PAT.Common.Classes.Expressions.ExpressionClass.Sequence;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class GuardProcess : Process
    {
        public Process Process;
        public Expression Condition;

        public GuardProcess(Process process, Expression cond)
        {
            Process = process;
            Condition = cond;
            ProcessID = DataStore.DataManager.InitializeProcessID("[" + cond.ExpressionID + "]" + Process.ProcessID);
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = Process.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList(Variables, Condition.GetVars());
            return Variables;
        }

        public override List<string> GetChannels()
        {
            return Process.GetChannels();
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ExpressionValue v = EvaluatorDenotational.Evaluate(Condition, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                Process.MoveOneStep(GlobalEnv, list);
            }

            //return new List<Configuration>(0);
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            ExpressionValue v = EvaluatorDenotational.Evaluate(Condition, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                 Process.SyncOutput(GlobalEnv, list);                
            }

            //return new List<ConfigurationWithChannelData>(0);
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            ExpressionValue v = EvaluatorDenotational.Evaluate(Condition, eStep.GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                Process.SyncInput(eStep, list);
            }

            //return new List<Configuration>(0);
        }

        public override string ToString()
        {
            return "([" + Condition + "]" + Process.ToString() + ")";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            return Process.GetAlphabets(visitedDefinitionRefs);
        }

//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            //return GetProcess(eStep.GlobalEnv).GetActiveProcesses(eStep);
//            ExpressionValue v = EvaluatorDenotational.Evaluate(Condition, GlobalEnv);

//            if ((v as BoolConstant).Value)
//            {
//                return Process.GetActiveProcesses(GlobalEnv);
//            }

//            return new List<string>(0);
//        }
//#endif

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression newCon = Condition.ClearConstant(constMapping);
            return new GuardProcess(Process.ClearConstant(constMapping), newCon);
        }

#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression newCon = Condition.ClearConstant(constMapping);
            Process result = new GuardProcess(Process.Rename(constMapping, newDefNames, renamedProcesses), newCon);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }

        
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            if (Condition.HasExternalLibraryCall())
            {
                return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
            }

            return (IsBDDEncodableProp = Process.IsBDDEncodable(calledProcesses));
        }


        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            List<Expression> guardsTemp = new List<Expression>();
            List<Event> eventsTemp = new List<Event>();
            List<Expression> programBlocksTemp = new List<Expression>();
            List<Process> processesTemp = new List<Process>();

            Process.MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);

            for (int i = 0; i < eventsTemp.Count; i++)
            {
                guardsTemp[i] = Expression.CombineGuard(guardsTemp[i], Condition);
            }

            guards.AddRange(guardsTemp);
            events.AddRange(eventsTemp);
            programBlocks.AddRange(programBlocksTemp);
            processes.AddRange(processesTemp);
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD processAutomataBDD = this.Process.Encode(encoder);

            //
            return AutomataBDD.Guard(this.Condition, processAutomataBDD, encoder.model);
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            this.Process.CollectEvent(allEvents, calledProcesses);
        }
#endif
        public override bool MustBeAbstracted()
        {
            return Process.MustBeAbstracted();
        }

    }
}