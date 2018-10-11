using System;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class Sequence : Process
    {
        public Process FirstProcess;
        public Process SecondProcess;


        public Sequence(Process firstProcess, Process secondProcess)
        {
            FirstProcess = firstProcess;
            SecondProcess = secondProcess;

            if (firstProcess.IsSkip()) 
            //if (FirstProcess is Skip || (FirstProcess is AtomicProcess && (FirstProcess as AtomicProcess).Process is Skip))
            {
                ProcessID = SecondProcess.ProcessID;
            }
            else if (FirstProcess is Stop || (FirstProcess is AtomicProcess && (FirstProcess as AtomicProcess).Process is Stop))
            {
                ProcessID = Constants.STOP;
            }
            else
            {
                ProcessID = DataStore.DataManager.InitializeProcessID(Constants.SEQUENTIAL + FirstProcess.ProcessID + Constants.SEPARATOR + SecondProcess.ProcessID);
            }
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            if (FirstProcess.IsSkip())
            {
                SecondProcess.MoveOneStep(GlobalEnv, list);
                return;
            }

            FirstProcess.MoveOneStep(GlobalEnv, list);
            for (int i = 0; i < list.Count; i++)
            {
                Configuration step = list[i];
                if (step.Event == Constants.TERMINATION)
                {
                    step.Event = Constants.TAU;
                    step.Process = SecondProcess;
                }
                else
                {
                    Sequence p = new Sequence(step.Process, this.SecondProcess);
                    step.Process = p;
                }
                list[i] = step;
            }
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            if (FirstProcess.IsSkip())
            {
                SecondProcess.SyncOutput(GlobalEnv, list);
                return;
            }

            FirstProcess.SyncOutput(GlobalEnv, list);

            for (int i = 0; i < list.Count; i++)
            {
                Configuration step = list[i];
                step.Process = new Sequence(step.Process, SecondProcess);
            }

            //return list;
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            if (FirstProcess.IsSkip())
            {
                SecondProcess.SyncInput(eStep, list);
                return;
            }

            FirstProcess.SyncInput(eStep, list);
            
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Process = new Sequence(list[i].Process, SecondProcess);
            }
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = FirstProcess.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList<string>(Variables, SecondProcess.GetGlobalVariables());

            return Variables;
        }

        public override List<string> GetChannels()
        {
            List<string> channels = FirstProcess.GetChannels();
            Common.Classes.Ultility.Ultility.AddList<string>(channels, SecondProcess.GetChannels());
            return channels;
        }

        public override string ToString()
        {
            return FirstProcess.ToString() + ";\r\n" + SecondProcess.ToString();
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            if (FirstProcess.IsSkip())
            {
                return SecondProcess.GetAlphabets(visitedDefinitionRefs);
            }

            HashSet<string> list = SecondProcess.GetAlphabets(visitedDefinitionRefs);
            list.UnionWith(FirstProcess.GetAlphabets(visitedDefinitionRefs));
            return list;
        }

//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            if (FirstProcess.IsSkip())
//            {
//                return SecondProcess.GetActiveProcesses(GlobalEnv);
//            }

//            return FirstProcess.GetActiveProcesses(GlobalEnv);
//        }
//#endif

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            if (FirstProcess.IsSkip())
            {
                return SecondProcess.ClearConstant(constMapping);
            }
            
            return new Sequence(FirstProcess.ClearConstant(constMapping), SecondProcess.ClearConstant(constMapping));
        }

#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            if (FirstProcess.IsSkip())
            {
                Process secondProcess = SecondProcess.Rename(constMapping, newDefNames, renamedProcesses);
                secondProcess.IsBDDEncodableProp = SecondProcess.IsBDDEncodableProp;
                return secondProcess;
            }

            Process result = new Sequence(FirstProcess.Rename(constMapping, newDefNames, renamedProcesses), SecondProcess.Rename(constMapping, newDefNames, renamedProcesses));
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }


        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            return (IsBDDEncodableProp = Math.Min(FirstProcess.IsBDDEncodable(calledProcesses), SecondProcess.IsBDDEncodable(calledProcesses)));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            List<Expression> guardsTemp = new List<Expression>();
            List<Event> eventsTemp = new List<Event>();
            List<Expression> programBlocksTemp = new List<Expression>();
            List<Process> processesTemp = new List<Process>();

            FirstProcess.MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);

            for (int i = 0; i < eventsTemp.Count; i++)
            {
                if (eventsTemp[i].BaseName != Constants.TERMINATION)
                {
                    guards.Add(guardsTemp[i]);
                    events.Add(eventsTemp[i]);
                    programBlocks.Add(programBlocksTemp[i]);
                    processes.Add(new Sequence(processesTemp[i], SecondProcess));
                }
                else
                {
                    guards.Add(guardsTemp[i]);
                    events.Add(new Event(Constants.TAU));
                    programBlocks.Add(programBlocksTemp[i]);
                    processes.Add(SecondProcess);
                }
            }
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD process1BDD = this.FirstProcess.Encode(encoder);
            AutomataBDD process2BDD = this.SecondProcess.Encode(encoder);

            return AutomataBDD.Sequence(process1BDD, process2BDD, encoder.model);
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

        public override bool IsSkip()
        {
            if(FirstProcess.IsSkip())
            {
                return SecondProcess.IsSkip();
            }

            return false;
        }

    }
}