using ADLParser.Classes;
using PAT.ADL.LTS;
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
    public class ADLAssertionAmbiguosInterface : AssertionBase
    {
        protected bool isNotTerminationTesting;
        private DefinitionRef Process;
        public Dictionary<string, Component> ComponentDatabase = null;

        public ADLAssertionAmbiguosInterface(DefinitionRef processDef): base()
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

            return StartingProcess + " ambiguous interface";
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
            ComponentDatabase.TryGetValue(compName, out Component comp);
            if (comp.portList.Count == 1)
                return true;
            else
                return false;
        }
        string ambiguousInterface = "";
        public void DFSVerification()
        {
            StringHashTable Visited = new StringHashTable(1048576);

            Stack<ConfigurationBase> working = new Stack<ConfigurationBase>(1024);

             Dictionary<string, List<string>> componentInvokeByDict = new Dictionary<string, List<string>>();
            HashSet<string> singleInterfaceComponentInvoked = new HashSet<string>();

            Visited.Add(InitialStep.GetID());

            working.Push(InitialStep);
            Stack<int> depthStack = new Stack<int>(1024);
            depthStack.Push(0);

            List<int> depthList = new List<int>(1024);
            string previousComponent = "";
            int dataSignature = 0;
            bool isOnRequest = false;
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

                        // removing the invoked component from hashset when the counterexample is stepped back.
                        if (this.VerificationOutput.CounterExampleTrace[lastIndex].Event.IndexOf("!")==-1 && this.VerificationOutput.CounterExampleTrace[lastIndex].Event.IndexOf("?") == -1)
                        {
                            string compName = this.VerificationOutput.CounterExampleTrace[lastIndex].Event.Substring(0, this.VerificationOutput.CounterExampleTrace[lastIndex].Event.IndexOf("_"));
                            if (singleInterfaceComponentInvoked.Contains(compName))
                            {
                                Console.WriteLine("     ...removing " + compName);
                                singleInterfaceComponentInvoked.Remove(compName);
                            }
                        }
                        this.VerificationOutput.CounterExampleTrace.RemoveAt(lastIndex);
                        // visitedStates.RemoveAt(lastIndex);
                        
                    }
                }
                
                this.VerificationOutput.CounterExampleTrace.Add(current);
               
                IEnumerable<ConfigurationBase> list = current.MakeOneMove();
                this.VerificationOutput.Transitions += list.Count();
                 Console.WriteLine("tracing event: " + current.Event + " " + current.GetID()+"||");
                Console.WriteLine(toStringCounterExample(this.VerificationOutput.CounterExampleTrace));

               ///////////////////////////////////////////////////////// Code specific for this smell detection
                if(current.Event.IndexOf("!")==-1 && current.Event.IndexOf("?") == -1 && current.Event.IndexOf("_")!=-1)
                {
                    // not channel event, it is component event
                   Console.WriteLine("comp event: "+current.Event);
                    String currentComponent = current.Event.Substring(0,current.Event.IndexOf("_"));
                    // check if the previous calling component and current component are not the same, also calling component has single interface 
                    if (currentComponent != previousComponent && previousComponent != "" && this.IsSingleInterface(previousComponent))
                    {
                        // START add to dict for debuging
                        if (!componentInvokeByDict.ContainsKey(currentComponent))
                        {
                            List<string> componentInvokeList = new List<string>();
                            componentInvokeList.Add(previousComponent);
                            componentInvokeByDict.Add(currentComponent, componentInvokeList);

                        }
                        else
                        {
                            componentInvokeByDict.TryGetValue(currentComponent, out List<string> componentInvokeList);
                            if (!componentInvokeList.Contains(previousComponent))
                            {
                                componentInvokeList.Add(previousComponent);
                                
                                componentInvokeByDict[currentComponent] = componentInvokeList;
                            }
                        }
                        // END add to dict for debuging
                        // actual checking for ambiguous interface
                        if (singleInterfaceComponentInvoked.Contains(previousComponent) && isOnRequest)
                        {
                            // found ambiguous
                            Console.WriteLine("              Ambiguous Interface ********* "+ previousComponent +" "+ currentComponent);
                            this.VerificationOutput.VerificationResult = VerificationResultType.INVALID;
                            this.VerificationOutput.NoOfStates = Visited.Count;
                            ambiguousInterface = previousComponent;
                            PrintComponentInvokeDict(componentInvokeByDict);
                            return;
                        }
                        else if(isOnRequest)
                        {
                            singleInterfaceComponentInvoked.Add(previousComponent);
                        }
                    }
                    Console.Write(" compInvoked->");
                    PrintList(singleInterfaceComponentInvoked);

                    previousComponent = currentComponent ;
                }
                
                // if it is channel input, capture data signature sending through port
                if (current.Event.IndexOf("req") != -1)
                {
                    isOnRequest = true;
                 //   dataSignature = Convert.ToInt32( current.Event.Substring(current.Event.IndexOf("?") + 1));
                } else if(current.Event.IndexOf("res") != -1)
                {
                    isOnRequest = false;
                }
                   
                foreach(string str in singleInterfaceComponentInvoked)
                {
                    Console.WriteLine(" scompinv "+str);
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

               // PrintComponentInvokeDict(componentInvokeByDict);
        

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
            /*
            // check ambiguous interface which are the components that is invoked by only the same component.
            Dictionary<string, int> ambiguousInterfaceList = new Dictionary<string, int>();
            foreach (string comp in componentInvokeByDict.Keys)
            {
                componentInvokeByDict.TryGetValue(comp, out List<String> compSource);
                if(compSource.Count == 1)
                {
                    if (!ambiguousInterfaceList.ContainsKey(compSource[0]))
                    {
                        ambiguousInterfaceList.
                    }
                    
                }
            }
            */

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

        private void PrintList(HashSet<string> set)
        {
            foreach(string e in set)
            {
                Console.Write(e + " ");
            }
            Console.WriteLine();
        }
        
        private void PrintComponentInvokeDict(Dictionary<string, List<string>> dict)
        {
            Console.WriteLine("============= ComponentInvokeDict=============");
            foreach (string comp in dict.Keys)
            {
                dict.TryGetValue(comp, out List<String> compSource);
                StringBuilder sb = new StringBuilder();
                foreach(string csrc in compSource)
                {
                    sb.Append(csrc+", ");
                }
                
                Console.WriteLine(comp + " = ["+sb.ToString()+"]");
               
            }
            Console.WriteLine("===============================================");
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
                    sb.AppendLine("The following trace leads to a ambiguous interface: "+ ambiguousInterface);
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
