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

        private Expression BuildExpression(RuleToken operand, ParameterExpression pramExpr)
        {
            if (operand.RuleTokenType == TokenType.Operand)
            {

                return Expression.Convert(Expression.Property(
                    Expression.Convert(
                        Expression.Property(
                            Expression.Call(pramExpr, "Property", null, Expression.Constant(operand.Value))
                            , "Value")
                        , typeof(JValue))
                    , "Value"), operand.CorrespondingType);
            }
            return null;
        }

        private dynamic BuildExpression(List<RuleToken> rawTokens, int startIndex)
        {
            Expression result = Expression.Empty();
            var lToken = rawTokens[startIndex];

            var param = Expression.Parameter(typeof(JObject), "item");
            Expression lhs = BuildExpression(lToken, param);

            var op = rawTokens[startIndex + 1];
            Expression rhs = BuildExpression(rawTokens[startIndex + 2], param);
            if (rhs == null)
            {
                if (op.Value == "IN" || op.Value == "NOTIN")
                {
                    Func<string, int> f = x => int.Parse(x);
                    //.Lambda #Lambda1<System.Func`2[System.String,System.Int32]>(System.String $x) {
                    //.Call System.Int32.Parse($x)
                    //}
                    var splits = rawTokens[startIndex + 2].Value.Split(',');
                    //var paramk = Expression.Parameter(typeof(String), "item");

                    //Type converterType = typeof(Converter<,>);
                    //Type[] typeArgs = { typeof(string), lToken.CorrespondingType };
                    //Type closedConverterType = converterType.MakeGenericType(typeArgs);

                    //var converter = Expression.Lambda(
                    //    Expression.Convert(Expression.Convert(
                    //    paramk
                    //    , lToken.CorrespondingType), closedConverterType),
                    //    paramk);


                    //var method = typeof(Array).GetMethod("ConvertAll").MakeGenericMethod(new Type[] { typeof(string), lToken.CorrespondingType });
                    //var arrayBuilder = Expression.Call(method, Expression.Constant(splits), converter);

                    //Type d1 = typeof(List<>);
                    //Type[] typeArgs = { lToken.CorrespondingType };
                    //Type DynamicListType = d1.MakeGenericType(typeArgs);
                    //dynamic DynamicListObj = Activator.CreateInstance(DynamicListType);
                    var listObj = new List<object>();
                    foreach (var split in splits)
                    {
                        listObj.Add(Convert.ChangeType(split, lToken.CorrespondingType));
                    }

                    rhs = Expression.Constant(listObj);
                }
                else
                    rhs = Expression.Constant(Convert.ChangeType(rawTokens[startIndex + 2].Value, lToken.CorrespondingType), lToken.CorrespondingType);
            }

            if (op.Value == "==")
                result = Expression.Lambda(
                    Expression.Equal(
                        lhs,
                        rhs
                    )
                , param);
            else if (op.Value == ">")
                result = Expression.Lambda(
                    Expression.GreaterThan(
                        lhs,
                        rhs
                    )
                , param);
            else if (op.Value == "+")
            {
                if (lToken.CorrespondingType != typeof(string))
                    result = Expression.Lambda(
                        Expression.Add(
                            lhs,
                            rhs
                        )
                    , param);
                else
                {
                    var methodInfo = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                    result = Expression.Lambda(
                        Expression.Call(
                            methodInfo,
                            lhs,
                            rhs
                        )
                    , param);
                }
            }
            else if (op.Value == "IN")
            {
                var methodInfo = (rhs as ConstantExpression).Value.GetType().GetMethod("Contains",new[] { lhs.Type });
                result = Expression.Lambda(
                        Expression.Call(rhs,
                            methodInfo,
                            Expression.Convert(lhs, typeof(object))
                        )
                    , param);
            }
            else if (op.Value == "NOTIN")
            {
                var methodInfo = (rhs as ConstantExpression).Value.GetType().GetMethod("Contains", new[] { lhs.Type });
                result = Expression.Lambda(Expression.Not(
                        Expression.Call(rhs,
                            methodInfo,
                            Expression.Convert(lhs, typeof(object))
                        ))
                    , param);
            }


            return result;
        }
    }
}
