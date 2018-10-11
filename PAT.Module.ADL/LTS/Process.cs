using System;
using System.Collections.Generic;
using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;

namespace PAT.ADL.LTS
{
    public abstract class Process : ICloneable //: ProcessBase<Configuration>
    {
        public string ProcessID;

        /// <summary>
        /// returns all the possible moves of the current process
        /// </summary>
        /// <param name="GlobalEnv">The current global valuation</param>
        /// <param name="list">The list of steps to be returned.</param>
        /// A precondition of the method is that "System.Diagnostics.Debug.Assert(list.Count == 0);"
        public abstract void MoveOneStep(Valuation GlobalEnv, List<Configuration> list);

        /// <summary>
        /// Get the set of global variables which may be accessed by this process. Notice that arrays will be flatened (for one level). 
        /// For instance, let leader[3] be an array, leader[0], leader[1] will be listed as two different variables. 
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetGlobalVariables()
        {
            return new List<string>(0);
        }

        /// <summary>
        /// Get the set of relevant channels.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetChannels()
        {
            return new List<string>(0);
        }

        /// <summary>
        /// This method returns true iff it can be encoded using Khanh's BDD library. 
        /// A process can be encoded using Khanh's BDD library iff it is composed of compositions of LTSs.
        /// Notice that a process which contains the following features is not BDD encodable for the moment.
        /// 1. Atomic
        /// 2. external library
        /// 3. |||{..} 
        /// 4. Hiding
        /// </summary>
        /// <returns></returns>
        public virtual bool IsBDDEncodable()
        {
            return true;
        }

        /// <summary>
        /// This method defines the logic for calculating the default alphabet of a process. That is, the set of events consitituting the 
        /// process expression with process reference unfolded ONCE! We found this to be intuitive at the moment.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<string> GetAlphabets(Dictionary<string, string> visitedDefinitionRefs)
        {
            return new HashSet<string>();
        }

        public virtual bool MustBeAbstracted()
        {
            return false;
        }


        /// <summary>
        /// clear global constants and process parameters; This method is the starting point of run-time execution of the process.
        /// </summary>
        /// <param name="constMapping"></param>
        /// <returns></returns>
        public abstract Process ClearConstant(Dictionary<string, Expression> constMapping);

        ///// <summary>
        ///// returns all the possible synchoronous input process on the given channel
        ///// </summary>
        ///// <returns></returns>
        public virtual void SyncInput(ConfigurationWithChannelData eStep, List<Configuration> list) //, string syncChannel, Expression[] values
        {
        }

        ///// <summary>
        ///// returns all the possible synchronous output steps
        ///// </summary>
        ///// <returns></returns>
        public virtual void SyncOutput(Valuation GlobalEnv, List<ConfigurationWithChannelData> list)
        {
        }

        public virtual Process GetTopLevelConcurrency(List<string> visitedDef)
        {
            return null;
        }
        public virtual bool IsSkip()
        {
            return false;
        }
        public virtual object Clone()
        {
            return this.Clone();
        }
    }
}