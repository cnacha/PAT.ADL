using PAT.Common.Classes.Expressions;
using PAT.Common.Classes.Expressions.ExpressionClass;
namespace PAT.ADL.LTS{
    public class ConfigurationWithChannelData : Configuration
    {
        public string ChannelName;
        public Expression[] Expressions;

        public ConfigurationWithChannelData (Process p, string e, string hiddenEvent, Valuation globalEnv, bool isDataOperation, string name, Expression[] expressions) : base (p, e, hiddenEvent, globalEnv, isDataOperation)
        {
            ChannelName = name;
            Expressions = expressions;
        }
    }
}