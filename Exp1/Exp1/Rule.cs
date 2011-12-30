using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    public enum Comparision
    {
        [StringValue(">")]
        Greater,
        [StringValue(">=")]
        GreaterOrEquals,
        [StringValue("<=")]
        LessOrEquals,
        [StringValue("<")]
        Less,
        [StringValue("=")]
        Equals,
        [StringValue("!=")]
        NotEquals
    }

    public class Condition
    {
        public Parameter Parameter;
        public Comparision Comparision;
        public object Value;

        public Condition(Parameter parameter, string comparision, string value)
        {
            Parameter = parameter;
            //FIXME
            List<CreditParameter> creditparams = Helpers.ReadCreditParametersList();
            switch (comparision) 
            {
                case ">": Comparision = Comparision.Greater; break;
                case "<": Comparision = Comparision.Less; break;
                case ">=": Comparision = Comparision.GreaterOrEquals; break;
                case "<=": Comparision = Comparision.LessOrEquals; break;
                case "=": Comparision = Comparision.Equals; break;
                case "!=": Comparision = Comparision.NotEquals; break;
            }
            if (creditparams.Exists(a => a.ParamName == value))
                    Value = creditparams.First(a => a.ParamName == value);
           else if (parameter.ParamType == ParamType.PBool)
                Value = bool.Parse(value);
            else
            {
                if (parameter.ParamType == ParamType.PDouble)
                    Value = double.Parse(value);
                else Value = value;
            }
        }
        public override string ToString()
        {
            return Parameter.ParamName + " " + Comparision.GetStringValue() + " " + Value;
        }
    }

    public class Rule
    {
        public int RuleId;
        public List<Condition> Conditions = new List<Condition>();
        public Parameter Result;
        public string ResultValue;
        public int RulePriority;

        public Rule(int ruleId, Parameter result, string resultValue, int rulePriority)
        {
            RuleId = ruleId;
            Result = result;
            RulePriority = rulePriority;
            ResultValue = resultValue;
        }
        public override string ToString()
        {
            string rule = RulePriority + ": IF";
            foreach (Condition rl in Conditions)
            {
                rule += "( " + rl + " )";
                if (Conditions.IndexOf(rl) != Conditions.Count - 1)
                    rule+=" AND ";
            }
            rule+=" THEN " + Result.ParamName + " = " + ResultValue;
            return rule;
        }
    }
}
