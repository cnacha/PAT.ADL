using PAT.Common.Classes.Assertion;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.ModuleInterface;
using PAT.ADL.LTS;

namespace PAT.ADL.Assertions{
    public class ADLAssertionDeadLock : AssertionCSPDeadLock
    {
        private DefinitionRef Process;
        public ADLAssertionDeadLock(DefinitionRef processDef) : base()
        {
            Process = processDef;
        }

        public ADLAssertionDeadLock(DefinitionRef processDef, bool isNontermination) : base(isNontermination)
        {
            Process = processDef;
        }

        public override void Initialize(SpecificationBase spec)
        {
            //initialize the ModelCheckingOptions
            base.Initialize(spec);
            
            Assertion.Initialize(this, Process, spec);
        }

        public override string StartingProcess
        {
            get
            {
                return Process.ToString();
            }
        }
        
        //todo: override ToString method if your assertion uses different syntax as PAT
        //public override string ToString()
        //{
        //		return "";
        //}
    }
}