using System;
using System.Text;
using System.Collections.Generic;
using PAT.Common.Classes.DataStructure;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class ChannelInputDataOperation : Process
    {
        public string ChannelName;
        public Expression ChannelIndex;
        public Expression[] ExpressionList;
        public Process Process;
        public Expression AssignmentExpr;
        public bool HasLocalVar;

        public ChannelInputDataOperation(string evtName, Expression index, Expression[] exp, Process process, Expression assignment, bool localvar)
        {
            ChannelName = evtName;

            ChannelIndex = index;

            ExpressionList = exp;
            Process = process;

            AssignmentExpr = assignment;
            HasLocalVar = localvar;

            StringBuilder ID = new StringBuilder();
            ID.Append(ChannelName);
            if (ChannelIndex != null)
            {
                ID.Append(ChannelIndex);
            }
            ID.Append("?");
            ID.Append(Common.Classes.Ultility.Ultility.PPIDListDot(ExpressionList));
            ID.Append("{" + AssignmentExpr.ExpressionID + "}");
            ID.Append(Constants.EVENTPREFIX);

            ID.Append(Process.ProcessID);
            ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ChannelQueue Buffer = null;

            string channelName = this.ChannelName;

            if(ChannelIndex != null)
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

                    if (values.Length == ExpressionList.Length)
                    {
                        Dictionary<string, Expression> mapping = new Dictionary<string, Expression>(values.Length);

                        Valuation newChannelEvn = GlobalEnv.GetChannelClone();
                        newChannelEvn.Channels[channelName] = newBuffer;

                        string eventName = channelName + "?";
                        string eventID = channelName + "?";


                        for (int i = 0; i < ExpressionList.Length; i++)
                        {
                            ExpressionValue v = values[i];
                            if (i == ExpressionList.Length - 1)
                            {
                                eventName += v;
                                eventID += v.ExpressionID;//.GetID();
                            }
                            else
                            {
                                eventName += v + ".";
                                eventID += v.ExpressionID + ".";
                            }

                            if (ExpressionList[i] is Variable)
                            {
                                mapping.Add(ExpressionList[i].ExpressionID, v); //.GetID()
                            }
                            else
                            {
                                if (v.ExpressionID != ExpressionList[i].ExpressionID) //.GetID() .GetID() 
                                {
                                    return; //list
                                }
                            }
                        }

                        Valuation newGlobleEnv = newChannelEvn.GetVariableClone();

                        EvaluatorDenotational.Evaluate(AssignmentExpr.ClearConstant(mapping), newGlobleEnv);

                        if (HasLocalVar)
                        {
                            Valuation tempEnv = newChannelEvn.GetVariableClone();
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

                        Process newProcess = mapping.Count > 0 ? Process.ClearConstant(mapping) : Process;

                        if (eventID != eventName)
                        {
                            list.Add(new Configuration(newProcess, eventID, eventName, newGlobleEnv, false));
                        }
                        else
                        {
                            list.Add(new Configuration(newProcess, eventID, null, newGlobleEnv, false));
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

            if (eStep.ChannelName == channelName && eStep.Expressions.Length == ExpressionList.Length)
            {
                Dictionary<string, Expression> mapping = new Dictionary<string, Expression>(eStep.Expressions.Length);

                for (int i = 0; i < ExpressionList.Length; i++)
                {
                    Expression v = eStep.Expressions[i];
                   
                    if (ExpressionList[i] is Variable)
                    {
                        mapping.Add(ExpressionList[i].ExpressionID, v); //.GetID()
                    }
                    else
                    {
                        if (v.ExpressionID != ExpressionList[i].ExpressionID) //.GetID()
                        {
                            return ;
                        }
                    }
                }

                Valuation newGlobleEnv = eStep.GlobalEnv.GetVariableClone();

                EvaluatorDenotational.Evaluate(AssignmentExpr.ClearConstant(mapping), newGlobleEnv);

                if (HasLocalVar)
                {
                    Valuation tempEnv = eStep.GlobalEnv.GetVariableClone();
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

                Configuration vm;
                if (mapping.Count > 0)
                {
                    vm = new Configuration(Process.ClearConstant(mapping), null, null, newGlobleEnv, false);
                }
                else
                {
                    vm = new Configuration(Process, null, null, newGlobleEnv, false);
                }

                list.Add(vm);
            }
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

            Common.Classes.Ultility.Ultility.AddList(toReturn, AssignmentExpr.GetVars());

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
             if(ChannelIndex != null)
             {
                 return ChannelName + "[" + ChannelIndex.ToString() + "]" + "?" +
                        Common.Classes.Ultility.Ultility.PPStringListDot(ExpressionList).TrimStart('.') + "{" + AssignmentExpr + "}->" + Process;
             }
             else
             {
                 return ChannelName + "?" +
                        Common.Classes.Ultility.Ultility.PPStringListDot(ExpressionList).TrimStart('.') + "{" + AssignmentExpr + "}->" + Process;
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

            return new ChannelInputDataOperation(ChannelName, ChannelIndex != null ? ChannelIndex.ClearConstant(constMapping) : ChannelIndex, newExpression, Process.ClearConstant(constMapping), AssignmentExpr.ClearConstant(constMapping), HasLocalVar);
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

            Process result = new ChannelInputDataOperation(ChannelName, ChannelIndex != null ? ChannelIndex.ClearConstant(constMapping) : ChannelIndex, newExpression, Process.Rename(constMapping, newDefNames, renamedProcesses), AssignmentExpr.ClearConstant(constMapping), HasLocalVar);
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
            lts.AddParaInChannelAsLocalVar(ExpressionList);

            if (model.mapChannelToSize.ContainsKey(ChannelName))
            {
                List<Expression> guardUpdateChannel = AutomataBDD.GetGuardUpdateOfChannelInput(this.ChannelName, new BoolConstant(true), new List<Expression>(ExpressionList), AssignmentExpr, model);
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
                return AutomataBDD.ChannelInputPrefixing(this.ChannelName, channelEventIndex, new BoolConstant(true), new List<Expression>(this.ExpressionList), AssignmentExpr, processAutomataBDD, encoder.model);
            }
            else
            {
                List<Expression> expressionList = (this.ExpressionList != null) ? new List<Expression>(this.ExpressionList) : new List<Expression>();
                int channelEventIndex = encoder.GetEventIndex(this.ChannelName, expressionList.Count);

                //
                return AutomataBDD.SyncChannelInputPrefixing(channelEventIndex, new BoolConstant(true), expressionList, processAutomataBDD, encoder.model);
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
