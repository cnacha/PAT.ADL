using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.Ultility;
using PAT.Common.Classes.SemanticModels.LTS.BDD;
using PAT.Common.Classes.LTS;

namespace PAT.ADL.LTS
{
    public sealed class Skip : Process
    {       
        public Skip() 
        {
            ProcessID = Constants.SKIP;
        }
        
        public override void MoveOneStep(Valuation GlobalEnv, List<Configuration> list)
        {
            System.Diagnostics.Debug.Assert(list.Count == 0);

            list.Add(new Configuration(new Stop(), Constants.TERMINATION, null, GlobalEnv, false));
        }
    
        public override string ToString()
        {
            return "Skip";
        }
    
        public override Process ClearConstant(Dictionary<string, Expression> constMapping)
        {
            return this;
        }

        public override bool IsSkip()
        {
            return true;
        }

#if BDD
        public override int IsBDDEncodable(List<string> calledProcesses)
        {
            return (IsBDDEncodableProp = Constants.BDD_LTS_COMPOSITION_3);
        }

        /// <summary>
        /// termination->Stop
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="lts"></param>
        /// <param name="process2State"></param>
        /// <param name="state2UpdatedParas"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public override void MakeDiscreteTransition(List<Expression> guards, List<Event> events, List<Expression> programBlocks, List<Process> processes, Model model, SymbolicLTS lts)
        {
            guards.Add(null);
            events.Add(new Event(Constants.TERMINATION));
            programBlocks.Add(null);
            processes.Add(new Stop());
        }

        public override AutomataBDD EncodeComposition(BDDEncoder encoder)
        {
            return AutomataBDD.Skip(encoder.model);
        }

        public override void CollectEvent(List<string> allEvents, List<string> calledProcesses)
        {
            allEvents.Add(Constants.TERMINATION);
        }
#endif
    }
}