using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            LTL
        }
        public AssertionType Type;
        public string Target;
        public string Expression;

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
