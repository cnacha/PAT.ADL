using System;
using System.Collections.Generic;
using System.Text;
using ADLParser;
using ADLParser.Classes;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using PAT.ADL.LTS.ADL_Parser;
using static PAT.ADL.LTS.ADL_Parser.ADLParser;

namespace ADLCompiler
{
    class ADLVisitor : ADLBaseVisitor<Object>
    {
        public Dictionary<String, List<String>> components = new Dictionary<String, List<String>>();
        public Dictionary<String, List<String>> connectors = new Dictionary<String, List<String>>();
        

        public override object VisitArchelement([NotNull] ArchelementContext context)
        {
       //     Console.WriteLine("VisitArchelement ");
            if (context.component() != null)
            {
                // generate component
           //     Console.WriteLine("component found!");
                return VisitComponent(context);

            }
            else if (context.connector() != null)
            {
                // generate connector
            //    Console.WriteLine("connector found!");
                return VisitConnector(context);
            }
            else if (context.system() != null)
            {
                // generate connector
             //   Console.WriteLine("system found!");
                return VisitSystem(context);
            }
            else
            {
                return null;
            }
        }

       

       public Connector VisitConnector([NotNull] ArchelementContext context)
       {
           if (context.connector().ID() == null)
               return null;
           Connector connector = new Connector(context.connector().ID().GetText());
           if (context.feature() != null)
           {
               // parsing each role
               FeatureContext[] roles = context.feature();
               foreach (var ctx in roles)
               {
                   Feature ftr = (Feature)Visit(ctx);
                   if (ftr != null)
                   {
                          connector.roleList.Add(ftr);
                   }
               }
           }
           return connector;
       }

       public Component VisitComponent([NotNull] ArchelementContext context)
       {
           if (context.component().ID() == null)
               return null;

           Component comp = new Component(context.component().ID().GetText());
            if (context.feature() != null)
            {
                // parsing each port
                FeatureContext[] ports = context.feature();
                foreach (var ctx in ports)
                {
                    Feature port = (Feature)Visit(ctx);
                    if (port != null)
                    {
                        comp.portList.Add(port);
                    }
                }
            }
            return comp;
       }

        public override object VisitAssertion([NotNull] AssertionContext context)
        {
            AssertionExpr assert = new AssertionExpr(context.ID().GetText());
            if (context.verification().DEADLOCKFREE() != null)
            {
                assert.Type = AssertionExpr.AssertionType.deadlockfree;
            }
            else if (context.verification().LTL() != null)
                assert.Type = AssertionExpr.AssertionType.LTL;

            return assert;
        }

        public override object VisitFeature([NotNull] FeatureContext context)
        {
            
            Feature feature;
            if (context.port() != null)
            {
         //       Console.WriteLine("Visit port: " + context.port().ID());
                feature = new Feature(context.port().ID().GetText());
               
            }
            else if (context.role() != null)
            {
         //       Console.WriteLine("Visit role: " + context.role().ID());
                feature = new Feature(context.role().ID().GetText());
               
            } else if(context.glue() != null)
            {
             //   Console.WriteLine("Visit glue............. ");
                feature = new Feature("glue");
               // feature.process.Add((SysProcess)VisitGlue(context.glue()));
            }
           
            else
            {
                return null;
            }


            if (context.sequence() != null)
            {
         //       Console.WriteLine("    event: " + printEvent(context.sequence().@event()));
                feature.process.AddRange(this.parseSequence(context.sequence().@event()));
            }
            // parsing parameter of process Role
            if (context.paramdefs() != null
                && context.paramdefs().ID() != null
                && context.paramdefs().ID().Length > 0)
            {
                // Console.WriteLine("   params: " + context.paramdefs().ID()[0]);
                foreach (var i in context.paramdefs().ID())
                {
                    feature.Params.Add(i.GetText());
                }
            }
            return feature;
           
        }


        public override object VisitGlue([NotNull] GlueContext context)
        {
            SysProcess prev = null;
            SysProcess first = null;
         //   Console.WriteLine("Visit Glue "+context.processexpr().Length );
            SysProcess procObj = (SysProcess)VisitProcess(context.process());
            first = procObj;
            foreach (var expr in context.processexpr())
            {
        //        Console.WriteLine(expr.process().ID());
                
                SysProcess next = (SysProcess)VisitProcess(expr.process());
                procObj.next = next;

                procObj = next;
                
            }
            return first;
        }

        public override object VisitProcess([NotNull] ProcessContext context)
        {
            SysProcess proc = null;
       //     Console.WriteLine("...............Visit process: " + context.ID()[0]+" "+context.ID().Length);
            if (context.ID().Length >1)
                proc = new SysProcess(context.ID()[0].GetText(), context.ID()[1].GetText());
            else
                 proc = new SysProcess(context.ID()[0].GetText());
            if ((context.paramdefs() != null)
                && (context.paramdefs().ID() != null)
                && (context.paramdefs().ID().Length > 0))
            {
                foreach (var param in context.paramdefs().ID())
                {
                    proc.Parameters.Add(param.GetText());
                }
            }
            return proc;
        }

        public object VisitSystem([NotNull] ArchelementContext context)
        {
       //     Console.WriteLine("VisitSystem: "+ context.system().ID());
            SystemConfig systemCfg = new SystemConfig(context.system().ID().GetText());
            if (context.feature() != null)
            {
                // parsing each define
                FeatureContext[] defines = context.feature();
                foreach (var ctx in defines)
                {
                    // visit define
                    if (ctx.define() != null)
                    {
                        ConfigDeclaration define = (ConfigDeclaration)VisitDefine(ctx.define());
                        if (define != null)
                        {
                            systemCfg.defineList.Add(define);
                        }

                    // visit attach
                    } else if (ctx.attach()!= null)
                    {
                        Attachment attach = (Attachment)VisitAttach(ctx.attach());
                        if (attach != null)
                        {
                            systemCfg.attachList.Add(attach);
                        }

                    // visit glue
                    } else if (ctx.glue() != null) {
                        systemCfg.Glue = (SysProcess)VisitGlue(ctx.glue());

                    // visit link
                    } else if(ctx.link() != null)
                    {
                        systemCfg.linkList.Add((Linkage)VisitLink(ctx.link()));
                    }
                    

                }
                // parsing each attach
            }
            return systemCfg;
        }

        public override object VisitLink([NotNull] LinkContext context)
        {
            SysProcess leftProc = (SysProcess)VisitProcess(context.process()[0]);
            SysProcess rightProc = (SysProcess)VisitProcess(context.process()[1]);
            Linkage link = new Linkage(leftProc, rightProc);
            return link;
        }

        public override object VisitDefine([NotNull] DefineContext context)
        {
       //     Console.WriteLine("VisitDefine: "+ context.ID()[0] + "   "+ context.ID()[1]) ;
            ConfigDeclaration declare = new ConfigDeclaration(context.ID()[0].GetText(), context.ID()[1].GetText());
            return declare;
        }

        public override object VisitAttach([NotNull] AttachContext context)
        {
      //      Console.WriteLine("VisitAttach: " + context.process()[0].ID() + "   " + context.process()[1].ID());
            
            SysProcess procObj = (SysProcess)VisitProcess(context.process()[1]);
            SysProcess first = procObj;
            foreach (var expr in context.processexpr())
            {
      //          Console.WriteLine(expr.process().ID());

                SysProcess next = (SysProcess)VisitProcess(expr.process());
                procObj.next = next;

                procObj = next;

            }
            Attachment attach = new Attachment(
                context.process()[0].ID()[0].GetText()
                , (SysProcess)VisitProcess(context.process()[0])
                , first);
            return attach;
        }

        private List<SysEvent> parseSequence(EventContext[] any)
        {
            List<SysEvent> sequence = new List<SysEvent>();
            foreach (EventContext i in any)
            {
                if (i.process() != null)
                {
                    // event is process such as proc()
                    sequence.Add((SysProcess)VisitProcess(i.process()));
                }
                else if (i.channel() != null)
                {
                    // event is channel input or output
                    SysChannel channel = null;
                    if ((i.channel().channelOutput() != null) 
                        && (i.channel().channelOutput().ID() != null))
                    {
                        channel = new SysChannel(i.channel().ID().GetText(), SysChannel.Type.Output);
                        channel.Parameters.Add(i.channel().channelOutput().ID().GetText());
                    } else if ((i.channel().channelInput() != null) 
                        && (i.channel().channelInput().ID() != null))
                    {
                        channel = new SysChannel(i.channel().ID().GetText(), SysChannel.Type.Input);
                        channel.Parameters.Add(i.channel().channelInput().ID().GetText());
                    }
                    sequence.Add(channel);
                }

            }
            return sequence;
        }

        private String printEvent(EventContext[] any)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach(EventContext i in any)
            {
                if (i.process() != null)
                {
                    sb.Append(count + "" + i.process().ID());
                    if ((i.process().paramdefs() != null) && (i.process().paramdefs().ID() != null) && (i.process().paramdefs().ID().Length > 0))
                        sb.Append("(" + i.process().paramdefs().ID()[0] + ")");
                    sb.Append(((count < any.Length - 1) ? "->" : ""));
                } else if (i.channel() != null)
                {
                    sb.Append(count + "ch_" + i.channel().ID());
                    if ((i.channel().channelOutput() != null) && (i.channel().channelOutput().ID() != null) )
                        sb.Append("(out:" + i.channel().channelOutput().ID() + ")");
                    else if ((i.channel().channelInput() != null) && (i.channel().channelInput().ID() != null))
                        sb.Append("(in:" + i.channel().channelInput().ID() + ")");
                    sb.Append(((count < any.Length - 1) ? "->" : ""));
                }
               
                count++;
            }
            return sb.ToString();
        }

        
    }
}
