using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using PAT.Common;
using PAT.Common.Classes.LTS;
using PAT.Common.Classes.ModuleInterface;
using PAT.ADL.LTS;
using PAT.ADL.Ultility;

namespace PAT.ADL{
    public sealed class ModuleFacade : ModuleFacadeBase
    {
        public ModuleFacade()
        {
        }

        public override string ModuleName
        {
            get
            {
                return "ADL Model";
            }
        }
        
        public override Image ModuleIcon
        {
            get
            {
				try
				{	
					Assembly myAssembly = Assembly.GetExecutingAssembly();
					Stream myStream = myAssembly.GetManifestResourceStream("PAT.ADL.Whistle-512.png");
					Bitmap image = new Bitmap(myStream);
					return image;
				}
				catch (Exception)
				{
					return Common.Ultility.Ultility.GetModuleImage("Error");
				}
            }
        }


        protected override SpecificationBase InstanciateSpecification(string text, string options, string filePath)
        {
            return new Specification(text, options, filePath);
        }

		//TODO: return the latex string
        //public override string PrintLaTexString()
        //{
        //    return "";
        //}


        #region Templates

        public override List<string> GetTemplateTypes()
        {
            List<string> modelTypes = new List<string>();
            modelTypes.Add("Assertions");
            modelTypes.Add("Others");
            return modelTypes;
        }

        public override SortedList<string, string> GetTemplateNames(string type)
        {
            SortedList<string, string> templates = new SortedList<string, string>();

            if (type == "Assertions")
            {
                templates.Add("Deadlock Checking", "Deadlock Checking");
                templates.Add("LTL Checking", "Linear Temparal Logic (LTL) Checking");
                templates.Add("Reachability Checking", "Reachability Checking");
                templates.Add("Refinement Checking", "Refinement Checking");
            }
            else if (type == "Others")
            {
                templates.Add("Linearizability Checking", "Linearizability Checking");
            }
            return templates;
        }

        public override string GetTemplateModel(string templateName)
        {
            StringBuilder sb = new StringBuilder();
			/*
            if (templateName == "Deadlock Checking")
            {
                sb.AppendLine("P() = a -> Skip;");
                sb.AppendLine("Q() = b -> Q();");

                sb.AppendLine("System() = P() ||| Q();");
                sb.AppendLine("#assert System() deadlockfree;");
            }
            else if (templateName == "LTL Checking")
            {
                sb.AppendLine("P() = a -> Skip;");
                sb.AppendLine("Q() = b -> Q();");

                sb.AppendLine("System() = P() ||| Q();");
                sb.AppendLine("#assert System() |= []<>b;");
            }
            else if (templateName == "Reachability Checking")
            {
                sb.AppendLine("var x = 0;");

                sb.AppendLine("P(i) = a -> Skip;");
                sb.AppendLine("Q(i) = b.i{x=i;} -> Q(i+1);");

                sb.AppendLine("System() = P(0) ||| Q(0);");

                sb.AppendLine("#define goal x == 10;");
                sb.AppendLine("#assert System() reaches goal;");
            }
            else if (templateName == "Refinement Checking")
            {
                sb.AppendLine("////////////////The Model//////////////////");
                sb.AppendLine("#define N 2;");

                sb.AppendLine("Phil(i) = get.i.(i+1)%N -> get.i.i -> eat.i -> put.i.(i+1)%N -> put.i.i -> Phil(i);");
                sb.AppendLine("Fork(x) = get.x.x -> put.x.x -> Fork(x) [] get.(x-1)%N.x -> put.(x-1)%N.x -> Fork(x);");
                sb.AppendLine("College() = ||x:{0..N-1}@(Phil(x)||Fork(x));");
                sb.AppendLine("Implementation() = College() \\ {get.0.0,get.0.1,put.0.0,put.0.1,eat.1,get.1.1,get.1.0,put.1.1,put.1.0};");

                sb.AppendLine("Specification() = eat.0 -> Specification();");

                sb.AppendLine("////////////////The Properties//////////////////");
                sb.AppendLine("#assert Implementation() refines Specification();");
                sb.AppendLine("#assert Specification() refines Implementation();");
                sb.AppendLine("#assert Implementation() refines <F> Specification();");
                sb.AppendLine("#assert Specification() refines <F> Implementation();");
                sb.AppendLine("#assert Implementation() refines <FD> Specification();");
                sb.AppendLine("#assert Specification() refines <FD> Implementation();");
            }
            else if (templateName == "Linearizability Checking")
            {
                sb.AppendLine("////number of processes");
                sb.AppendLine("#define N 2;");
                sb.AppendLine("//stack size");
                sb.AppendLine("#define SIZE 2;");

                sb.AppendLine("//shared head pointer for the concrete implementation");
                sb.AppendLine("var H = 0;");
                sb.AppendLine("//local variable to store the temporary head value");
                sb.AppendLine("var HL[N];");

                sb.AppendLine("//shared head pointer for the abstract implementation");
                sb.AppendLine("var HA = 0;");
                sb.AppendLine("//local variable to store the temporary head value");
                sb.AppendLine("var HLA[N];");

                sb.AppendLine("////////////////The Concrete Implementation Model//////////////////");
                sb.AppendLine("PushLoop(i) = headread.i{HL[i]=H;} -> (");
                sb.AppendLine("	if (HL[i] == H) {");
                sb.AppendLine("		push.i{if(H < SIZE) {H = H+1;} HL[i]=H;} -> t -> push_res.i.HL[i] -> Skip");
                sb.AppendLine("	} else {");
                sb.AppendLine("		PushLoop(i)");
                sb.AppendLine("	});");

                sb.AppendLine("PopLoop(i) = headread.i{HL[i]=H;} -> ");
                sb.AppendLine("	(if(HL[i] == 0) {");
                sb.AppendLine("		t -> pop_res.i.0 -> Skip ");
                sb.AppendLine("	} else {");
                sb.AppendLine("		t-> (if(HL[i] != H) { PopLoop(i) } else { pop.i{H = H-1; HL[i]=H;} -> t -> pop_res.i.(HL[i]+1) -> Skip");
                sb.AppendLine("		})");
                sb.AppendLine("	});");

                sb.AppendLine("Process(i) = (push_inv.i -> PushLoop(i)[] pop_inv.i -> PopLoop(i));Process(i);");
                sb.AppendLine("Stack() = (|||x:{0..N-1}@Process(x)) \\ {headread.0, push.0, pop.0, headread.1, push.1, pop.1, t};");

                sb.AppendLine("////////////////The Abstract Specification Model//////////////////");
                sb.AppendLine("PushAbs(i) = push_inv.i -> push.i{if(HA < SIZE) {HA = HA+1;}; HLA[i]=HA;} -> push_res.i.HLA[i] -> Skip;");

                sb.AppendLine("PopAbs(i) = pop_inv.i ->");
                sb.AppendLine("	(if(HA == 0) {");
                sb.AppendLine("		pop_empty.i-> pop_res.i.0 -> Skip ");
                sb.AppendLine("	} else {");
                sb.AppendLine("		pop.i{HA = HA -1; HLA[i]=HA;} -> pop_res.i.(HLA[i]+1) -> Skip");
                sb.AppendLine("	});");

                sb.AppendLine("ProcessAbs(i) = (PushAbs(i)[]PopAbs(i));ProcessAbs(i);");

                sb.AppendLine("StackAbs() = (|||x:{0..N-1}@ProcessAbs(x)) \\{push.0, pop.0, pop_empty.0, push.1, pop.1, pop_empty.1};");

                sb.AppendLine("////////////////The Properties//////////////////");
                sb.AppendLine("#assert Stack() refines StackAbs();");
                sb.AppendLine("#assert StackAbs() refines Stack();");
            }
            */
            return sb.ToString();
        }

        #endregion

      
       

    }
}
