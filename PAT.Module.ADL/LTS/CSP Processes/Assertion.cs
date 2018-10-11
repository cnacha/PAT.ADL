using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.Ultility;
using PAT.ADL.Assertions;

namespace PAT.ADL.LTS
{
    public sealed class Assertion : Process
    {
        public Expression ConditionalExpression;
        public int LineNumber;

        public Assertion(Expression conditionExpression, int line)
        {
            ConditionalExpression = conditionExpression;
            LineNumber = line;
            ProcessID = DataStore.DataManager.InitializeProcessID(Constants.ASSERTION + conditionExpression.ExpressionID);
        }

        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            ExpressionValue v = EvaluatorDenotational.Evaluate(ConditionalExpression, GlobalEnv);

            if ((v as BoolConstant).Value)
            {
                list.Add(new Configuration(new Stop(), Constants.TERMINATION, null, GlobalEnv, false));
            }
            else
            {
                throw new RuntimeException("Assertion at line " + LineNumber + " failed: " + ConditionalExpression.ToString());
            }
        }

        public override string ToString()
        {
            return " assert(" + ConditionalExpression + ")";
        }

        public override List<string> GetGlobalVariables()
        {
            return ConditionalExpression.GetVars();
        }

        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            Expression newCon = ConditionalExpression.ClearConstant(constMapping);
            return new Assertion(newCon, LineNumber);
        }
    }
}