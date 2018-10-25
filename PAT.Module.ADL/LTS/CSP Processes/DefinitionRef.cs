using System.Collections.Generic;
using System.Text;
using PAT.Common.Classes.CUDDLib;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.Ultility;
using SequenceExpression = PAT.Common.Classes.Expressions.ExpressionClass.Sequence;
using System;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class DefinitionRef : Process
    {
        public string Name;
        public Expression[] Args;
        public Definition Def;

        public DefinitionRef(string name, Expression[] args)
        {
            Name = name;
            Args = args;
            
            StringBuilder ID = new StringBuilder(Name + "(");

            for (int i = 0; i < Args.Length; i++)
            {
                ID.Append(Args[i].ExpressionID);
                if (i < Args.Length - 1)
                {
                    ID.Append(",");
                }
            }

            ID.Append(")");
            ProcessID = DataStore.DataManager.InitializeProcessID(ID.ToString());
        }

        public Process GetProcess(Valuation global)
        {
          //  Console.WriteLine("GetProcess for " + this.Name +" Def:"+this.Def);
            Expression[] newArgs = new Expression[Args.Length];
            try { 
            //instance String has all information about the argument value,  which is used for storing the process of definition into the DefinitionInstanceDatabase
            //idString store only local information about the argument (exclude the global variables)
            string instanceString = Name + "(";

            string idString = Name + "(";

            for (int i = 0; i < Args.Length; i++)
            {
                Expression exp = Args[i];
                idString += exp.ExpressionID;

                if (!exp.HasVar)
                {                    
                    newArgs[i] = exp;
                    instanceString += exp.ExpressionID;
                }
                else
                {
                    ExpressionValue v = EvaluatorDenotational.Evaluate(exp, global);
                    if (v == null)
                    {
                        throw new RuntimeException(string.Format("The argument {0} of parameter {1} in process {2} evaluated to be null!", exp, Def.Parameters[i], Name));
                    }
                    newArgs[i] = v;

                    instanceString += v.ExpressionID;
                }

                if (newArgs[i] is IntConstant)
                {
                    int val = (newArgs[i] as IntConstant).Value;
                    //    Console.WriteLine("   getting param "+i+" : "+ Def.Parameters);
                    string x = Def.Parameters[i];
                    if (Def.ParameterLowerBound.ContainsKey(x))
                    {
                        int bound = Def.ParameterLowerBound.GetContainsKey(x);
                        if (bound > val)
                        {
                            throw new VariableValueOutOfRangeException("Argument " + x + "'s current value " + val +
                                                                       " is smaller than its lower bound " + bound);
                        }
                    }
                    if (Def.ParameterUpperLowerBound.ContainsKey(x))
                    {
                        int bound = Def.ParameterUpperLowerBound.GetContainsKey(x);
                        if (val > bound)
                        {
                            throw new VariableValueOutOfRangeException("Argument " + x + "'s current value " + val +
                                                                       " is greater than its upper bound " + bound);
                        }
                    }
                }


                if (i < Args.Length - 1)
                {
                    instanceString += ",";
                    idString += ",";
                }
            }
            idString += ")";
            instanceString += ")";

            Process ProcExpr = DataStore.DataManager.DefinitionInstanceDatabase.GetContainsKey(instanceString);
            if (ProcExpr != null)
            {
                return ProcExpr;
            }
            else
            {
                Dictionary<string, Expression> values = new Dictionary<string, Expression>(Args.Length);

                for (int i = 0; i < newArgs.Length; i++)
                {
                    values.Add(Def.Parameters[i], newArgs[i]);
                }

                ProcessID = DataStore.DataManager.InitializeProcessID(idString);

                //lock the data manager to prevent the multi-thread update the last Process ID at the same time.
                lock (DataStore.DataManager)
                {
                    ProcExpr = Def.Process.ClearConstant(values); //Instantiate
                    DataStore.DataManager.SetLastProcessID(ProcessID);
                }

                ProcExpr.ProcessID = ProcessID;
                DataStore.DataManager.DefinitionInstanceDatabase.Add(instanceString, ProcExpr);
              //      Console.WriteLine("     ProcExpr: " + ProcExpr);
                return ProcExpr;
            }
            } catch(Exception e)
            {
                //Console.WriteLine(e.ToString());
                throw e;
            }
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
        //    Console.WriteLine("MoveOneStep");
      //      Console.WriteLine("    gloenv " + GlobalEnv +"#");
            GetProcess(GlobalEnv).MoveOneStep(GlobalEnv, list);
        }

        public override void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
            GetProcess(GlobalEnv).SyncOutput(GlobalEnv, list);
        }

        public override void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list)
        {
            GetProcess(eStep.GlobalEnv).SyncInput(eStep, list);
        }

        public override string ToString()
        {
            return Name + "(" + Common.Classes.Ultility.Ultility.PPStringList(Args) + ")";
        }

        public override HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            if (visitedDefinitionRefs == null)
            {
                return new HashSet<string>();
            }

            if (Specification.CollectDataOperationEvent == null)
            {
                if (Def.AlphabetsCalculable)
                {
                    return new HashSet<string>(Def.Alphabets);
                }

                //if the alphabet is defined manually. 
                if (Def.AlphabetEvents != null)
                {
                    Dictionary<string, Expression> newMapping = new Dictionary<string, Expression>();
                    for (int i = 0; i < Args.Length; i++)
                    {
                        string key = Def.Parameters[i];
                        newMapping.Add(key, Args[i]);
                    }
                     
                    EventCollection evtcoll = Def.AlphabetEvents.ClearConstant(newMapping);

                    if (!evtcoll.ContainsVariable())
                    {
                        return new HashSet<string>(evtcoll.EventNames);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("ERROR - PAT FAILED to calculate the alphabet of process " + Name + ".");
                        sb.AppendLine("CAUSE - Process " + Name + " is invoked with gloabl variables as parameters!");
                        sb.AppendLine(
                            "REMEDY - 1) Avoid passing global variable as parameters  2) Or manually specify the alphabet of process " +
                            Name +
                            " using the following syntax: \n\r\t #alphabet " + Name +
                            " {X}; \n\rwhere X is a set of event names with no variables.");
                        throw new RuntimeException(sb.ToString());
                    }
                }

                //if the arguments contain variable (global variable), then throw an exception to say we can't calculate the alphabet.
                foreach (var arg in Args)
                {
                    if (arg.HasVar)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("ERROR - PAT FAILED to calculate the alphabet of process " + Name + ".");
                        sb.AppendLine("CAUSE - Process " + Name +
                                      " is invoked with gloabl variables as parameters!");
                        sb.AppendLine("REMEDY - Manually specify the alphabet of process " + Name +
                                      " using the following syntax: \n\r\t #alphabet " + Name +
                                      " {X}; \n\rwhere X is a set of events.");
                        throw new RuntimeException(sb.ToString());
                    }
                }
            }

            string idString = Name + "(";

            for (int i = 0; i < Args.Length; i++)
            {
                Expression exp = Args[i];
                idString += exp.ToString();
                if (i < Args.Length - 1)
                {
                    idString += ",";
                }
            }

            idString += ")";

            if (Specification.CollectDataOperationEvent == null)
            {
                if (visitedDefinitionRefs.ContainsKey(Name))
                {
                    if (visitedDefinitionRefs[Name] != idString)
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine("ERROR - PAT FAILED to calculate the alphabet of process " + Name + ".");
                        sb.AppendLine("CAUSE - Process " + Name + " is recursively invoked with different parameters!");
                        sb.AppendLine("REMEDY - Manually specify the alphabet of process " + Name +
                                      " using the following syntax: \n\r\t #alphabet " + Name +
                                      " {X}; \n\rwhere X is a set of events.");
                        throw new RuntimeException(sb.ToString());
                    }
                    else
                    {
                        return new HashSet<string>();
                    }
                }
                else
                {
                    Dictionary<string, string> newVisitedDef = new Dictionary<string, string>();

                    foreach (string var in visitedDefinitionRefs.Keys)
                    {
                        newVisitedDef.Add(var, visitedDefinitionRefs[var]);
                    }

                    newVisitedDef.Add(Name, idString);

                    return GetProcess(null).GetAlphabets(newVisitedDef);
                }
            }
            else
            {

                if (visitedDefinitionRefs.ContainsKey(Name))
                {
                   return new HashSet<string>();                   
                }
                else
                {
                    Dictionary<string, string> newVisitedDef = new Dictionary<string, string>();

                    foreach (string var in visitedDefinitionRefs.Keys)
                    {
                        newVisitedDef.Add(var, visitedDefinitionRefs[var]);
                    }

                    newVisitedDef.Add(Name, idString);

                    return Def.Process.GetAlphabets(newVisitedDef);
                }
            }
        }

        public override List<string> GetGlobalVariables()
        {
            // Console.WriteLine("Def :"+ Def);
            List<string> vars = Def.GlobalVars;
            foreach (Expression expression in Args)
            {
                Common.Classes.Ultility.Ultility.Union(vars, expression.GetVars());
            }
            return vars; 
        }

        public override List<string> GetChannels()
        {
            return Def.Channels;
        }        

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression[] newArgs = new Expression[Args.Length];
            DefinitionRef newRef = null;
          // Console.WriteLine("clearing constant for " + this.Name +" Def"+Def);
            try { 
            for (int i = 0; i < Args.Length; i++)
            {
                Expression arg = Args[i].ClearConstant(constMapping);

                if (!arg.HasVar)
                {
                    newArgs[i] = EvaluatorDenotational.Evaluate(arg, null);
                }
                else
                {
                    newArgs[i] = arg;
                }
            }

            newRef = new DefinitionRef(Name, newArgs);

            //this is a special cases happened in the middle of parsing, where the current Def is not initialized in the parser.
            //so need to put the newRef into the def list to initialize the Def once the parsing is done.
            if (Def == null)
            {
            //    EGTreeWalker.dlist.Add(newRef);
            //    EGTreeWalker.dtokens.Add(null);
            }
            else
            {
                newRef.Def = Def;
            }
            } catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                throw e;
            }
            return newRef;
        }

#if BDD
        public override CSPProcess Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses)
        {
            Expression[] newArguments = new Expression[Args.Length];
            Array.Copy(Args, newArguments, Args.Length);
            for (int i = 0; i < Args.Length; i++)
            {
                newArguments[i] = constMapping.ContainsKey(Args[i].ToString()) ? constMapping[Args[i].ToString()] : Args[i];
            }

            if(!newDefNames.ContainsKey(Name))
            {
                newDefNames.Add(Name, Name + "-" + Common.Classes.Ultility.Ultility.PPStringList(newArguments) + "-");
            }
            string newName = newDefNames[Name];

            
            DefinitionRef newRef = new DefinitionRef(newName, new Expression[0]);
            newRef.IsBDDEncodableProp = IsBDDEncodableProp;

            if (renamedProcesses.ContainsKey(newName))
            {
                newRef.Def = renamedProcesses[newName];
            }
            else
            {
                Def.Name = newName;
                newRef.Def = Def.Rename(constMapping, newDefNames, renamedProcesses, new List<Expression>(Args));

                //Remove this new name from the dictionary for a new DefRef
                newDefNames.Remove(Name);
            }
            
            return newRef;
        }

                /// <summary>
        /// </summary>
        /// <param name="calledProcesses"></param>
        /// <returns></returns>
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            //use ProcessID and Name to distinguish when process has parameter
            string name = this.Name;
            if (!calledProcesses.Contains(name))
            {
                ////This is the place where we call the function
                //for (int i = 0; i < Args.Length; i++)
                //{
                //    //only recursive without change parameter
                //    if (!(Args[i] is IntConstant) || Args[i].HasExternalLibraryCall())
                //    {
                //        this.IsBDDEncodableProp = Constants.BDD_NON_ENCODABLE_0;
                //        return Constants.BDD_NON_ENCODABLE_0;
                //    }
                //}


                //always add to know self-loop
                calledProcesses.Add(name);
                int lastIndex = calledProcesses.Count - 1;

                int result = this.Def.Process.IsBDDEncodable(calledProcesses);

                int count = 0;
                for(int i = lastIndex; i < calledProcesses.Count; i++)
                {
                    if (calledProcesses[i] == name)
                    {
                        count++;
                    }
                }

                //self-loop, could not composition
                if (count >= 2)
                {
                    result = result & 2;
                }

                //Remove all calls of this process
                while (calledProcesses.Remove(name))
                {
                    
                }
                
                this.IsBDDEncodableProp = result;
                return result;
            }
            else
            {
                //for (int i = 0; i < Args.Length; i++)
                //{
                //    //only recursive without change parameter
                //    if (Args[i].ToString() != Def.Parameters[i])
                //    {
                //        return Constants.BDD_NON_ENCODABLE_0;
                //    }
                //}

                //always add to know self-loop
                calledProcesses.Add(name);
                return Constants.BDD_LTS_COMPOSITION_3;
            }
        }

        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<CSPProcess> processes, Model model, SymbolicLTS lts)
        {
            //for (int i = 0; i < Args.Length; i++)
            //{
            //    string paraName = Def.Parameters[i];

            //    if (!lts.Parameters.Contains(paraName))
            //    {
            //        int lowerBound = (Def.ParameterLowerBound.ContainsKey(paraName)) ? Def.ParameterLowerBound.GetContainsKey(paraName) : Model.BDD_INT_LOWER_BOUND;
            //        int upperBound = (Def.ParameterUpperLowerBound.ContainsKey(paraName)) ? Def.ParameterUpperLowerBound.GetContainsKey(paraName) : Model.BDD_INT_UPPER_BOUND;
            //        lts.Parameters.Add(paraName);
            //        lts.ParameterUpperBound.Add(paraName, upperBound);
            //        lts.ParameterLowerBound.Add(paraName, lowerBound);
            //    }
            //}

            //Expression updatePara = GetUpdatePara();
            //if (updatePara != null)
            //{
            //    guards.Add(null);
            //    events.Add(new Event(Constants.TAU));
            //    programBlocks.Add(updatePara);
            //    processes.Add(Def.Process);
            //}
            //else
            //{
            //    Def.Process.MakeDiscreteTransition(guards, events, programBlocks, processes, model, lts);
            //}

            Def.Process.MakeDiscreteTransition(guards, events, programBlocks, processes, model, lts);
        }

        /// <summary>
        /// If no need update para, return null
        /// </summary>
        /// <returns></returns>
        private Expression GetUpdatePara()
        {
            bool needUpdates = false;
            for (int i = 0; i < Args.Length; i++)
            {
                if (Args[i].ToString() != Def.Parameters[i])
                {
                    needUpdates = true;
                }
            }

            if (needUpdates)
            {
                Expression updatePara = new Assignment(Def.Parameters[0], Args[0]);

                for (int i = 1; i < Args.Length; i++)
                {

                    updatePara = new SequenceExpression(updatePara, new Assignment(Def.Parameters[0], Args[0]));
                }
                return updatePara;
            }

            return null;
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            this.AddParameterVariable(encoder.model);

            AutomataBDD result = this.Def.Process.Encode(encoder);
            for (int i = 0; i < this.Args.Length; i++)
            {
                string para = this.Def.Parameters[i];
                result.initExpression = Expression.AND(result.initExpression,
                                            Expression.EQ(new Variable(para), this.Args[i]));
            }
            return result;
        }

        public void AddParameterVariable(Model model)
        {
            if (Def.Parameters != null)
            {
                for (int i = 0; i < this.Def.Parameters.Length; i++)
                {
                    string parameter = this.Def.Parameters[i];

                    int min = Model.BDD_INT_UPPER_BOUND;
                    int max = Model.BDD_INT_LOWER_BOUND;

                    if (this.Def.ParameterLowerBound.ContainsKey(parameter) && this.Def.ParameterUpperLowerBound.ContainsKey(parameter))
                    {
                        min = this.Def.ParameterLowerBound.GetContainsKey(parameter);
                        max = this.Def.ParameterUpperLowerBound.GetContainsKey(parameter);
                    }
                    else
                    {
                        ExpressionBDDEncoding argumentBDD = Args[i].TranslateIntExpToBDD(model);
                        foreach (CUDDNode argExp in argumentBDD.ExpressionDDs)
                        {
                            min = Math.Min(min, (int) CUDD.FindMinThreshold(argExp, Model.BDD_INT_LOWER_BOUND));
                            max = Math.Max(max, (int) CUDD.FindMaxThreshold(argExp, Model.BDD_INT_UPPER_BOUND));
                        }
                    }

                    //Also set the parameter as global variable to make sure that the parameters unchanged in encoding Transition
                    model.AddGlobalVar(parameter, min, max);
                }
            }

        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            if (!calledProcesses.Contains(this.Name))
            {
                calledProcesses.Add(this.Name);
                Def.Process.CollectEvent(allEvents, calledProcesses);
            }
        }
#endif

        public override bool MustBeAbstracted() {
            return Def.MustAbstract;
        }

        public override Process GetTopLevelConcurrency(List<string> visitedDef)
        {
            if (!visitedDef.Contains(Name))
            {
                List<string> newVisitedDef = new List<string>(visitedDef);
                newVisitedDef.Add(Name);
                return Def.Process.GetTopLevelConcurrency(newVisitedDef);
            }

            return null;
        }
    }
}
