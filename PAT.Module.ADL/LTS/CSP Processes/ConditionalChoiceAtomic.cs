using System;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.LTS;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class ConditionalChoiceAtomic : Process
    {
        public Process FirstProcess;
        public Process SecondProcess;
        public Expression ConditionalExpression;

        public ConditionalChoiceAtomic(Process firstProcess, Process secondProcess, Expression conditionExpression)
        {
            FirstProcess = firstProcess;
            SecondProcess = secondProcess;
            ConditionalExpression = conditionExpression;

            ProcessID = DataStore.DataManager.InitializeProcessID(FirstProcess.ProcessID + Constants.CONDITIONAL_CHOICE +
                                                                      conditionExpression.ExpressionID + Constants.CONDITIONAL_CHOICE +
                                                                      SecondProcess.ProcessID);
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            ExpressionValue v = EvaluatorDenotational.Evaluate(ConditionalExpression, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                FirstProcess.MoveOneStep(GlobalEnv, list);
            }
            else
            {
                SecondProcess.MoveOneStep(GlobalEnv, list);    
            }            
        }


        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ExpressionValue v = EvaluatorDenotational.Evaluate(ConditionalExpression, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                FirstProcess.SyncOutput(GlobalEnv, list);
            }
            else
            {
                SecondProcess.SyncOutput(GlobalEnv, list);    
            }
            
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            ExpressionValue v = EvaluatorDenotational.Evaluate(ConditionalExpression, eStep.GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                FirstProcess.SyncInput(eStep, list);
            }
            else
            {
                SecondProcess.SyncInput(eStep, list);    
            }            
        }

        public override string ToString()
        {
            return "ifa " + ConditionalExpression + " {" + FirstProcess.ToString() + "} else {" + SecondProcess.ToString() + "}";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            HashSet<string> list = SecondProcess.GetAlphabets(visitedDefinitionRefs);
            list.UnionWith(FirstProcess.GetAlphabets(visitedDefinitionRefs));
            return list;
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = SecondProcess.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList(Variables, FirstProcess.GetGlobalVariables());
            Common.Classes.Ultility.Ultility.AddList(Variables, ConditionalExpression.GetVars());
            return Variables;
        }

        public override List<string> GetChannels()
        {
            List<string> channels = SecondProcess.GetChannels();
            Common.Classes.Ultility.Ultility.AddList(channels, FirstProcess.GetChannels());

            return channels;
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression newCon = ConditionalExpression.ClearConstant(constMapping);

            Process newFirstProc = FirstProcess.ClearConstant(constMapping);
            Process newSecondProc = SecondProcess.ClearConstant(constMapping);

            return new ConditionalChoiceAtomic(newFirstProc, newSecondProc, newCon);
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression newCon = ConditionalExpression.ClearConstant(constMapping);

            Process newFirstProc = FirstProcess.Rename(constMapping, newDefNames, renamedProcesses);
            Process newSecondProc = SecondProcess.Rename(constMapping, newDefNames, renamedProcesses);

            Process result = new ConditionalChoiceAtomic(newFirstProc, newSecondProc, newCon);
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }

        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            if (ConditionalExpression.HasExternalLibraryCall())
            {
                return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
            }

            return (IsBDDEncodableProp = Math.Min(FirstProcess.IsBDDEncodable(calledProcesses), SecondProcess.IsBDDEncodable(calledProcesses)));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            List<Expression> guardsTemp = new List<Expression>();
            List<Event> eventsTemp = new List<Event>();
            List<Expression> programBlocksTemp = new List<Expression>();
            List<Process> processesTemp = new List<Process>();

            FirstProcess.MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);
            for (int i = 0; i < eventsTemp.Count; i++ )
            {
                guards.Add(Expression.CombineGuard(guardsTemp[i], ConditionalExpression));
                events.Add(eventsTemp[i]);
                programBlocks.Add(programBlocksTemp[i]);
                processes.Add(processesTemp[i]);
            }

            guardsTemp = new List<Expression>();
            eventsTemp = new List<Event>();
            programBlocksTemp = new List<Expression>();
            processesTemp = new List<Process>();

            SecondProcess.MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);
            for (int i = 0; i < eventsTemp.Count; i++)
            {
                guards.Add(Expression.CombineGuard(guardsTemp[i], Expression.NOT(ConditionalExpression)));
                events.Add(eventsTemp[i]);
                programBlocks.Add(programBlocksTemp[i]);
                processes.Add(processesTemp[i]);
            }
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD process1BDD = this.FirstProcess.Encode(encoder);
            AutomataBDD process2BDD = this.SecondProcess.Encode(encoder);

            //
            AutomataBDD ifBDD = AutomataBDD.Guard(this.ConditionalExpression, process1BDD, encoder.model);
            AutomataBDD elseBDD = AutomataBDD.Guard(Expression.NOT(this.ConditionalExpression), process2BDD, encoder.model);

            //
            return AutomataBDD.Choice(new List<AutomataBDD> { ifBDD, elseBDD }, encoder.model);

        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            this.FirstProcess.CollectEvent(allEvents, calledProcesses);
            this.SecondProcess.CollectEvent(allEvents, calledProcesses);
        }
#endif
        public override bool MustBeAbstracted()
        {
            return FirstProcess.MustBeAbstracted() || SecondProcess.MustBeAbstracted();
        }


    }
}
