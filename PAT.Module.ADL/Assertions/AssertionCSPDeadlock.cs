using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PAT.Common.Classes.DataStructure;
using PAT.Common.Classes.LTS;
using System.Text;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.Ultility;
using System;

namespace PAT.ADL.Assertions
{
    public abstract partial class AssertionCSPDeadLock : AssertionBase
    {
        protected bool isNotTerminationTesting;

        protected AssertionCSPDeadLock()
        {
        }

        protected AssertionCSPDeadLock(bool isNontermination)
        {
            isNotTerminationTesting = isNontermination;
        }

        public override string ToString()
        {
            if (isNotTerminationTesting)
            {
                return StartingProcess + " nonterminating";
            }

            return StartingProcess + " deadlockfree";
        }

        /// <summary>
        /// Run the verification and get the result.
        /// </summary>
        /// <returns></returns>
        public override void RunVerification()
        {
            if (SelectedEngineName == Constants.ENGINE_DEPTH_FIRST_SEARCH)
            {
                DFSVerification();
            }
            else
            {
                BFSVerification();
            }
        }

        public void DFSVerification()
        {
            StringHashTable Visited = new StringHashTable(1048576);

            Stack<ConfigurationBase> working = new Stack<ConfigurationBase>(1024);

            Visited.Add(InitialStep.GetID());

            working.Push(InitialStep);
            Stack<int> depthStack = new Stack<int>(1024);
            depthStack.Push(0);

            List<int> depthList = new List<int>(1024);

            String prevCurrent = "";
            String prevLastCounterExample = "";
            do
            {
                if (CancelRequested)
                {
                    this.VerificationOutput.NoOfStates = Visited.Count;
                    return;
                }

                ConfigurationBase current = working.Pop();
                
                int depth = depthStack.Pop();

                if (depth > 0)
                {
                    while (depthList[depthList.Count - 1] >= depth)
                    {
                        
                        int lastIndex = depthList.Count - 1;
                       
                        depthList.RemoveAt(lastIndex);
                        this.VerificationOutput.CounterExampleTrace.RemoveAt(lastIndex);
                    }
                }
               
               this.VerificationOutput.CounterExampleTrace.Add(current);
                IEnumerable<ConfigurationBase> list = current.MakeOneMove();
                this.VerificationOutput.Transitions += list.Count();
          

                if (current.IsDeadLock)
                {
                    //if (isNotTerminationTesting || current.Event != Constants.TERMINATION)
                    //{
                    this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                    this.VerificationOutput.NoOfStates = Visited.Count;
                    return;
                    //}
                }

                depthList.Add(depth);

                //for (int i = list.Length - 1; i >= 0; i--)
                foreach (ConfigurationBase step in list)
                {
                    //ConfigurationBase step = list[i];
                    string stepID = step.GetID();
      

                    if (step.Event == Constants.TERMINATION)
                    {
                        if (isNotTerminationTesting)
                        {
                           
                            this.VerificationOutput.CounterExampleTrace.Add(step);
                            this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                            this.VerificationOutput.NoOfStates = Visited.Count;
                            return;
                        }
                    }
                    else
                    {
                        if (!Visited.ContainsKey(stepID))
                        {
                            
                            Visited.Add(stepID);
                            working.Push(step);
                            depthStack.Push(depth + 1);
                        }
                    }
                }
               

            } while (working.Count > 0);


            this.VerificationOutput.CounterExampleTrace = null;

            if (MustAbstract)
            {
                this.VerificationOutput.VerificationResult = VerificationResultType.UNKNOWN;
            }
            else
            {
                this.VerificationOutput.VerificationResult = VerificationResultType.VALID;
            }

            this.VerificationOutput.NoOfStates = Visited.Count;
        }

       

        public void BFSVerification()
        {
            StringHashTable Visited = new StringHashTable(1048576);

            Queue<ConfigurationBase> working = new Queue<ConfigurationBase>(1024);
            Queue<List<ConfigurationBase>> paths = new Queue<List<ConfigurationBase>>(1024);

            Visited.Add(InitialStep.GetID());

            working.Enqueue(InitialStep);
            List<ConfigurationBase> path = new List<ConfigurationBase>();
            path.Add(InitialStep);
            paths.Enqueue(path);

            do
            {
                if (CancelRequested)
                {
                    VerificationOutput.NoOfStates = Visited.Count;
                    return;
                }

                ConfigurationBase current = working.Dequeue();
                List<ConfigurationBase> currentPath = paths.Dequeue();
                IEnumerable<ConfigurationBase> list = current.MakeOneMove();
                this.VerificationOutput.Transitions += list.Count();

                Debug.Assert(currentPath[currentPath.Count - 1].GetID() == current.GetID());

                //If the current process is deadlocked, return true if the current BA state is accepting. Otherwise, return false;
                if (current.IsDeadLock)
                {
                    //if (this.isNotTerminationTesting || current.Event != Constants.TERMINATION)
                    //{
                    this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                    this.VerificationOutput.NoOfStates = Visited.Count;
                    this.VerificationOutput.CounterExampleTrace = currentPath;
                    return;
                    //}
                }

                //for (int i = list.Length - 1; i >= 0; i--)
                foreach (ConfigurationBase step in list)
                {
                    //ConfigurationBase step = list[i];
                    string stepID = step.GetID();

                    if (step.Event == Constants.TERMINATION)
                    {
                        if (isNotTerminationTesting)
                        {
                            this.VerificationOutput.CounterExampleTrace.Add(step);
                            this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                            this.VerificationOutput.NoOfStates = Visited.Count;
                            return;
                        }
                    }
                    else
                    {
                        if (!Visited.ContainsKey(stepID))
                        {
                            Visited.Add(stepID);
                            working.Enqueue(step);

                            List<ConfigurationBase> newPath = new List<ConfigurationBase>(currentPath);
                            newPath.Add(step);
                            paths.Enqueue(newPath);
                        }
                    }
                }
            } while (working.Count > 0);

            this.VerificationOutput.CounterExampleTrace = null;
            if (MustAbstract)
            {
                VerificationOutput.VerificationResult = VerificationResultType.UNKNOWN;
            }
            else
            {
                VerificationOutput.VerificationResult = VerificationResultType.VALID;
            }

            VerificationOutput.NoOfStates = Visited.Count;
        }

        public override string GetResultString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Constants.VERFICATION_RESULT_STRING);
            if (VerificationOutput.VerificationResult == VerificationResultType.VALID)
            {
                sb.AppendLine("The Assertion (" + ToString() + ") is VALID.");
            }
            else if (VerificationOutput.VerificationResult == VerificationResultType.UNKNOWN)
            {
                sb.AppendLine("The Assertion (" + ToString() + ") is NEITHER PROVED NOR DISPROVED.");
            }
            else
            {
                sb.AppendLine("The Assertion (" + ToString() + ") is NOT valid.");
                if (isNotTerminationTesting)
                {
                    sb.AppendLine("The following trace leads to a terminating situation.");
                }
                else
                {
                    sb.AppendLine("The following trace leads to a deadlock situation.");
                }

                VerificationOutput.GetCounterxampleString(sb);
            }

            sb.AppendLine();

            sb.AppendLine("********Verification Setting********");
            sb.AppendLine("Admissible Behavior: " + SelectedBahaviorName);
            sb.AppendLine("Search Engine: " + SelectedEngineName);
            sb.AppendLine("System Abstraction: " + MustAbstract);
            sb.AppendLine();

            return sb.ToString();
        }
    }
}