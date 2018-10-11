using System;
using System.Text;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class ChannelOutput : Process
    {
        public string ChannelName;
        public Expression[] ExpressionList;
        public Process Process;
        public Expression ChannelIndex;


        public ChannelOutput(string evtName, Expression index, Expression[] e, Process process)
        {
            ChannelName = evtName;
            Process = process;
            ExpressionList = e;
            ChannelIndex = index;

            StringBuilder ID = new StringBuilder();
            ID.Append(ChannelName);
            if (ChannelIndex != null)
            {
                ID.Append(ChannelIndex);
            }
            ID.Append("!");
            ID.Append(Common.Classes.Ultility.Ultility.PPIDListDot(ExpressionList));
            ID.Append(Constants.EVENTPREFIX);
            ID.Append(Process.ProcessID);

            ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            Valuation globalEnv = GlobalEnv;
            ChannelQueue Buffer = null;

            string channelName = this.ChannelName;

            if (ChannelIndex != null)
            {
                int size = ((IntConstant) EvaluatorDenotational.Evaluate(ChannelIndex, GlobalEnv)).Value;
                if (size >= Specification.ChannelArrayDatabase[ChannelName])
                {
                    throw new PAT.Common.Classes.Expressions.ExpressionClass.IndexOutOfBoundsException("Channel index out of bounds for expression " + this.ToString());
                }
                channelName = channelName + "[" + size + "]";
            }

            if (globalEnv.Channels.TryGetValue(channelName, out Buffer))
            {
                if (Buffer.Count < Buffer.Size)
                {

                    ExpressionValue[] values = new ExpressionValue[ExpressionList.Length];
                    string eventName = channelName + "!";
                    string eventID = channelName + "!";

                    for (int i = 0; i < ExpressionList.Length; i++)
                    {
                        values[i] = EvaluatorDenotational.Evaluate(ExpressionList[i], globalEnv);
                        if (i == ExpressionList.Length - 1)
                        {
                            eventName += values[i].ToString();
                            eventID += values[i].ExpressionID;
                        }
                        else
                        {
                            eventName += values[i].ToString() + ".";
                            eventID += values[i].ExpressionID + ".";
                        }
                    }

                    ChannelQueue newBuffer = Buffer.Clone();
                    newBuffer.Enqueue(values);

                    globalEnv = globalEnv.GetChannelClone();
                    globalEnv.Channels[channelName] = newBuffer;

                    if (eventID != eventName)
                    {
                        list.Add(new Configuration(Process, eventID, eventName, globalEnv, false));
                    }
                    else
                    {
                        list.Add(new Configuration(Process, eventID, null, globalEnv, false));
                    }
                }
            }

           // return list;
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            //List<ConfigurationWithChannelData> list = new List<ConfigurationWithChannelData>(1);

            string channelName = this.ChannelName;

            if (ChannelIndex != null)
            {
                int size = ((IntConstant)EvaluatorDenotational.Evaluate(ChannelIndex, GlobalEnv)).Value;
                if (size >= Specification.ChannelArrayDatabase[ChannelName])
                {
                    throw new PAT.Common.Classes.Expressions.ExpressionClass.IndexOutOfBoundsException("Channel index out of bounds for expression " + this.ToString());
                }
                channelName = channelName + "[" + size + "]";
            }

            if (SpecificationBase.SyncrhonousChannelNames.Contains(channelName))
            {
                string eventName = channelName;
                string eventID = channelName;

                Expression[] newExpressionList = new Expression[ExpressionList.Length];

                for (int i = 0; i < ExpressionList.Length; i++)
                {
                    newExpressionList[i] = EvaluatorDenotational.Evaluate(ExpressionList[i], GlobalEnv);
                    eventName += "." + newExpressionList[i];
                    eventID += "." + newExpressionList[i].ExpressionID;
                }

                if(eventID != eventName)
                {
                    list.Add(new ConfigurationWithChannelData(Process, eventID, eventName, GlobalEnv, false, channelName, newExpressionList));
                }
                else
                {
                    list.Add(new ConfigurationWithChannelData(Process, eventID, null, GlobalEnv, false, channelName, newExpressionList));
                }
            }

            //return list;
        }


        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {         
            return Process.GetAlphabets(visitedDefinitionRefs);
        }

        public override List<string> GetGlobalVariables()
        {
            List<string> toReturn = Process.GetGlobalVariables();

            foreach (Expression expression in ExpressionList)
            {
                Common.Classes.Ultility.Ultility.AddList(toReturn, expression.GetVars());
            }

            if (ChannelIndex != null)
            {
                Common.Classes.Ultility.Ultility.AddList(toReturn, ChannelIndex.GetVars());
            }

            return toReturn;
        }

        public override List<string> GetChannels()
        {
            List<string> toReturn = Process.GetChannels();

            if (!toReturn.Contains(ChannelName))
            {
                toReturn.Add(ChannelName);
            }

            return toReturn;
        }

        public override string ToString()
        {
            if (ChannelIndex != null)
            {
                return ChannelName + "[" + ChannelIndex + "]" + "!" +
                       Common.Classes.Ultility.Ultility.PPStringListDot(ExpressionList).TrimStart('.') + "->" + Process;
            }
            else
            {
                return ChannelName + "!" + Common.Classes.Ultility.Ultility.PPStringListDot(ExpressionList).TrimStart('.') + "->" + Process;
 
            }
        }


        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression[] newExpression = new Expression[ExpressionList.Length];
            for (int i = 0; i < ExpressionList.Length; i++)
            {
                newExpression[i] = ExpressionList[i].ClearConstant(constMapping);
            }

            return new ChannelOutput(ChannelName, ChannelIndex != null? ChannelIndex.ClearConstant(constMapping) : ChannelIndex, newExpression, Process.ClearConstant(constMapping));
        }

#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression[] newExpression = new Expression[ExpressionList.Length];
            for (int i = 0; i < ExpressionList.Length; i++)
            {
                newExpression[i] = ExpressionList[i].ClearConstant(constMapping);
            }

            Process result = new ChannelOutput(ChannelName, ChannelIndex != null ? ChannelIndex.ClearConstant(constMapping) : ChannelIndex, newExpression, Process.Rename(constMapping, newDefNames, renamedProcesses));
            result.IsBDDEncodableProp = this.IsBDDEncodableProp;
            return result;
        }


        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            for (int i = 0; i < ExpressionList.Length; i++)
            {
                if (ExpressionList[i].HasExternalLibraryCall())
                {
                    return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
                }
            }

            if (ChannelIndex != null) // && ChannelIndex.HasExternalLibraryCall()
            {
                return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
            }

            return (IsBDDEncodableProp = Process.IsBDDEncodable(calledProcesses));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            if (model.mapChannelToSize.ContainsKey(ChannelName))
            {
                List<Expression> guardUpdateChannel = AutomataBDD.GetGuardUpdateOfChannelOutput(this.ChannelName, new List<Expression>(ExpressionList), null, model);
                Event channelOutput = new BDDEncoder.EventChannelInfo(this.ChannelName, 0, BDDEncoder.EventChannelInfo.EventType.ASYNC_CHANNEL_OUTPUT);

                guards.Add(guardUpdateChannel[0]);
                events.Add(channelOutput);
                programBlocks.Add(guardUpdateChannel[1]);
                processes.Add(Process);
            }
            else
            {
                Event channelOutput = new BDDEncoder.EventChannelInfo(this.ChannelName, 0, BDDEncoder.EventChannelInfo.EventType.SYNC_CHANNEL_OUTPUT);
                channelOutput.ExpressionList = this.ExpressionList;

                guards.Add(null);
                events.Add(channelOutput);
                programBlocks.Add(null);
                processes.Add(Process);
            }
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD processAutomataBDD = this.Process.Encode(encoder);

            if (encoder.model.mapChannelToSize.ContainsKey(this.ChannelName))
            {
                int channelEventIndex = encoder.GetChannelIndex(this.ChannelName, BDDEncoder.EventChannelInfo.EventType.ASYNC_CHANNEL_OUTPUT);

                return AutomataBDD.ChannelOutputPrefixing(this.ChannelName, channelEventIndex, new List<Expression>(this.ExpressionList), null, processAutomataBDD, encoder.model);
            }
            else
            {
                List<Expression> expressionList = (this.ExpressionList != null) ? new List<Expression>(this.ExpressionList) : new List<Expression>();
                int channelEventIndex = encoder.GetEventIndex(this.ChannelName, expressionList.Count);

                return AutomataBDD.SyncChannelOutputPrefixing(channelEventIndex, expressionList, processAutomataBDD, encoder.model);
            }
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            //Channel always create new one
            allEvents.Add(string.Empty);
            this.Process.CollectEvent(allEvents, calledProcesses);
        }
#endif
        public override bool MustBeAbstracted()
        {
            return Process.MustBeAbstracted();
        }

    }  
}