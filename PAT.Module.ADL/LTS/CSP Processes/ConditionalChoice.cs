using System;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class ConditionalChoice : Process
    {
        public Process FirstProcess;
        public Process SecondProcess;
        public Expression ConditionalExpression;

        public ConditionalChoice(Process firstProcess, Process secondProcess, Expression conditionExpression)
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
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ExpressionValue v = EvaluatorDenotational.Evaluate(ConditionalExpression, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                list.Add(new Configuration(FirstProcess, Constants.TAU, "[if(" + ConditionalExpression + ")]", GlobalEnv, false));                
            }
            else
            {
                list.Add(new Configuration(SecondProcess, Constants.TAU, "[if!(" + ConditionalExpression + ")]", GlobalEnv, false));
            }
        }

        public override string ToString()
        {
            if(SecondProcess is Skip)
                return "if " + ConditionalExpression + " {\r\n" + FirstProcess.ToString() + "\r\n}";

            return "if " + ConditionalExpression + " {\r\n" + FirstProcess.ToString() + "\r\n} else {\r\n" + SecondProcess.ToString() + "\r\n}";
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

            return new ConditionalChoice(newFirstProc, newSecondProc, newCon);
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression newCon = ConditionalExpression.ClearConstant(constMapping);
            Process newFirstProc = FirstProcess.Rename(constMapping, newDefNames, renamedProcesses);
            Process newSecondProc = SecondProcess.Rename(constMapping, newDefNames, renamedProcesses);

            Process result = new ConditionalChoice(newFirstProc, newSecondProc, newCon);
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
            guards.Add(ConditionalExpression);
            events.Add(new Event(Constants.TAU));
            programBlocks.Add(null);
            processes.Add(FirstProcess);

            guards.Add(Expression.NOT(ConditionalExpression));
            events.Add(new Event(Constants.TAU));
            programBlocks.Add(null);
            processes.Add(SecondProcess);
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD process1BDD = this.FirstProcess.Encode(encoder);
            AutomataBDD process2BDD = this.SecondProcess.Encode(encoder);

            Expression eventUpdateExpression = encoder.GetEventExpression(new Event(Constants.TAU));
            //
            AutomataBDD ifBDD = AutomataBDD.EventPrefix(this.ConditionalExpression, eventUpdateExpression, process1BDD, encoder.model);
            AutomataBDD elseBDD = AutomataBDD.EventPrefix(Expression.NOT(this.ConditionalExpression), eventUpdateExpression, process2BDD, encoder.model);

            //
            return AutomataBDD.Choice(new List<AutomataBDD> { ifBDD, elseBDD }, encoder.model);

        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            if (!allEvents.Contains(Constants.TAU))
            {
                allEvents.Add(Constants.TAU);
            }

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