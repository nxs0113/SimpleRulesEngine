using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RuleEngine.Common.Impl;
using RulesEngine.Common;
using RulesEngine.Common.Interfaces;
using RulesEngine.Common.Models;
using RulesEngine.Core;

namespace RulesEngine.Tests
{
    [TestClass]
    public class Start
    {
        [TestMethod]
        public void StartOff()
        {
            Person sn = new Person() { Age = 37, FirstName = "Srinivas", LastName = "Naik", Hobbies = new List<string>() { "Cricket", "Movies" } };
            var jObj = JObject.FromObject(sn);
            var behaviour = new Func<JObject, JObject>(x => { x["RuleApplied"] = true; return x; });

            var equalsToken = new RuleToken() { Value = "==", RuleTokenType = TokenType.Operator };
            var gtToken = new RuleToken() { Value = ">", RuleTokenType = TokenType.Operator };
            var ltToken = new RuleToken() { Value = "<", RuleTokenType = TokenType.Operator };
            var addToken = new RuleToken() { Value = "+", RuleTokenType = TokenType.Operator };
            var orToken = new RuleToken() { Value = "OR", RuleTokenType = TokenType.Conditional };

            var fnToken = new RuleToken()
            {
                Value = "FirstName",
                RuleTokenType = TokenType.Operand,
                AcceptableOperators = new List<RuleToken>() { equalsToken, addToken }
,                CorrespondingType = typeof(string)
            };
            var lnToken = new RuleToken()
            {
                Value = "LastName",
                RuleTokenType = TokenType.Operand,
                AcceptableOperators = new List<RuleToken>() { equalsToken, addToken }
,
                CorrespondingType = typeof(string)
            };
            var ageToken = new RuleToken()
            {
                Value = "Age",
                RuleTokenType = TokenType.Operand,
                AcceptableOperators = new List<RuleToken>() { equalsToken, gtToken, ltToken },
                CorrespondingType = typeof(double)
            };

            IRawRuleTokenizer rawRuleTokenizer = new SimpleRawRuleTokenizer(" ",
                new List<RuleToken> { fnToken, ageToken },
                new List<RuleToken> { equalsToken, gtToken, ltToken, addToken });

            RuleBuilder ruleBuilder = new RuleBuilder(rawRuleTokenizer);

            Assert.IsTrue(ruleBuilder.BuildRule("Age == 37").Compile()(jObj));
            Assert.IsFalse(ruleBuilder.BuildRule("Age == 38").Compile()(jObj));

            Assert.IsTrue(ruleBuilder.BuildRule("FirstName == Srinivas").Compile()(jObj));
            Assert.IsFalse(ruleBuilder.BuildRule("FirstName == Namita").Compile()(jObj));

            Assert.IsTrue(ruleBuilder.BuildRule("Age > 0").Compile()(jObj));
            Assert.IsFalse(ruleBuilder.BuildRule("Age > 37").Compile()(jObj));

            Assert.IsTrue(38 == ruleBuilder.BuildRule("Age + 1").Compile()(jObj));

            Assert.AreEqual("SrinivasBrain", ruleBuilder.BuildRule("FirstName + Brain").Compile()(jObj));

            rawRuleTokenizer = new SimpleRawRuleTokenizer(" ",
                new List<RuleToken> { fnToken, ageToken,lnToken  },
                new List<RuleToken> { equalsToken, gtToken, ltToken,addToken });

            ruleBuilder = new RuleBuilder(rawRuleTokenizer);
            Assert.AreEqual("SrinivasNaik", ruleBuilder.BuildRule("FirstName + LastName").Compile()(jObj));

            Assert.IsTrue(ruleBuilder.BuildRule("FirstName IN Srinivas,Namita").Compile()(jObj));
            Assert.IsFalse(ruleBuilder.BuildRule("FirstName IN Vihana,Namita").Compile()(jObj));

            Assert.IsFalse(ruleBuilder.BuildRule("Age IN 1,2").Compile()(jObj));
            Assert.IsTrue(ruleBuilder.BuildRule("Age IN 37,2").Compile()(jObj));

            Assert.IsTrue(ruleBuilder.BuildRule("Age NOTIN 1,2").Compile()(jObj));
            Assert.IsFalse(ruleBuilder.BuildRule("Age NOTIN 37,2").Compile()(jObj));

            Assert.IsFalse(ruleBuilder.BuildRule("FirstName NOTIN Srinivas,Namita").Compile()(jObj));
            Assert.IsTrue(ruleBuilder.BuildRule("FirstName NOTIN Vihana,Namita").Compile()(jObj));
        }

        
    }
}
