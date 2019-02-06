using System.Collections.Generic;
using PAT.Common.Classes.Assertion;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.Assertion;
using PAT.ADL.LTS;

namespace PAT.ADL.Assertions{
    public class ADLAssertionLTL : AssertionLTL
    {
        private DefinitionRef Process;

        public ADLAssertionLTL(DefinitionRef processDef, string ltl) : base(ltl)
        {
            Process = processDef;
        }

        public override void Initialize(SpecificationBase spec)
        {
            Specification Spec = spec as Specification;

            List<string> varList = Process.GetGlobalVariables();

            BA.Initialize(Spec.DeclarationDatabase, Spec.SpecValuation);

            foreach (KeyValuePair<string, Expression> pair in BA.DeclarationDatabase)
            {
                varList.AddRange(pair.Value.GetVars());
            }

            Valuation GlobalEnv = Spec.SpecValuation.GetVariableChannelClone(varList, Process.GetChannels());
            InitialStep = new Configuration(Process, Constants.INITIAL_EVENT, null, GlobalEnv, false);

            MustAbstract = Process.MustBeAbstracted();


            base.Initialize(spec);
        }

        public override string StartingProcess
        {
            get
            {
                return Process.ToString();
            }
        }

        protected bool CheckIsProcessLevelFairnessApplicable()
        {
            Process nextProcess = Process.GetTopLevelConcurrency(new List<string>());
            if (MustAbstract)
            {
                if (nextProcess is IndexInterleaveAbstract)
                {
                    IndexInterleaveAbstract interleave = nextProcess as IndexInterleaveAbstract;
                    foreach (Process p in interleave.Processes)
                    {
                        if (p.MustBeAbstracted())
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
            else
            {
                if (nextProcess is IndexInterleave || nextProcess is IndexParallel || nextProcess is IndexInterleaveAbstract)
                {
                    return true;
                }
            }

            return false;
        }
        
        //todo: override ToString method if your assertion uses different syntax as PAT
        //public override string ToString()
        //{
        //		return "";
        //}        
    }
}