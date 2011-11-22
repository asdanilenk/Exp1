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
        public Parameter par;
        public Comparision comparision;
        public object value;

        public Condition(Parameter par, string comparision, string value)
        {
            this.par = par;
            //FIXME
            List<CreditParameter> creditparams = Helpers.ReadCreditParametersList();
            switch (comparision) 
            {
                case ">": this.comparision = Comparision.Greater; break;
                case "<": this.comparision = Comparision.Less; break;
                case ">=": this.comparision = Comparision.GreaterOrEquals; break;
                case "<=": this.comparision = Comparision.LessOrEquals; break;
                case "=": this.comparision = Comparision.Equals; break;
                case "!=": this.comparision = Comparision.NotEquals; break;
            }
            if (creditparams.Exists(a => a.param_name == value))
                    this.value = creditparams.First(a => a.param_name == value);
           else if (par.param_type == Param_type.p_bool)
                this.value = bool.Parse(value);
            else
            {
                if (par.param_type == Param_type.p_double)
                    this.value = double.Parse(value);
                else this.value = value;
            }
        }
        public override string ToString()
        {
            return par.param_name + " " + comparision.GetStringValue() + " " + value.ToString();
        }
    }

    public class Rule
    {
        public int rule_id;
        public List<Condition> conditions = new List<Condition>();
        public Parameter result;
        public string resultvalue;
        public int rule_priority;

        public Rule(int rule_id, Parameter result, string resultvalue, int rule_priority)
        {
            this.rule_id = rule_id;
            this.result = result;
            this.rule_priority = rule_priority;
            this.resultvalue = resultvalue;
        }
        public override string ToString()
        {
            string rule = rule_priority + ": IF";
            foreach (Condition rl in conditions)
            {
                rule += "( " + rl.ToString() + " )";
                if (conditions.IndexOf(rl) != conditions.Count - 1)
                    rule+=" AND ";
            }
            rule+=" THEN " + result.param_name + " = " + resultvalue.ToString();
            return rule;
        }
    }
}
