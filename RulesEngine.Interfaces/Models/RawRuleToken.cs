using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine.Common.Models
{
    public class RuleToken
    {
        public string Value { get; set; }
        public List<RuleToken> AcceptableOperators
        {
            get;
            set;
        }
        public TokenType RuleTokenType { get; set; }
        public Type CorrespondingType{ get; set; }
    }
}
