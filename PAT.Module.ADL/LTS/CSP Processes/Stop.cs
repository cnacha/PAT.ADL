using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;

namespace PAT.ADL.LTS
{
    public sealed class Stop : Process
    {
        public Stop()
        {
            ProcessID = Constants.STOP;
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);
        }

        public override string ToString()
        {
            return "Stop";
        }
       
        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            return this;
        }
#if BDD
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            return (IsBDDEncodableProp = Constants.BDD_LTS_COMPOSITION_3);
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            return AutomataBDD.Stop();
        }
#endif
    }
}