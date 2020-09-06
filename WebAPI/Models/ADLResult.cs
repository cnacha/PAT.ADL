using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class ADLResult
    {
        public string model { get; set; }
        public string smell { get; set; }
        public string result { get; set; }
        public long visitedStates { get; set; }
        public double verificationTime { get; set; }
        public string fullResultString { get; set; }
    }
}