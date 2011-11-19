using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Exp1
{
    class ReverseChain
    {
        List<Rule> rules;
        Dictionary<param, object> paramValues;
        Dictionary<creditparam,string> cparam;


        public ReverseChain(int credit_id)
        {
            cparam = Helpers.ReadCreditParams(credit_id);
            rules = Helpers.ReadRulesList();
            paramValues = new Dictionary<param, object>();
            List<param> pars = Helpers.ReadParametersList();
            foreach (param par in pars)
                paramValues.Add(par, null);

            
            Stack<param> needed = new Stack<param>();
            //FIXME
            bool result = ReccurentSearch(pars.First(p => p.param_name == "Кредит"));
            if (result == true)
            {
                KeyValuePair<param,object> kvp = paramValues.First(a => a.Key.param_name == "Кредит");
                MessageBox.Show("Результат определен:" + kvp.Value.ToString());
            }
        }

        private bool ReccurentSearch(param needed)
        {
            List<Rule> goodrules = rules.FindAll(r => r.result.param_id == needed.param_id);
            if (goodrules.Count==0)
            {
                if (!String.IsNullOrEmpty(needed.question))
                {
                    AskWindow ask = new AskWindow(needed);
                    if (ask.ShowDialog() == true)
                    {
                        param p = paramValues.First(a => a.Key.param_id == needed.param_id).Key;
                        paramValues[p] = ask.Answer;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }

            foreach (Rule r in goodrules)
            {
                bool ruleOK = true;
                foreach (Condition rl in r.conditions)
                {
                    param p = paramValues.Keys.First(a => a.param_id == rl.par.param_id);
                    if (paramValues[p] == null)
                        if (ReccurentSearch(rl.par) == false)
                            return false;
                    if (!CheckCondition(rl))
                        ruleOK=false;
                }
                if (ruleOK == true)
                {
                    rules.Remove(r);
                    param p = paramValues.First(a => a.Key.param_id == r.result.param_id).Key;
                    paramValues[p] = r.resultvalue;
                    
                    return true;
                }
            }
            return false;
        }

        public bool CheckCondition(Condition rl)
        {
            param p = paramValues.Keys.First(pp => pp.param_id == rl.par.param_id);

            switch (rl.par.param_type)
            {
                case Param_type.p_bool:
                    {
                        switch (rl.comparision)
                        {
                            case Comparision.Equals: return ((bool)paramValues[p] == (bool)rl.value);
                            case Comparision.NotEquals: return ((bool)paramValues[p] != (bool)rl.value);
                        }
                        break;
                    }
                case Param_type.p_int:
                    {
                        if (rl.value is creditparam)
                        {
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((int)paramValues[p] == int.Parse(cparam[rl.value as creditparam]));
                                case Comparision.NotEquals: return ((int)paramValues[p] != int.Parse(cparam[rl.value as creditparam]));
                                case Comparision.Greater: return ((int)paramValues[p] > int.Parse(cparam[rl.value as creditparam]));
                                case Comparision.Less: return ((int)paramValues[p] < int.Parse(cparam[rl.value as creditparam]));
                            }
                        }
                        else
                        {
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((int)paramValues[p] == (int)rl.value);
                                case Comparision.NotEquals: return ((int)paramValues[p] != (int)rl.value);
                                case Comparision.Greater: return ((int)paramValues[p] > (int)rl.value);
                                case Comparision.Less: return ((int)paramValues[p] < (int)rl.value);
                            }
                        }
                        
                        break;
                    }
                case Param_type.p_string:
                    {
                        if (rl.value is creditparam)
                        {
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((string)paramValues[p] == cparam[rl.value as creditparam]);
                                case Comparision.NotEquals: return ((string)paramValues[p] != cparam[rl.value as creditparam]);
                            }
                        }
                        else
                        {
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((string)paramValues[p] == (string)rl.value);
                                case Comparision.NotEquals: return ((string)paramValues[p] != (string)rl.value);
                            }
                        }
                        break;
                    }
            }
            return false;
        }
    }
}
