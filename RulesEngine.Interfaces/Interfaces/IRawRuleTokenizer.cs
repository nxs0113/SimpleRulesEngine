using RulesEngine.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine.Common.Interfaces
{
    public interface IRawRuleTokenizer
    {
        IEnumerable<RuleToken> ConvertToTokens(string rawRuleStr);
    }
}
