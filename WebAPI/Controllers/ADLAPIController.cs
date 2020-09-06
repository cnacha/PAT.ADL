using PAT.Common.Classes.ModuleInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class ADLAPIController : ApiController
    {
        [HttpPost]
        [ActionName("verify")]
        public List<ADLResult> Verify(ADLRequest request)
        {
            System.Diagnostics.Debug.WriteLine("request : \n" +request.code);
            PAT.ADL.ModuleFacade modulebase = new PAT.ADL.ModuleFacade();

            SpecificationBase Spec = modulebase.ParseSpecification(request.code, string.Empty, string.Empty);
            System.Diagnostics.Debug.WriteLine("Specification Loaded...");

            //print assertion for debugging
            List<KeyValuePair<string, AssertionBase>> asrtlists = Spec.AssertionDatabase.ToList();
            List<ADLResult> results = new List<ADLResult>();
            foreach (KeyValuePair<string, AssertionBase> asrt in asrtlists)
            {
                System.Diagnostics.Debug.WriteLine("#" + asrt.Key + "#");
                // start run assertion
                AssertionBase assertion = asrt.Value;

                assertion.UIInitialize(null, 0, 0);

                assertion.VerificationMode = true;
                assertion.InternalStart();

                assertion.GetVerificationStatistics();

                // assertion.VerificationOutput.EstimateMemoryUsage;
                //System.Diagnostics.Debug.WriteLine(assertion.GetResultString());
                ADLResult rs = new ADLResult();
                rs.smell = asrt.Key.Substring(asrt.Key.IndexOf("-")+1).Replace("free","");
                rs.model = request.model;
                if (assertion.VerificationOutput.VerificationResult.Equals(VerificationResultType.VALID))
                    rs.result = "valid";
                else
                    rs.result = "invalid";

                rs.visitedStates = assertion.VerificationOutput.NoOfStates;
                rs.verificationTime = assertion.VerificationOutput.VerificationTime;
                rs.fullResultString = assertion.GetResultString();

                results.Add(rs);
            }


            return results;

        }
    }
}
