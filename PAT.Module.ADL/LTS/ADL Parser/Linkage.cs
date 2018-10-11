using ADLParser.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADLParser.Classes
{
    class Linkage
    {
        public SysProcess LeftProcess;
        public SysProcess RightProcess;

        public Linkage(SysProcess leftProcess, SysProcess rightProcess)
        {
            this.LeftProcess = leftProcess;
            this.RightProcess = rightProcess;
        }

        public override string ToString()
        {
            return "Linkage{ left: " + LeftProcess.ToString() + ", right:" + RightProcess.ToString() + "}";
        }
    }
}
