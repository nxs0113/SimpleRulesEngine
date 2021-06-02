using RulesEngine.Common;
using RulesEngine.Common.Interfaces; 
using RulesEngine.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleEngine.Common.Impl
{
    public class SimpleRawRuleTokenizer : IRawRuleTokenizer
    {
        readonly string _separator;
        readonly Dictionary<string,RuleToken> _operatorTokens;
        readonly Dictionary<string,RuleToken> _operandTokens;

        public SimpleRawRuleTokenizer(string separator, List<RuleToken> operatorTokens, List<RuleToken> operandTokens)
        {
            _separator = separator;
            _operatorTokens = operatorTokens.ToDictionary(x=>x.Value);
            _operandTokens = operandTokens.ToDictionary(x => x.Value);
        }

        public IEnumerable<RuleToken> ConvertToTokens(string rawRuleStr)
        {
            var rawRuleTokens =
            rawRuleStr.Split().Select((x) =>
            {
                if (_operatorTokens.ContainsKey(x)) return _operatorTokens[x];
                if (_operandTokens.ContainsKey(x)) return _operandTokens[x];
                
                return new RuleToken() { Value = x,RuleTokenType = TokenType.Value};
            });

            return rawRuleTokens;
        }
    }    
}
