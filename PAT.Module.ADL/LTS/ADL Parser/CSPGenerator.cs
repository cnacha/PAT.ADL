using ADLParser.Classes;
using PAT.ADL.Assertions;
using PAT.Common.Classes.BA;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.ModuleInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PAT.ADL.LTS.ADL_Parser
{
    class CSPGenerator
    {
        private Specification Spec;
        public CSPGenerator(Specification spec)
        {
            this.Spec = spec;
        }

        private void createRoleProcessSpec(Feature role, Feature linkedRole, String eventPrefixStr)
        {
            if (Spec.DefinitionDatabase.ContainsKey(role.getName()))
                return;
            //Feature role = conn.getRoleByName(roleName);

            // create parameter of role process
            List<Expression> paramsExpr = new List<Expression>();
            if (role.Params.Count > 0)
            {
                foreach (var param in role.Params)
                {
                    paramsExpr.Add(new Variable(param));
                }
            }
            // create role process
            DefinitionRef process = new DefinitionRef(role.getName(), paramsExpr.ToArray());
            Process prev = null;
            Console.WriteLine(role.process.ElementAt(role.process.Count - 1).Name +"===="+ role.getName());
            if (role.process.ElementAt(role.process.Count - 1).getName() == role.getName())
            {
                prev = process;
            }
            else if (role.process.ElementAt(role.process.Count - 1).Name.IndexOf("Skip")!=-1)
            {
                prev = new Skip();
            }
            else if (role.process.ElementAt(role.process.Count - 1).Name.IndexOf("Stop") != -1)
            {
                prev = new Stop();
            }
            // copy list of event from the role process
            List<SysEvent> roleProcess = new List<SysEvent>();
            roleProcess.AddRange(role.process);

            // intercept if there is a link to other role
            if (linkedRole!=null)
            {
                for (int i = 0; i < roleProcess.Count; i++)
                {
                    if (roleProcess.ElementAt(i).Name.IndexOf("_process")!=-1)

                    {
                        // combine process
                       
                        if (linkedRole.process.ElementAt(linkedRole.process.Count - 1).Name == linkedRole.Name)
                            linkedRole.process.RemoveAt(linkedRole.process.Count - 1);
                        roleProcess.InsertRange(i, linkedRole.process);
                        break;
                    }
                }
            }

            // construct a sequenc for the role process
            // start from the second event
            for (int i = roleProcess.Count - 2; i >= 0; i--)
            {
                var sysEvent = roleProcess.ElementAt(i);
                Process current = null;
                if (sysEvent is SysProcess)
                {
                    // it is event
                    Console.WriteLine("prev: "+prev);
                    current = new EventPrefix(new Common.Classes.LTS.Event(eventPrefixStr + "_" + sysEvent.getName()), prev);

                }
                else if (sysEvent is SysChannel)
                {
                    // it is channel
                    SysChannel channel = (SysChannel)sysEvent;
                    // parse channel parameters
                    List<Expression> chparamsExpr = new List<Expression>();
                    if (channel.Parameters.Count > 0)
                    {
                        foreach (var param in channel.Parameters)
                        {
                            chparamsExpr.Add(new Variable(param));
                        }
                    }
                    // add channelqueue to database, if still not exists
                    if (!Spec.ChannelDatabase.ContainsKey(channel.getName()))
                    {
                        ChannelQueue queue = new ChannelQueue(1);
                        Spec.ChannelDatabase.Add(channel.getName(), queue);
                    }
                    if (channel.ChannelType == SysChannel.Type.Input)
                    {
                        current = new ChannelInput(channel.getName(), null, chparamsExpr.ToArray(), prev);
                    }
                    else if (channel.ChannelType == SysChannel.Type.Output)
                    {
                        current = new ChannelOutput(channel.getName(), null, chparamsExpr.ToArray(), prev);
                    }
                }
                prev = current;
            }
            // create process definition
            Definition processDef = new Definition(role.getName(), role.Params.ToArray(), prev);
            process.Def = processDef;

            // add role process to spec
              Console.WriteLine("............ create role process :" + role.getName());
            Spec.DefinitionDatabase.Add(role.getName(), processDef);


        }
/*
        private void createConnectorSpec(Connector conn, Linkage linkage, Connector linkageConn)
        {
            foreach (var role in conn.roleList)
            {
                createRoleProcessSpec(conn, role.Name, linkage, linkageConn);

            }
        }
        */
        private Expression[] convertParamValues(List<String> str)
        {
            List<Expression> paramsExpr = new List<Expression>();
            foreach (var param in str)
            {
                paramsExpr.Add(new IntConstant(Convert.ToInt32(param)));
            }
            return paramsExpr.ToArray();
        }

        public void parse(SystemConfig config)
        {
            Dictionary<string, Connector> connectorInstanceList = new Dictionary<string, Connector>();
            // get connector instance to the dictionary
            foreach (var define in config.declareList)
            {
                Spec.ConnectorDatabase.TryGetValue(define.RightName, out Connector conn);
                if (conn == null)
                    throw new Exception("At define statement, cannot find connector :" + define.RightName);

                // Console.WriteLine(".......... define: " + define.LeftName + ", conn:" + conn.Name);
                Connector connInst = conn.DeepClone();
                connInst.setConfigName(define.LeftName);
                connectorInstanceList.Add(define.LeftName, connInst);
            }

            // create attach process 
            //  Dictionary<string, DefinitionRef> attachMap = new Dictionary<string, DefinitionRef>();
            foreach (var attach in config.attachList)
            {
                if(!Spec.ComponentDatabase.ContainsKey(attach.LeftName) )
                    throw new Exception("No component " + attach.LeftName + " found");

                SysProcess current = attach.HeadFunction;
                SysProcess prevProc = null;
                Feature prevRole = null;
                DefinitionRef atprocRef = null;
                Process headprocRef = null;
                // loop through trail of process of attachment
                while (current != null)
                {
                    atprocRef = new DefinitionRef(current.Name + "_" + current.Super, convertParamValues(current.Parameters));
                    // retrieve connector
                    connectorInstanceList.TryGetValue(current.Super, out Connector conn);

                    if (conn == null)
                        throw new Exception("At attach statement, " + current.Super + " is not defined");

                    //  Console.WriteLine("finding role process " + config.Name + "_" + current.Name + ": " + atprocRef.Args);
                    Feature role = conn.getRoleByName(current.Name);
                    if (headprocRef != null)
                    {
                        List<Process> coProcs = new List<Process>();
                        coProcs.Add(headprocRef);

                        if (current.operation == SysProcess.Operation.Interleave)
                        {
                           
                            this.createRoleProcessSpec(role, null, attach.LeftName);
                            Spec.DefinitionDatabase.TryGetValue(current.Name + "_" + current.Super, out atprocRef.Def);
                            coProcs.Add(atprocRef);
                            IndexInterleave procs = new IndexInterleave(coProcs);
                            headprocRef = procs;
                        }
                        else if(current.operation == SysProcess.Operation.Parallel)
                        {

                            this.createRoleProcessSpec(role, null, attach.LeftName);
                            Spec.DefinitionDatabase.TryGetValue(current.Name + "_" + current.Super, out atprocRef.Def);
                            coProcs.Add(atprocRef);
                            IndexParallel procs = new IndexParallel(coProcs);
                            headprocRef = procs;
                        }
                    
                        else if (current.operation == SysProcess.Operation.Embed)
                        {
                            atprocRef = new DefinitionRef(prevRole.Name + "_" + prevRole.ConfigName, convertParamValues(prevProc.Parameters));
                            // create embed 
                            this.createRoleProcessSpec(prevRole, role.DeepClone<Feature>(), attach.LeftName);
                            Spec.DefinitionDatabase.TryGetValue(prevRole.getName(), out atprocRef.Def);
                            headprocRef = atprocRef;

                        }
                    }
                    else
                    {
                        if(current.next==null || current.next.operation != SysProcess.Operation.Embed)
                            this.createRoleProcessSpec(role, null, attach.LeftName);
                        Spec.DefinitionDatabase.TryGetValue(current.Name + "_" + current.Super, out atprocRef.Def);
                        // first sub process
                        headprocRef = atprocRef;
                    }
                    prevProc = current;
                    prevRole = conn.getRoleByName(current.Name);
                    current = current.next;
                }

                Definition atprocDef = new Definition(attach.LeftName + "_" + attach.LeftFunction.Name, new String[] { }, headprocRef);
                Spec.DefinitionDatabase.Add(atprocDef.Name, atprocDef);
                //   attachMap.Add(atprocRef.Name, atprocRef);
            }

            // find glue statement and create glue process
            SysProcess subproc = config.Glue;
            //loop through each function in the glue process
            Process head = null;
            while (subproc != null)
            {
                // find attachment process according to what is called in glue process
                DefinitionRef atprocRef = new DefinitionRef(subproc.Super + "_" + subproc.Name, new Expression[] { });
                Spec.DefinitionDatabase.TryGetValue(subproc.Super + "_" + subproc.Name, out atprocRef.Def);
                if (atprocRef.Def == null)
                    throw new Exception("At glue statement, cannot find port" + subproc.Name + " on " + subproc.Super);
                // Process atprocRef = def.Process;
                if (head != null)
                {
                    List<Process> coProcs = new List<Process>();
                    coProcs.Add(head);
                    coProcs.Add(atprocRef);
                    if (subproc.operation == SysProcess.Operation.Interleave)
                    {
                        IndexInterleave interleaveProcs = new IndexInterleave(coProcs);
                        head = interleaveProcs;
                    }
                    else if (subproc.operation == SysProcess.Operation.Parallel)
                    {
                        IndexParallel parallelProcs = new IndexParallel(coProcs);
                        head = parallelProcs;
                    }
                    else if (subproc.operation == SysProcess.Operation.Choice)
                    {
                        IndexChoice choiceProcs = new IndexChoice(coProcs);
                        head = choiceProcs;
                    }

                }
                else
                {
                    // first sub process
                    head = atprocRef;
                }

                // go to next process
                subproc = subproc.next;
            }
            // create glue process
            DefinitionRef glueRef = new DefinitionRef(config.Name, new Expression[] { });
            //Console.WriteLine("glue proc: " + head.ToString());
            Definition instanceDef = new Definition(glueRef.Name, new String[] { }, head);

            glueRef.Def = instanceDef;
            Spec.DefinitionDatabase.Add(instanceDef.Name, instanceDef);
            Spec.GlueProcessDatabase.Add(glueRef.Name, glueRef);


        }



        public void AddAssertion(AssertionExpr assertion, string options)
        {
            if (assertion.Type == AssertionExpr.AssertionType.deadlockfree)
            {
                AssertionBase asrt = createDeadlockAssertion(assertion);
                Spec.AssertionDatabase.Add(assertion.Target + "-deadlockfree", asrt);
            }
            else if (assertion.Type == AssertionExpr.AssertionType.circularfree)
            {
                AssertionBase asrt = createCircularAssertion(assertion);
                Spec.AssertionDatabase.Add(assertion.Target + "-circularfree", asrt);
            }
            else if (assertion.Type == AssertionExpr.AssertionType.bottleneckfree)
            {
                AssertionBase asrt = createBottleneckAssertion(assertion);
                Spec.AssertionDatabase.Add(assertion.Target + "-bottleneckfree", asrt);
            }
            else if (assertion.Type == AssertionExpr.AssertionType.LTL)
            {
                AssertionBase asrt = createLTLAssertion(assertion, options);
                Spec.AssertionDatabase.Add(assertion.Target + " " + assertion.Expression, asrt);
            } 
            else if(assertion.Type == AssertionExpr.AssertionType.reachability)
            {
                AssertionBase asrt = createReachability(assertion);
                Spec.AssertionDatabase.Add(assertion.Target + " reaches " + assertion.Expression, asrt);
            }
        }

        private AssertionBase createReachability(AssertionExpr assertion)
        {
            Spec.GlueProcessDatabase.TryGetValue(assertion.Target, out DefinitionRef glueProc);
            ADLAssertionReachability assertionCSP = new ADLAssertionReachability(glueProc, assertion.Expression);
            //Spec.DeclarationDatabase.Add(assertion.Expression, new Expression());
            // TODO: need to revise this to work 
            return assertionCSP;
        }

        private AssertionBase createDeadlockAssertion(AssertionExpr assertion)
        {
            Spec.GlueProcessDatabase.TryGetValue(assertion.Target, out DefinitionRef glueProc);
            // insert assertion 
            ADLAssertionDeadLock assertionCSP = null;
            if (glueProc != null) {
               assertionCSP = new ADLAssertionDeadLock(glueProc);
            }else
                throw new Exception("Unknown target process for assertion");

            return assertionCSP;
        }

        private AssertionBase createBottleneckAssertion(AssertionExpr assertion)
        {
            Spec.GlueProcessDatabase.TryGetValue(assertion.Target, out DefinitionRef glueProc);
            // insert assertion 
            ADLAssertionBottleneck assertionCSP = null;
            if (glueProc != null)
            {
                assertionCSP = new ADLAssertionBottleneck(glueProc);
            }
            else
                throw new Exception("Unknown target process for assertion");

            return assertionCSP;
        }

        private AssertionBase createCircularAssertion(AssertionExpr assertion)
        {
            Spec.GlueProcessDatabase.TryGetValue(assertion.Target, out DefinitionRef glueProc);
            // insert assertion 
            ADLAssertionCircular assertionCSP = null;
            if (glueProc != null)
            {
                assertionCSP = new ADLAssertionCircular(glueProc);
            }
            else
                throw new Exception("Unknown target process for assertion");

            return assertionCSP;
        }

        private AssertionBase createLTLAssertion(AssertionExpr assertion, string options)
        {
            Spec.GlueProcessDatabase.TryGetValue(assertion.Target, out DefinitionRef glueProc);
            ADLAssertionLTL assertLTL = null;
            if (glueProc != null)
            {
                String ltl = assertion.Expression;
                assertLTL = new ADLAssertionLTL(glueProc, ltl.Trim());
                BuchiAutomata PositiveBA = LTL2BA.FormulaToBA(ltl.Trim(), options, null);
                // default to false for x operator U X V T F R  NOT SUPPORTED for now
                bool hasXoperator = false;
                PositiveBA.HasXOperator = hasXoperator;
                if (!LivenessChecking.isLiveness(PositiveBA))
                {
                    assertLTL.SeteBAs(null, PositiveBA);
                }
                else
                {
                    BuchiAutomata BA = LTL2BA.FormulaToBA("!(" + ltl.Trim() + ")", options, null); //.Replace(".", Ultility.Ultility.DOT_PREFIX)      
                    BA.HasXOperator = hasXoperator;
                    assertLTL.SeteBAs(BA, PositiveBA);
                }
            }
            else
            {
                throw new Exception("Unknown target process for assertion");
            }

            return assertLTL;
        }
    }
}
