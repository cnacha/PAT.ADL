using System;
using System.Text;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class ChannelInputGuarded : Process
    {
        public Expression GuardExpression;
        public string ChannelName;
        public Expression ChannelIndex;

        public Expression[] ExpressionList;
        public Process Process;

        public ChannelInputGuarded(string evtName, Expression index, Expression[] exp, Process process, Expression guard)
        {
            ChannelName = evtName;            
            ExpressionList = exp;
            Process = process;
            GuardExpression = guard;
            ChannelIndex = index;

            StringBuilder ID = new StringBuilder();
            ID.Append(ChannelName);
            if (ChannelIndex != null)
            {
                ID.Append(ChannelIndex);
            }
            ID.Append("?");
            ID.Append(Common.Classes.Ultility.Ultility.PPIDListDot(ExpressionList));
            ID.Append(guard.ExpressionID); //.GetID()
            ID.Append(Constants.EVENTPREFIX);
            ID.Append(Process.ProcessID);
            ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ChannelQueue Buffer = null;

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

            if (GlobalEnv.Channels.TryGetValue(channelName, out Buffer))
            {
                if (Buffer.Count > 0)
                {
                    ChannelQueue newBuffer = Buffer.Clone();
                    ExpressionValue[] values = newBuffer.Dequeue();

                    if (values.Length == this.ExpressionList.Length)
                    {
                        Dictionary<string, Expression> mapping = new Dictionary<string, Expression>(values.Length);

                        Valuation newEnv = GlobalEnv.GetChannelClone();
                        newEnv.Channels[channelName] = newBuffer;

                        string eventName = channelName + "?";
                        string eventID = channelName + "?";
                        
                        for (int i = 0; i < ExpressionList.Length; i++)
                        {
                            ExpressionValue v = values[i];
                            if (i == ExpressionList.Length - 1)
                            {
                                eventName += v;
                                eventID += v.ExpressionID;
                            }
                            else
                            {
                                eventName += v + ".";
                                eventID += v.ExpressionID + ".";
                            }

                            if (ExpressionList[i] is Variable)
                            {
                                mapping.Add(ExpressionList[i].ExpressionID, v);
                            }
                            else
                            {
                                if (v.ExpressionID != ExpressionList[i].ExpressionID)
                                {
                                    return; //list
                                }
                            }
                        }

                        Expression guard = GuardExpression.ClearConstant(mapping);

                        ExpressionValue value = EvaluatorDenotational.Evaluate(guard, newEnv);

                        if ((value as BoolConstant).Value)
                        {
                            Process newProcess = mapping.Count > 0 ? Process.ClearConstant(mapping) : Process;

                            if(eventID != eventName)
                            {
                                list.Add(new Configuration(newProcess, eventID, eventName, newEnv, false));
                            }
                            else
                            {
                                list.Add(new Configuration(newProcess, eventID, null, newEnv, false));
                            }
                        }
                    }
                }
            }

            //return list;
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            string channelName = this.ChannelName;

            if (ChannelIndex != null)
            {
                int size = ((IntConstant)EvaluatorDenotational.Evaluate(ChannelIndex, eStep.GlobalEnv)).Value;
                if (size >= Specification.ChannelArrayDatabase[ChannelName])
                {
                    throw new PAT.Common.Classes.Expressions.ExpressionClass.IndexOutOfBoundsException("Channel index out of bounds for expression " + this.ToString());
                }
                channelName = channelName + "[" + size + "]";
            }

            //List<Configuration> list = new List<Configuration>(1);
            if (eStep.ChannelName == channelName && eStep.Expressions.Length == ExpressionList.Length)
            {
                Dictionary<string, Expression> mapping = new Dictionary<string, Expression>(eStep.Expressions.Length);

                for (int i = 0; i < ExpressionList.Length; i++)
                {
                    Expression v = eStep.Expressions[i];
                   
                    if (ExpressionList[i] is Variable)
                    {
                        mapping.Add(ExpressionList[i].ExpressionID, v);
                    }
                    else
                    {
                        if (v.ExpressionID != ExpressionList[i].ExpressionID)
                        {
                            return ;
                        }
                    }
                }
                
                Expression guard = GuardExpression.ClearConstant(mapping);
                ExpressionValue value = EvaluatorDenotational.Evaluate(guard, eStep.GlobalEnv);

                if ((value as BoolConstant).Value)
                {
                    Configuration vm;
                    if (mapping.Count > 0)
                    {
                        vm = new Configuration(Process.ClearConstant(mapping), null, null, eStep.GlobalEnv, false);
                    }
                    else
                    {
                        vm = new Configuration(Process, null, null, eStep.GlobalEnv, false);
                    }

                    list.Add(vm);
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
            List<string> Variables = Process.GetGlobalVariables();
            Common.Classes.Ultility.Ultility.AddList(Variables, GuardExpression.GetVars());

            foreach (Expression expression in ExpressionList)
            {
                Common.Classes.Ultility.Ultility.AddList(Variables, expression.GetVars());
            }
            
            if(ChannelIndex != null)
            {
                Common.Classes.Ultility.Ultility.AddList(Variables, ChannelIndex.GetVars());
            }

            return Variables;
        }

        public override List<string> GetChannels()
        {
            List<string> toReturn = Process.GetChannels();

            if (!toReturn.Contains(this.ChannelName))
            {
                toReturn.Add(this.ChannelName);
            }

            return toReturn;
        }

        public override string ToString()
        {
            if (ChannelIndex == null)
            {
                return ChannelName + "?[" + GuardExpression.ToString() + "]" +
                       Common.Classes.Ultility.Ultility.PPStringListDot(ExpressionList).TrimStart('.') + "->" + Process;
            }
            else
            {
                return ChannelName + "[" + ChannelIndex +"]" + "?[" + GuardExpression.ToString() + "]" +
                       Common.Classes.Ultility.Ultility.PPStringListDot(ExpressionList).TrimStart('.') + "->" + Process;
            }
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression[] newExpression = new Expression[ExpressionList.Length];
            for (int i = 0; i < ExpressionList.Length; i++)
            {
                if (ExpressionList[i] is ExpressionValue)
                {
                    newExpression[i] = ExpressionList[i];
                }
                else
                {
                    newExpression[i] = ExpressionList[i].ClearConstant(constMapping);
                    //evaluate the value after the clearance, to make sure there only single variable or single value for each expression
                    if (!newExpression[i].HasVar)
                    {
                        newExpression[i] = EvaluatorDenotational.Evaluate(newExpression[i], null);
                    }                    
                }
            }

            return new ChannelInputGuarded(ChannelName, ChannelIndex != null ? ChannelIndex.ClearConstant(constMapping) : ChannelIndex, newExpression, Process.ClearConstant(constMapping), GuardExpression.ClearConstant(constMapping));
        }
#if BDD
        public override Process Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression[] newExpression = new Expression[ExpressionList.Length];
            for (int i = 0; i < ExpressionList.Length; i++)
            {
                if (ExpressionList[i] is ExpressionValue)
                {
                    newExpression[i] = ExpressionList[i];
                }
                else
                {
                    newExpression[i] = ExpressionList[i].ClearConstant(constMapping);
                    //evaluate the value after the clearance, to make sure there only single variable or single value for each expression
                    if (!newExpression[i].HasVar)
                    {
                        newExpression[i] = EvaluatorDenotational.Evaluate(newExpression[i], null);
                    }
                }
            }

            Process result = new ChannelInputGuarded(ChannelName, ChannelIndex != null ? ChannelIndex.ClearConstant(constMapping) : ChannelIndex, newExpression, Process.Rename(constMapping, newDefNames, renamedProcesses), GuardExpression.ClearConstant(constMapping));
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

            if (GuardExpression.HasExternalLibraryCall() || ChannelIndex != null)
            {
                return (IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0);
            }

            return (IsBDDEncodableProp = Process.IsBDDEncodable(calledProcesses));
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            lts.AddParaInChannelAsLocalVar(ExpressionList);

            if (model.mapChannelToSize.ContainsKey(ChannelName))
            {
                List<Expression> guardUpdateChannel = AutomataBDD.GetGuardUpdateOfChannelInput(this.ChannelName, new BoolConstant(true), new List<Expression>(ExpressionList), null, model);
                guardUpdateChannel[0] = Expression.AND(guardUpdateChannel[0], this.GuardExpression);
                Event channelInput = new BDDEncoder.EventChannelInfo(this.ChannelName, 0, BDDEncoder.EventChannelInfo.EventType.ASYNC_CHANNEL_INPUT);

                guards.Add(guardUpdateChannel[0]);
                events.Add(channelInput);
                programBlocks.Add(guardUpdateChannel[1]);
                processes.Add(Process);
            }
            else
            {
                Event channelInput = new BDDEncoder.EventChannelInfo(this.ChannelName, 0, BDDEncoder.EventChannelInfo.EventType.SYNC_CHANNEL_INPUT);
                channelInput.ExpressionList = this.ExpressionList;

                guards.Add(null);
                events.Add(channelInput);
                programBlocks.Add(null);
                processes.Add(Process);
            }
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            AutomataBDD processAutomataBDD = this.Process.Encode(encoder);

            if (encoder.model.mapChannelToSize.ContainsKey(this.ChannelName))
            {
                int channelEventIndex = encoder.GetChannelIndex(this.ChannelName, BDDEncoder.EventChannelInfo.EventType.ASYNC_CHANNEL_INPUT);

                //
                return AutomataBDD.ChannelInputPrefixing(this.ChannelName, channelEventIndex, this.GuardExpression, new List<Expression>(this.ExpressionList), null, processAutomataBDD, encoder.model);
            }
            else
            {
                List<Expression> expressionList = (this.ExpressionList != null) ? new List<Expression>(this.ExpressionList) : new List<Expression>();
                int channelEventIndex = encoder.GetEventIndex(this.ChannelName, expressionList.Count);

                //
                return AutomataBDD.SyncChannelInputPrefixing(channelEventIndex, this.GuardExpression, expressionList, processAutomataBDD, encoder.model);
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