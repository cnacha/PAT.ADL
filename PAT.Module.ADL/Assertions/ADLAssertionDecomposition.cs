using PAT.ADL.LTS;
using ADLParser.Classes;
using PAT.Common.Classes.DataStructure;
using PAT.Common.Classes.ModuleInterface;
using PAT.Common.Classes.Ultility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PAT.ADL.Assertions
{
    public class ADLAssertionDecomposition: AssertionBase
    {
        protected bool isNotTerminationTesting;
        private DefinitionRef Process;
        public Dictionary<string, Component> ComponentDatabase = null;
        public Dictionary<string, Attachment> AttachmentDatabase { get; internal set; }
        private static int MAX_SEQUENCE_SINGLE_INTERFACE_INVOKE = 3;

        public ADLAssertionDecomposition(DefinitionRef processDef): base()
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

        

        public override string ToString()
        {

            return StartingProcess + " decomposition";
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

        private bool IsSingleInterface(string compName)
        {
            Console.Write("###IsSingleInterface");
            ComponentDatabase.TryGetValue(compName, out Component comp);
            if (comp.portList.Count == 1 )
            {/*
                AttachmentDatabase.TryGetValue(comp.portList[0].Name, out Attachment attachment);
               Boolean isSingleAttch =  attachment.isSingleInterface();
                Console.WriteLine("     ### attached " + comp.portList[0].Name + " issingle#: " + isSingleAttch);
                // find number of attachment
                if (isSingleAttch)
                    return true;
                else
                    return false;
                    */
                return true;
            }
            else
                return false;
        }
        List<string> singleInterfaceInvokeSequence = new List<string>();

        public void DFSVerification()
        {
            StringHashTable Visited = new StringHashTable(1048576);

            Stack<ConfigurationBase> working = new Stack<ConfigurationBase>(1024);

            Visited.Add(InitialStep.GetID());

            working.Push(InitialStep);
            Stack<int> depthStack = new Stack<int>(1024);
            depthStack.Push(0);

            List<int> depthList = new List<int>(1024);
            List<String> visitedStates = new List<String>();
            

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
                Console.Write("tracing event: " + current.Event + " " + current.GetID());
                Console.WriteLine(toStringCounterExample(this.VerificationOutput.CounterExampleTrace)+"\n");

                if (current.Event.IndexOf("!") == -1 && current.Event.IndexOf("?") == -1 && current.Event.IndexOf("_") != -1 && current.Event.IndexOf("consumer_request") == -1)
                {
                    // not channel event, it is component event
                    Console.WriteLine("comp event: " + current.Event);
                    String currentComponent = current.Event.Substring(0, current.Event.IndexOf("_"));
                    if (IsSingleInterface(currentComponent))
                    {
                        if(singleInterfaceInvokeSequence.Count()==0 || singleInterfaceInvokeSequence.Last() != currentComponent)
                        {
                            singleInterfaceInvokeSequence.Add(currentComponent);
                            printComponentList(singleInterfaceInvokeSequence);
                            if (singleInterfaceInvokeSequence.Count() > MAX_SEQUENCE_SINGLE_INTERFACE_INVOKE)
                            {
                                // functional decomposition found
                                Console.WriteLine("              Functional decomposition ********* ");
                                this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                                this.VerificationOutput.NoOfStates = Visited.Count;
                                return;
                            }
                        } 
             
                    } else
                    {
                        singleInterfaceInvokeSequence.Clear();
                    }
                }

                depthList.Add(depth);

                //for (int i = list.Length - 1; i >= 0; i--)
                foreach (ConfigurationBase step in list)
                {
                    //ConfigurationBase step = list[i];
                    string stepID = step.GetID();


                    if (step.Event != Constants.TERMINATION)
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

        private void printComponentList(List<String> component)
        {
            Console.Write("complist:");
            foreach (var s in component)
            {
               
                Console.Write(s +"->");
            }
            Console.WriteLine();
        }

        private Boolean isProcessEventExist(List<String> evtrace, String connectorName)
        {
            foreach(var s in evtrace)
            {
                if (s.IndexOf("process_" + connectorName) != -1)
                    return true;
            }
            return false;
        }

        private String toStringCounterExample(List<ConfigurationBase> counterexample)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in counterexample)
            {


                sb.Append("->" + i.DisplayName + i.Event + "|" + i.GetID());
            }
            return sb.ToString();
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
            List<String> visitedStates = new List<String>();

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

                // track channel input for circular dependency
                if (current.Event.IndexOf("!") != -1 && visitedStates.Contains(current.Event) && !isProcessEventExist(visitedStates, current.Event.Substring(current.Event.LastIndexOf("_") + 1, (current.Event.IndexOf("!") - current.Event.LastIndexOf("_") - 1))))
                {
                    Console.WriteLine("              circular happen *********");
                    this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                    this.VerificationOutput.LoopIndex = visitedStates.IndexOf(current.Event);
                    this.VerificationOutput.NoOfStates = Visited.Count;

                    return;
                }
                visitedStates.Add(current.Event);

                //for (int i = list.Length - 1; i >= 0; i--)
                foreach (ConfigurationBase step in list)
                {
                    //ConfigurationBase step = list[i];
                    string stepID = step.GetID();

                  
                        if (!Visited.ContainsKey(stepID))
                        {
                            Visited.Add(stepID);
                            working.Enqueue(step);

                            List<ConfigurationBase> newPath = new List<ConfigurationBase>(currentPath);
                            newPath.Add(step);
                            paths.Enqueue(newPath);
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
                    sb.Append("The following trace leads to a functional decomposition situation.");
                    foreach(String seq in singleInterfaceInvokeSequence)
                    {
                        sb.Append(seq + " -> ");
                    }
                    sb.AppendLine();
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
