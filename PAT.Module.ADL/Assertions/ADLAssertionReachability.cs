using Antlr.Runtime;
using PAT.ADL.LTS;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.SemanticModels.LTS.Assertion;
using PAT.Common.Classes.Ultility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PAT.ADL.Assertions
{
    class ADLAssertionReachability : AssertionReachability
    {
        private DefinitionRef Process;

        public ADLAssertionReachability(DefinitionRef processDef, string reachableState) : base(reachableState)
        {
            Process = processDef;
        }

        public override string StartingProcess
        {
            get
            {
                return Process.ToString();
            }
        }

        public override void Initialize(SpecificationBase spec)
        {
            Specification Spec = spec as Specification;
            ReachableStateCondition = Spec.DeclarationDatabase[ReachableStateLabel];

            PAT.Common.Ultility.ParsingUltility.TestIsBooleanExpression(
                ReachableStateCondition,
                new CommonToken(null, -1, -1, -1, -1),
                " used in the condition \"" + ReachableStateLabel + "\" of reachablity assertion \"" + this.ToString() + "\"",
                Spec.SpecValuation,
                new Dictionary<string, Expression>()
                );

            List<string> varList = Process.GetGlobalVariables();
            varList.AddRange(ReachableStateCondition.GetVars());

            Valuation GlobalEnv = Spec.SpecValuation.GetVariableChannelClone(varList, Spec.GetChannelNames(Process.GetChannels()));

            //Initialize InitialStep
            InitialStep = new Configuration(Process, Constants.INITIAL_EVENT, null, GlobalEnv, false);

            MustAbstract = Process.MustBeAbstracted();

            //base.Initialize(spec);

            //initialize model checking options, the default option is for deadlock/reachablity algorithms
            ModelCheckingOptions = new ModelCheckingOptions();
            List<string> DeadlockEngine = new List<string>();

            if (!(spec as Specification).HasWildVariable)
            {
                DeadlockEngine.Add(Constants.ENGINE_DEPTH_FIRST_SEARCH);
                DeadlockEngine.Add(Constants.ENGINE_BREADTH_FIRST_SEARCH);
            }

            ModelCheckingOptions.AddAddimissibleBehavior(Constants.COMPLETE_BEHAVIOR, DeadlockEngine);
        }
    }
}
