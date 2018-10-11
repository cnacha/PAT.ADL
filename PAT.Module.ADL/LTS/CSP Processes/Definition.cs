using System.Collections.Generic;
using System.Text;
using Antlr.Runtime;
using PAT.Common;
using PAT.Common.Classes.DataStructure;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;

namespace PAT.ADL.LTS
{
    public sealed class Definition
    {
        public string Name;
        public ParsingException DefinitionToken;
        public Process Process;
        public string[] Parameters;
        public StringDictionary<int> ParameterLowerBound;
        public StringDictionary<int> ParameterUpperLowerBound;

        public Expression Size;

        //=========================Fileds for Static Analysis================================
        public List<Definition> SubDefinitions;
        public List<string> SubDefinitionNames;

        public List<string> GlobalVars;
        public List<string> Channels;
        public bool MustAbstract;

        public EventCollection AlphabetEvents;
        public HashSet<string> Alphabets;

        //after static analysis, AlphabetsCalculable is true if and only if Alphabets are not null.
        public bool AlphabetsCalculable;
        public IToken Token;
        //=========================Fileds for Static Analysis================================

        public Definition(string name, string[] vars, Process process)
        {
            Name = name;
            Process = process;
            Parameters = vars;

            SubDefinitions = new List<Definition>();
            SubDefinitionNames = new List<string>();

            Channels = new List<string>();
            GlobalVars = new List<string>();

            AlphabetsCalculable = true;
            Alphabets = new HashSet<string>();

            ParameterLowerBound = new StringDictionary<int>(8);
            ParameterUpperLowerBound = new StringDictionary<int>(8);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("====================================");
            sb.AppendLine("Definition: " + Name);
            foreach(var i in GlobalVars)
            {
                sb.AppendLine("   Global Vars " + i);
            }
            foreach (var i in Channels)
            {
                sb.AppendLine("   Channel " + i);
            }
            foreach (var i in SubDefinitions)
            {
                sb.AppendLine("   Subdefinition " + i.ToString());
            }
            foreach (var i in Parameters)
            {
                sb.AppendLine("   Parameters " + i.ToString());
            }
            sb.AppendLine("   Process " + Process.ToString());
            return sb.ToString();
        }

        public string GetFullDefinition()
        {
            return Name + "(" + Common.Classes.Ultility.Ultility.PPStringList(Parameters) + ")=\r\n" + Process + ";";
        }

        public Definition ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Process = Process.ClearConstant(constMapping);

            return this;
        }

#if BDD
        public Definition Rename(Dictionary<string, Expression> constMapping, Dictionary<string, string> newDefNames, Dictionary<string, Definition> renamedProcesses, List<Expression> arguments)
        {
            Dictionary<string, Expression> constMappingTemp = new Dictionary<string, Expression>();

            //P(i) = a -> Q(i)
            //Q(j) = b -> Stop then j in Q(j) receive value i
            for (int i = 0; i < this.Parameters.Length; i++)
            {
                string paraName = this.Parameters[i];
                constMappingTemp[paraName] = (constMapping.ContainsKey(arguments[i].ToString()))? constMapping[arguments[i].ToString()]: arguments[i];
            }

            Definition newDef = new Definition(this.Name, null, null);

            //Copy alphabets
            newDef.AlphabetsCalculable = this.AlphabetsCalculable;
            newDef.Alphabets = this.Alphabets;

            //set the hold-place
            renamedProcesses[this.Name] = newDef;

            newDef.Process = this.Process.Rename(constMappingTemp, newDefNames, renamedProcesses);

            //each Def should be rename
            renamedProcesses.Remove(this.Name);

            return newDef;
        }
#endif

        /// <summary>
        /// To Perform the static analysis on a single definition first.
        /// </summary>
        public void StaticAnalysis()
        {
            MustAbstract = Process.MustBeAbstracted();

            Channels = Process.GetChannels();

            GlobalVars = Process.GetGlobalVariables();

            foreach (string parameter in Parameters)
            {
                GlobalVars.Remove(parameter);
            }

            //IsBDDEncodable = Process.IsBDDEncodable(new List<string>());

            if (AlphabetEvents != null)
            {
                if (AlphabetEvents.ContainsVariable())
                {
                    AlphabetsCalculable = false;
                    Alphabets = null;                    
                }
                else
                {
                    Alphabets = new HashSet<string>(new EventCollection(AlphabetEvents).EventNames);
                    AlphabetsCalculable = true;                    
                }
            }
            else
            {
                if (AlphabetsCalculable)
                {
                    //to check why is null here? forget the reason le.
                    Alphabets = Process.GetAlphabets(null);
                }
            }            
        }
    }
}