using System;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.LTS;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class Interrupt : Process
    {
        public Process FirstProcess;
        public Process SecondProcess; 

        public Interrupt(Process firstProcess, Process secondProcess)
        {
            FirstProcess = firstProcess;
            SecondProcess = secondProcess;
            ProcessID = DataStore.DataManager.InitializeProcessID(FirstProcess.ProcessID + Constants.INTERRUPT + SecondProcess.ProcessID);
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            FirstProcess.MoveOneStep(GlobalEnv, list);

            for (int i = 0; i < list.Count; i++)
            {
                Configuration step = list[i];
                if (step.Event != Constants.TERMINATION)
                {
                    Interrupt inter = new Interrupt(step.Process, SecondProcess);
                    step.Process = inter;
                }      
            }

            List<Configuration> list2 = new List<Configuration>();
            SecondProcess.MoveOneStep(GlobalEnv, list2);
            for (int i = 0; i < list2.Count; i++)
            {
                Configuration step = list2[i];
                if (step.Event == Constants.TAU)
                {
                    Interrupt inter = new Interrupt(FirstProcess, step.Process);

                    step.Process = inter;
                }

                //if(step.Event == Constants.TAU)
                //{
                //    throw new RuntimeException("Tau event cannot be an interrupting event in process (" + this.ToString() + ")!");
                //}

                list.Add(step);
            }
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            FirstProcess.SyncOutput(GlobalEnv, list);

            for (int i = 0; i < list.Count; i++)
            {
                Configuration step = list[i];
                if (step.Event != Constants.TERMINATION)
                {
                    Interrupt inter = new Interrupt(step.Process, SecondProcess);
                    step.Process = inter;
                } 
            }

            SecondProcess.SyncOutput(GlobalEnv, list);
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            FirstProcess.SyncInput(eStep, list);

            for (int i = 0; i < list.Count; i++)
            {
                Configuration step = list[i];

                if (step.Event != Constants.TERMINATION)
                {
                    Interrupt inter = new Interrupt(step.Process, SecondProcess);
                    step.Process = inter;
                }
                list[i] = step;
            }

            SecondProcess.SyncInput(eStep, list);
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> Variables = FirstProcess.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList<string>(Variables, SecondProcess.GetGlobalVariables());

            return Variables;
        }

        public override List<string> GetChannels()
        {
            List<string> channel = FirstProcess.GetChannels();
            Common.Classes.Ultility.Ultility.AddList<string>(channel, SecondProcess.GetChannels());

            return channel;
        }

        public override string ToString()
        {
            return "(" + FirstProcess.ToString() + " interrupt " + SecondProcess.ToString() + ")";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            HashSet<string> list = SecondProcess.GetAlphabets(visitedDefinitionRefs);
            list.UnionWith(FirstProcess.GetAlphabets(visitedDefinitionRefs));
            return list;
        }

//#if XZC
//        public override List<string> GetActiveProcesses(Valuation GlobalEnv)
//        {
//            List<string> list = new List<string>(SecondProcess.GetActiveProcesses(GlobalEnv));
//            Common.Classes.Ultility.Ultility.AddList(list, FirstProcess.GetActiveProcesses(GlobalEnv));
//            return list;
//        }
//#endif

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            return new Interrupt(FirstProcess.ClearConstant(constMapping), SecondProcess.ClearConstant(constMapping));
        }
#if BDD
        public override CSPProcess Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            CSPProcess result = new Interrupt(FirstProcess.Rename(constMapping, newDefNames, renamedProcesses), SecondProcess.Rename(constMapping, newDefNames, renamedProcesses));
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }


        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            return (IsBDDEncodableProp = Math.Min(FirstProcess.IsBDDEncodable(calledProcesses), SecondProcess.IsBDDEncodable(calledProcesses)));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<CSPProcess> processes, Model model, SymbolicLTS lts)
        {
            List<Expression> guardsTemp = new List<Expression>();
            List<Event> eventsTemp = new List<Event>();
            List<Expression> programBlocksTemp = new List<Expression>();
            List<CSPProcess> processesTemp = new List<CSPProcess>();
            
            FirstProcess.MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);

            for(int i = 0; i < eventsTemp.Count; i++)
            {
                if(eventsTemp[i].BaseName != Constants.TERMINATION)
                {
                    guards.Add(guardsTemp[i]);
                    events.Add(eventsTemp[i]);
                    programBlocks.Add(programBlocksTemp[i]);
                    processes.Add(new Interrupt(processesTemp[i], SecondProcess));
                }
                else
                {
                    guards.Add(guardsTemp[i]);
                    events.Add(eventsTemp[i]);
                    programBlocks.Add(programBlocksTemp[i]);
                    processes.Add(processesTemp[i]);
                }
            }

            guardsTemp = new List<Expression>();
            eventsTemp = new List<Event>();
            programBlocksTemp = new List<Expression>();
            processesTemp = new List<CSPProcess>();

            SecondProcess.MakeDiscreteTransition(guardsTemp, eventsTemp, programBlocksTemp, processesTemp, model, lts);

            for (int i = 0; i < eventsTemp.Count; i++)
            {
                if (eventsTemp[i].BaseName != Constants.TAU)
                {
                    guards.Add(guardsTemp[i]);
                    events.Add(eventsTemp[i]);
                    programBlocks.Add(programBlocksTemp[i]);
                    processes.Add(processesTemp[i]);
                }
                else
                {
                    guards.Add(guardsTemp[i]);
                    events.Add(eventsTemp[i]);
                    programBlocks.Add(programBlocksTemp[i]);
                    processes.Add(new Interrupt(FirstProcess, processesTemp[i]));
                }
            }

        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD process1BDD = this.FirstProcess.Encode(encoder);
            AutomataBDD process2BDD = this.SecondProcess.Encode(encoder);

            //
            return AutomataBDD.Interrupt(process1BDD, process2BDD, encoder.model);
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