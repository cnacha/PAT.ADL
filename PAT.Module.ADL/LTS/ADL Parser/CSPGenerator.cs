using ADLParser.Classes;
using PAT.ADL.Assertions;
using PAT.Common.Classes.Expressions.ExpressionClass;
using PAT.Common.Classes.LTS;
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


        private void createConnectorSpec(Connector conn, Linkage linkage, Connector linkageConn)
        {
            foreach (var role in conn.roleList)
            {
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
                if (role.process.ElementAt(role.process.Count - 1).getName() == role.getName())
                {
                    prev = process;
                }
                else if (role.process.ElementAt(role.process.Count - 1).getName() == "skip")
                {
                    prev = new Skip();
                }
                // copy list of event from the role process
                List<SysEvent> roleProcess = new List<SysEvent>();
                roleProcess.AddRange(role.process);

                // intercept if there is a link to other role
                if (linkage != null && role.Name == linkage.LeftProcess.Name)
                {
                    for (int i = 0; i < roleProcess.Count; i++)
                    {
                        if (roleProcess.ElementAt(i).Name == "process")
                        {
                            // combine process
                            roleProcess.InsertRange(i, linkageConn.getRoleByName(linkage.RightProcess.Name).process);
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
                        current = new EventPrefix(new Common.Classes.LTS.Event(sysEvent.getName()), prev);

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
              //  Console.WriteLine("............ create role process :" + role.getName());
                Spec.DefinitionDatabase.Add(role.getName(), processDef);

            }
        }
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
            foreach (var define in config.defineList)
            {
                Spec.connectorDatabase.TryGetValue(define.RightName, out Connector conn);
                if (conn == null)
                    throw new Exception("At define statement, cannot find connector :"+define.RightName);

               // Console.WriteLine(".......... define: " + define.LeftName + ", conn:" + conn.Name);
                connectorInstanceList.Add(define.LeftName, conn);
            }
            // create define process by connector
            foreach (var define in config.defineList)
            {
                connectorInstanceList.TryGetValue(define.LeftName, out Connector conn);
                if (conn == null)
                    throw new Exception("At define statement, cannot find connector instance:" + define.LeftName);
                Linkage link = config.FindLinkByInstanceName(define.LeftName);
                Connector linkageConn = null;
                if (link != null)
                {
                    // find linkage connector
                    connectorInstanceList.TryGetValue(link.RightProcess.Super, out linkageConn);
                    if (linkageConn == null)
                        throw new Exception("At link statement, cannot find connector instance:" + link.RightProcess.Super);
                    linkageConn.setConfigName(link.RightProcess.Super);
                }
                // create specification for connector instance
                //conn.setConfigName(define.LeftName);
                conn.setConfigName(define.LeftName);
                createConnectorSpec(conn, link, linkageConn);
            }
            // create attach process 
            //  Dictionary<string, DefinitionRef> attachMap = new Dictionary<string, DefinitionRef>();
            foreach (var attach in config.attachList)
            {
                SysProcess current = attach.HeadFunction;
                DefinitionRef atprocRef = null;
                Process headprocRef = null;
                // loop through trail of process of attachment
                while (current != null)
                {
                    atprocRef = new DefinitionRef(current.Name + "_" + current.Super, convertParamValues(current.Parameters));

                    Spec.DefinitionDatabase.TryGetValue(current.Name + "_" + current.Super, out atprocRef.Def);
                    if (atprocRef.Def == null)
                        throw new Exception("At attach statement, cannot find " + current.Name + " on " + current.Super);
                  //  Console.WriteLine("finding role process " + config.Name + "_" + current.Name + ": " + atprocRef.Args);
                    if (headprocRef != null)
                    {
                        List<Process> coProcs = new List<Process>();
                        coProcs.Add(headprocRef);
                        coProcs.Add(atprocRef);
                        if (current.operation == SysProcess.Operation.Interleave)
                        {
                            IndexInterleave interleaveProcs = new IndexInterleave(coProcs);
                            headprocRef = interleaveProcs;
                        }
                        else if (current.operation == SysProcess.Operation.Choice)
                        {
                            IndexChoice choiceProcs = new IndexChoice(coProcs);
                            headprocRef = choiceProcs;
                        }

                    }
                    else
                    {
                        // first sub process
                        headprocRef = atprocRef;
                    }
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



        public void AddAssertion(AssertionExpr assertion)
        {
            if (assertion.Type == AssertionExpr.AssertionType.deadlockfree)
            {
                Spec.GlueProcessDatabase.TryGetValue(assertion.Target, out DefinitionRef glueProc);
                // insert assertion 
                if (glueProc != null)
                    Spec.AssertionDatabase.Add(assertion.Target + "-deadlockfree", new ADLAssertionDeadLock(glueProc));
                else
                    throw new Exception("Unknown target process for assertion");
            }
        }
    }
}
