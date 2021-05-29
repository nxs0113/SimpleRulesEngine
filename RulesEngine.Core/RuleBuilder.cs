using Newtonsoft;
using Newtonsoft.Json.Linq;
using RulesEngine.Common;
using RulesEngine.Common.Interfaces;
using RulesEngine.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine.Core
{
    public class RuleBuilder
    {
        readonly IRawRuleTokenizer _rawRuleTokenrizer;
        public RuleBuilder(IRawRuleTokenizer rawRuleTokenrizer)
        {
            _rawRuleTokenrizer = rawRuleTokenrizer;
        }

        public dynamic BuildRule(string ruleStr)
        {
            dynamic expr = Expression.Empty();
            var rawTokens = _rawRuleTokenrizer.ConvertToTokens(ruleStr).ToList();
            if (rawTokens == null)
                return null;

            int startIndex = 0;
            if (rawTokens.Any(x => x.RuleTokenType == TokenType.Conditional))
            {
                for (int idx = 0; idx < rawTokens.Count(); idx++)
                {
                    if (rawTokens[idx].RuleTokenType == TokenType.Conditional)
                    {
                        expr = BuildExpression(rawTokens, startIndex);
                    }
                }
            }
            else
                expr = BuildExpression(rawTokens, startIndex);

            return expr;
        }

        private dynamic BuildExpression(List<RuleToken> rawTokens, int startIndex)
        {
            Expression result = Expression.Empty();
            var param = rawTokens[startIndex];

            var item = Expression.Parameter(typeof(JObject), "item");
            var body = Expression.Convert(Expression.Property(
                Expression.Convert(
                    Expression.Property(
                        Expression.Call(item, "Property", null, Expression.Constant(param.Value))
                        , "Value")
                    , typeof(JValue))
                , "Value"), param.CorrespondingType);

            var op = rawTokens[startIndex + 1];

            var rhs = Expression.Constant(Convert.ChangeType(rawTokens[startIndex + 2].Value, param.CorrespondingType), param.CorrespondingType);

            if (op.Value == "==")
                result = Expression.Lambda(
                    Expression.Equal(
                        body,
                        rhs
                    )
                , item);
            else if (op.Value == ">")
                result = Expression.Lambda(
                    Expression.GreaterThan(
                        body,
                        rhs
                    )
                , item);

            return result;
        }
    }
}
