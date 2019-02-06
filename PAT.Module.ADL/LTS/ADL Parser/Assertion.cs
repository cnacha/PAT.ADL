using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PAT.ADL.LTS.ADL_Parser;

namespace ADLParser.Classes
{
    class AssertionExpr
    {
        public enum AssertionType
        {
            deadlockfree,
            circularfree,
            bottleneckfree,
            reachability,
            ambiguousinterface,
            lavaflow,
            decomposition,
            poltergeists,
            LTL
        }
        public AssertionType Type;
        public string Target;
        public string Expression;

        public PAT.ADL.LTS.ADL_Parser.ADLParser.LtlexprContext ExpressionContext { get; internal set; }

        public AssertionExpr(string target)
        {
            this.Target = target;
        }
        public AssertionExpr(AssertionType type, string target)
        {
            this.Type = type;
            this.Target = target;
        }

        public override string ToString()
        {
            return "assertion{type:"+Type+", target:"+Target+",LTLExpr:"+Expression+"}";
        }
    }
}
