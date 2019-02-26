using System;
using System.Collections.Generic;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using PAT.Common;
using PAT.Common.Classes.DataStructure;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.Ultility;
using System.Xml;
using PAT.ADL.Assertions;
using Antlr4.Runtime;
using PAT.ADL.LTS.ADL_Parser;
using ADLCompiler;
using ADLParser.Classes;
using static PAT.ADL.LTS.ADL_Parser.ADLParser;

namespace PAT.ADL.LTS
{
    /// <summary>
    /// The specification class a collection of the definitions, properties and alphasets
    /// Each of the user's input correspons to a user's input file
    /// </summary>
    public partial class Specification : SpecificationBase
    {
        public Dictionary<string, Definition> DefinitionDatabase = new Dictionary<string, Definition>(16);
        public Dictionary<string, ChannelQueue> ChannelDatabase = new Dictionary<string, ChannelQueue>(8);
        public static Dictionary<string, int> ChannelArrayDatabase = new Dictionary<string, int>(8);
        public List<IndexParallel> ParallelDatabase = new List<IndexParallel>(8);
        public Dictionary<string, Expression> DeclarationDatabase = new Dictionary<string, Expression>();
        public Dictionary<string, Configuration> ConfigurationDatabase = new Dictionary<string, Configuration>(16);
        public Dictionary<string, Connector> ConnectorDatabase = new Dictionary<string, Connector>();
        public Dictionary<string, Component> ComponentDatabase = new Dictionary<string, Component>();
        public Dictionary<string, DefinitionRef> ExecProcessDatabase = new Dictionary<string, DefinitionRef>();
        public Dictionary<string, List<string>> CompStateDatabase = new Dictionary<string, List<string>>();

        public Valuation SpecValuation = new Valuation();
        public SharedDataObjects SharedData;

        private string ModelType;

        public bool HasWildVariable = false;

        //constructor used for the console, testing for sequencial access.
        public Specification(string spec) : base(spec, null)
        {
            SharedData = new SharedDataObjects();
            LockSpecificationData();

            ParseSpec(spec, "");
        }

        public Specification(string spec, string option, string filePath) : base(spec, filePath)
        {
            SharedData = new SharedDataObjects();
            PAT.Common.Classes.Ultility.Ultility.LockSharedData(this);

            try
            {
                ParseSpec(spec, option);

                //we need to store the default values that can be used for the simulation.
                SharedData.HasSyncrhonousChannel = HasSyncrhonousChannel;
                SharedData.SyncrhonousChannelNames = SyncrhonousChannelNames;
                SharedData.HasAtomicEvent = HasAtomicEvent;
                SharedData.HasCSharpCode = HasCSharpCode;
                SharedData.LocalVars = Valuation.HiddenVars;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            finally
            {
                PAT.Common.Classes.Ultility.Ultility.UnLockSharedData(this);
            }
        }

        /// <summary>
        /// Parse the specification from string input into objects
        /// </summary>
        /// <param name="spec">string input of the model</param>
        /// <param name="option">option for LTL parsing, usually it is an empty string</param>
        protected virtual void ParseSpec(string spec, string options)
        {
            Console.WriteLine("parsing spec... ");

            IsParsing = true;

            if (GlobalConstantDatabase == null)
                GlobalConstantDatabase = new Dictionary<string, Expression>();

            // Start parsing ADL
            AntlrInputStream inputStream = new AntlrInputStream(spec);
            ADLLexer speakLexer = new ADLLexer(inputStream);

            Antlr4.Runtime.CommonTokenStream commonTokenStream = new Antlr4.Runtime.CommonTokenStream(speakLexer);

            ADL_Parser.ADLParser parser = new ADL_Parser.ADLParser(commonTokenStream);

            ADLVisitor visitor = new ADLVisitor();

            Object element;
            StatementContext statement;
            CSPGenerator generator = new CSPGenerator(this);

            while (true)
            {
                statement = parser.statement();

                // parsing architecture elements
                if ((element = statement.archelement()) != null)
                {
                    element = visitor.Visit((ArchelementContext)element);

                    if (element is Component)
                    {
                        Component comp = (Component)element;
                        Console.WriteLine(comp.ToString());
                        ComponentDatabase.Add(comp.Name, comp);
                    }
                    else if (element is Connector)
                    {
                        Connector conn = ((Connector)element);
                        Console.WriteLine(conn.ToString());
                        ConnectorDatabase.Add(conn.Name, conn);
                    }
                    else if (element is SystemConfig)
                    {
                        Console.WriteLine(((SystemConfig)element).ToString());
                        // Console.WriteLine("system:"+((SystemConfig)element).ToString());
                        generator.parse((SystemConfig)element);
                    }

                    // parsing assetion
                }
                else if ((element = statement.assertion()) != null)
                {
                    AssertionExpr assertion = (AssertionExpr)visitor.VisitAssertion((AssertionContext)element);
                    Console.WriteLine(assertion.ToString());
                    generator.AddAssertion(assertion, options);
                }
                else
                    break;

            }

            // End parsing ADL


            StaticAnalysis();
            CheckingConflictingEventsAndVariables();

            foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
            {
                List<string> gVar = pair.Value.GlobalVars;

                int i = 0;
                while (i < gVar.Count)
                {
                    //Console.WriteLine("parsed " + gVar[i]);
                    if (SpecValuation.Variables != null && !SpecValuation.Variables.ContainsKey(gVar[i]))
                    {
                        gVar.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            //get the relevant channels; 
            if (ChannelDatabase.Count > 0)
            {
                SyncrhonousChannelNames = new List<string>(0);
                Dictionary<string, ChannelQueue> newChannelDatabase = new Dictionary<string, ChannelQueue>();

                foreach (KeyValuePair<string, ChannelQueue> pair in ChannelDatabase)
                {
                    if (pair.Value.Size == 0)
                    {
                        SyncrhonousChannelNames.Add(pair.Key);
                    }
                    else
                    {
                        newChannelDatabase.Add(pair.Key, pair.Value);
                    }
                }

                SpecValuation.Channels = newChannelDatabase;
                HasSyncrhonousChannel = SyncrhonousChannelNames.Count > 0;
            }

            foreach (KeyValuePair<string, AssertionBase> entry in AssertionDatabase)
            {
                entry.Value.Initialize(this);
            }

            CheckVariableRange();
            // Console.WriteLine("Finish parsing spec....................");
            Console.WriteLine(this.GetSpecification());
        }

        protected void CheckVariableRange()
        {
            foreach (KeyValuePair<string, Declaration> declaration in DeclaritionTable)
            {
                if (declaration.Value.DeclarationType == DeclarationType.Variable)
                {
                    Valuation.CheckVariableRange(declaration.Key, SpecValuation.Variables[declaration.Key], declaration.Value.DeclarationToken.Line, declaration.Value.DeclarationToken.CharPositionInLine);
                }
            }
        }
        /// <summary>
        /// Perform static analysis to identity things like
        /// Whether a definition must be abstraction
        /// what are the relevant variables
        /// what are the relevant channels
        /// </summary>
        private void StaticAnalysis()
        {
            //Create the definition graph by using the names.
            foreach (Definition definition in DefinitionDatabase.Values)
            {
                foreach (string name in definition.SubDefinitionNames)
                {
                    if (DefinitionDatabase.ContainsKey(name))
                    {
                        definition.SubDefinitions.Add(DefinitionDatabase[name]);
                    }
                }
                definition.SubDefinitionNames = null;
            }

            foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
            {
                pair.Value.StaticAnalysis();
            }

            //update MustAbstract
            bool modified = true;
            while (modified)
            {
                modified = false;
                foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
                {
                    if (!pair.Value.MustAbstract)
                    {
                        List<Definition> calls = pair.Value.SubDefinitions;

                        foreach (Definition s in calls)
                        {
                            if (s.MustAbstract)
                            {
                                pair.Value.MustAbstract = true;
                                modified = true;
                                break;
                            }
                        }
                    }
                }
            }

            modified = true;
            while (modified)
            {
                modified = false;
                foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
                {
                    if (pair.Value.AlphabetsCalculable)
                    {
                        List<Definition> calls = pair.Value.SubDefinitions;

                        foreach (Definition s in calls)
                        {
                            if (!s.AlphabetsCalculable)
                            {
                                pair.Value.AlphabetsCalculable = false;
                                modified = true;
                                break;
                            }
                        }
                    }
                }
            }

            modified = true;
            while (modified)
            {
                modified = false;
                foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
                {
                    if (pair.Value.AlphabetsCalculable && pair.Value.AlphabetEvents == null)
                    {
                        int size = pair.Value.Alphabets.Count;
                        pair.Value.Alphabets = pair.Value.Process.GetAlphabets(new Dictionary<string, string>());

                        if (size != pair.Value.Alphabets.Count)
                        {
                            modified = true;
                        }
                    }
                }
            }


            modified = true;
            while (modified)
            {
                modified = false;
                foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
                {

                    List<Definition> calls = pair.Value.SubDefinitions;

                    foreach (Definition s in calls)
                    {
                        foreach (string channel in s.Channels)
                        {
                            if (!pair.Value.Channels.Contains(channel))
                            {
                                pair.Value.Channels.Add(channel);
                                modified = true;
                            }
                        }
                    }
                }
            }

            modified = true;
            while (modified)
            {
                modified = false;
                foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
                {

                    List<Definition> calls = pair.Value.SubDefinitions;

                    foreach (Definition s in calls)
                    {
                        foreach (string var in s.GlobalVars)
                        {
                            if (!pair.Value.GlobalVars.Contains(var))
                            {
                                pair.Value.GlobalVars.Add(var);
                                modified = true;
                            }
                        }
                    }
                }
            }

            return;
        }

        /// <summary>
        /// return the initial configuration of the given startingProcess, this is used by the simulator
        /// </summary>
        /// <param name="startingProcess"></param>
        /// <returns></returns>
        public override ConfigurationBase SimulationInitialization(string startingProcess)
        {
            //Definition def = DefinitionDatabase[startingProcess];
            //DefinitionRef defref = new DefinitionRef(def.Name, new Expression[0]) {Def = def};

            //return new Configuration(defref, Constants.INITIAL_EVENT, null, SpecValuation, false);
            return null;
        }


        /// <summary>
        /// return the string representation of the model. This can be helpful info that will be displayed 
        /// in the output box after checking grammar
        /// </summary>
        /// <returns></returns>
        public override string GetSpecification()
        {
            StringBuilder sb = new StringBuilder();

            if (GlobalConstantDatabase != null && GlobalConstantDatabase.Count > 0)
            {
                sb.AppendLine("//====================Global Constants====================");
                foreach (KeyValuePair<string, Expression> pair in GlobalConstantDatabase)
                {
                    if (pair.Value is ExpressionValue)
                    {
                        sb.AppendLine("#define " + pair.Key + " " + pair.Value.ExpressionID + ";");
                    }
                    else
                    {
                        sb.AppendLine("#define " + pair.Key + " " + pair.Value.ToString() + ";");
                    }
                }
            }

            if (SpecValuation.Variables != null)
            {
                sb.AppendLine("//====================Global Variable Definitions====================");
                foreach (StringDictionaryEntryWithKey<ExpressionValue> pair in SpecValuation.Variables._entries)
                {
                    if (pair != null)
                    {

                        string range = "";
                        if (Valuation.VariableLowerBound.ContainsKey(pair.Key))
                        {
                            range = ":{" + Valuation.VariableLowerBound.GetContainsKey(pair.Key) + "..";
                        }

                        if (Valuation.VariableUpperLowerBound.ContainsKey(pair.Key))
                        {
                            if (range == "")
                            {
                                range = ":{.." + Valuation.VariableUpperLowerBound.GetContainsKey(pair.Key) + "}";
                            }
                            else
                            {
                                range += Valuation.VariableUpperLowerBound.GetContainsKey(pair.Key) + "}";
                            }
                        }
                        else
                        {
                            if (range != "")
                            {
                                range += "}";
                            }
                        }

                        if (Valuation.HiddenVars != null && Valuation.HiddenVars.ContainsKey(pair.Key))
                        {
                            sb.AppendLine("hvar " + pair.Key + range + " = " + pair.Value + ";");
                        }
                        else
                        {
                            sb.AppendLine("var " + pair.Key + range + " = " + pair.Value + ";");
                        }

                    }
                }
            }


            if (ChannelDatabase.Count > 0)
            {
                sb.AppendLine("//====================Channel Definitions====================");
                foreach (KeyValuePair<string, ChannelQueue> pair in ChannelDatabase)
                {
                    sb.AppendLine("channel " + pair.Key + " " + pair.Value.Size + ";");
                }
            }

            sb.AppendLine("//====================Process Definitions====================");
            foreach (Definition entry in DefinitionDatabase.Values)
            {
                sb.AppendLine(entry.GetFullDefinition() + "\r\n");
            }


            sb.AppendLine("\r\n");
            //sb.AppendLine("\r\n//============================Alphabets Definitions============================");
            foreach (Definition entry in DefinitionDatabase.Values)
            {
                if (entry.AlphabetEvents != null)
                {
                    sb.AppendLine("#alphabet " + entry.Name + " {" + Common.Classes.Ultility.Ultility.PPStringList(entry.AlphabetEvents) + "};");
                }
            }


            if (DeclarationDatabase.Count > 0)
            {
                sb.AppendLine("\r\n//============================Declarations============================");

                foreach (KeyValuePair<string, Expression> entry in DeclarationDatabase)
                {
                    sb.AppendLine("#define " + entry.Key + " " + entry.Value + ";");
                }
            }

            if (AssertionDatabase.Count > 0)
            {
                sb.AppendLine("\r\n//============================Asserstion Definitions============================");

                foreach (KeyValuePair<string, AssertionBase> entry in AssertionDatabase)
                {
                    sb.AppendLine("#assert " + entry.Key + ";");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clear the information in the specification.
        /// </summary>
        public void ClearDatabase()
        {
            //HasFairEvent = false;
            DeclarationDatabase.Clear();
            DefinitionDatabase.Clear();
            AssertionDatabase.Clear();
            ChannelDatabase.Clear();
        }
        /// <summary>
        /// Return all process definition names in the model, whose number of parameter is 0. Used by 
        /// the simulator find out all processes that can be simulated.
        /// </summary>
        /// <returns></returns>
        public override List<string> GetProcessNames()
        {
            List<string> sb = new List<string>(DefinitionDatabase.Count);

            foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
            {
                if (pair.Value.Parameters.Length == 0)
                {
                    sb.Add(pair.Key);
                }
            }

            sb.Sort();
            return sb;
        }

        public override List<string> GetAllProcessNames()
        {
            List<string> sb = new List<string>(DefinitionDatabase.Count);

            foreach (KeyValuePair<string, Definition> pair in DefinitionDatabase)
            {
                sb.Add(pair.Key);
            }

            sb.Sort();
            return sb;
        }

        public override List<string> GetGlobalVarNames()
        {
            List<string> sb = new List<string>();

            if (SpecValuation.Variables != null)
            {
                foreach (StringDictionaryEntryWithKey<ExpressionValue> pair in SpecValuation.Variables._entries)
                {
                    if (pair != null)
                    {
                        sb.Add(pair.Key);
                    }
                }
            }

            sb.Sort();
            return sb;
        }

        /// <summary>
        /// Return all channel names in the model, used by the intellesense
        /// </summary>
        /// <returns></returns>
        public override List<string> GetChannelNames()
        {
            List<string> sb = new List<string>(this.ChannelDatabase.Count);

            foreach (KeyValuePair<string, ChannelQueue> pair in ChannelDatabase)
            {
                sb.Add(pair.Key);
            }

            sb.Sort();
            return sb;
        }

        public List<string> GetChannelNames(List<string> channels)
        {
            List<string> sb = new List<string>();
            foreach (string channel in channels)
            {
                if (ChannelArrayDatabase.ContainsKey(channel))
                {
                    int size = ChannelArrayDatabase[channel];
                    for (int i = 0; i < size; i++)
                    {
                        sb.Add(channel + "[" + i + "]");
                    }
                }
                else
                {
                    sb.Add(channel);
                }
            }

            return sb;
        }

        public override string[] GetParameterNames(string process)
        {
            if (DefinitionDatabase.ContainsKey(process))
            {
                return DefinitionDatabase[process].Parameters;
            }
            else
            {
                return null;
            }
        }

        public Process GetProcess(string name)
        {
            if (DefinitionDatabase.ContainsKey(name))
            {
                return DefinitionDatabase[name].Process;
            }
            else
            {
                return null;
            }
        }



        public override void LockSpecificationData()
        {
            DataStore.DataManager = this.SharedData.DataManager;
            this.SharedData.LockSpecificationData();
        }

        /// <summary>
        /// this variable is used as trick to control what to be returned in calling GetAlphabets method of the processes
        /// CollectDataOperationEvent == null: the normal calculation
        /// CollectDataOperationEvent == true: only collect the event name (without componend events parts) of data operations
        /// CollectDataOperationEvent == false: nly collect the event name (without componend events parts) of event prefix
        /// </summary>
        public static bool? CollectDataOperationEvent = null;

        public void CheckingConflictingEventsAndVariables()
        {

            foreach (IndexParallel parallel in ParallelDatabase)
            {
                try
                {
                    if (parallel.IndexedProcessDefinition != null)
                    {
                        CollectDataOperationEvent = false;
                        HashSet<string> events =
                            parallel.IndexedProcessDefinition.Process.GetAlphabets(new Dictionary<string, string>());

                        CollectDataOperationEvent = true;
                        HashSet<string> dataOpts =
                            parallel.IndexedProcessDefinition.Process.GetAlphabets(new Dictionary<string, string>());


                        foreach (string opt in dataOpts)
                        {
                            if (events.Contains(opt))
                            {
                                string s = "Data operation \"" + opt + "\" in process " +
                                                parallel.IndexedProcessDefinition.Process +
                                           " will not synchronized with the event \"" + opt + "\" in process " +
                                                parallel.IndexedProcessDefinition.Process;
                                Warnings.Add(s, new ParsingException(s, 0, 0, opt));
                            }

                            if (dataOpts.Contains(opt))
                            {
                                string s = "Data operation \"" + opt + "\" in process " + parallel.IndexedProcessDefinition.Process + " will not synchronized with the data operation \"" + opt + "\" in process " +
                                           parallel.IndexedProcessDefinition.Process;
                                Warnings.Add(s, new ParsingException(s, 0, 0, opt));
                            }
                        }
                    }
                    else
                    {
                        HashSet<string>[] events = new HashSet<string>[parallel.Processes.Count];
                        HashSet<string>[] dataOpts = new HashSet<string>[parallel.Processes.Count];
                        for (int i = 0; i < parallel.Processes.Count; i++)
                        {
                            //the alphabet returned shall not have duplicates!!!!
                            CollectDataOperationEvent = false;
                            events[i] = parallel.Processes[i].GetAlphabets(new Dictionary<string, string>());

                            CollectDataOperationEvent = true;
                            dataOpts[i] = parallel.Processes[i].GetAlphabets(new Dictionary<string, string>());
                        }

                        for (int i = 0; i < parallel.Processes.Count; i++)
                        {
                            for (int j = 0; j < parallel.Processes.Count; j++)
                            {
                                if (i != j)
                                {
                                    foreach (string opt in dataOpts[i])
                                    {
                                        if (events[j].Contains(opt))
                                        {
                                            string s = "Data operation \"" + opt + "\" in process " + parallel.Processes[i].ToString() + " will not synchronized with the event \"" + opt + "\"  in process " +
                                                       parallel.Processes[j].ToString();
                                            Warnings.Add(s, new ParsingException(s, 0, 0, opt));
                                        }

                                        if (dataOpts[j].Contains(opt))
                                        {
                                            string s = "Data operation \"" + opt + "\" in process " + parallel.Processes[i].ToString() + " will not synchronized with the data operation \"" + opt + "\" in process " +
                                                       parallel.Processes[j].ToString();
                                            Warnings.Add(s, new ParsingException(s, 0, 0, opt));
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                catch (System.Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            CollectDataOperationEvent = null;
        }
    }


    public sealed class SharedDataObjects : SharedDataObjectBase
    {
        public DataStore DataManager;

        public SharedDataObjects()
        {
            DataManager = new DataStore();
        }
    }
}