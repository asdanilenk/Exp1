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
        List<param> parameters;
        Dictionary<param, object> paramValues;
        Dictionary<creditparam,string> cparam;
        Logger log;
        

        public ReverseChain(int credit_id)
        {
            cparam = Helpers.ReadCreditParams(credit_id);
            rules = Helpers.ReadRulesList();
            paramValues = new Dictionary<param, object>();
            parameters = Helpers.ReadParametersList();
            log = new Logger();

            foreach (param par in parameters)
                paramValues.Add(par, null);
            
            //FIXME
            param kredit = parameters.First(p => p.param_name == "Кредит");
            bool result = ReccurentSearch(kredit, 0);
            if (result == true)
                (new ResultsWindow("Результат определен:" + paramValues[kredit], log)).ShowDialog();
            else
                (new ResultsWindow("Ваши данные не подходят для определения результата", log)).ShowDialog();
        }

        private bool ReccurentSearch(param needed, int level)
        {
            log.Add("Searching for " + needed.param_name + " :", level);
            List<Rule> goodrules = rules.FindAll(r => r.result.param_id == needed.param_id);
            log.Add("Rules found: " + goodrules.Count, level);
            if (goodrules.Count==0 || goodrules.All(rule => rule.conditions.Exists(cond => cond.par.param_id == needed.param_id)))
            {
                if (!String.IsNullOrEmpty(needed.question))
                {
                    log.Add("Asking user...", level);
                    AskWindow ask = new AskWindow();
                    paramValues[needed] = ask.Ask(needed);
                    if (paramValues[needed] != null)
                        return true;
                }    
                    return false;
            }

            foreach (Rule r in goodrules)
            {
                log.Add("Processing rule:" + r.ToString(), level);
                bool ruleOK = true;
                foreach (Condition rl in r.conditions)
                {
                    log.Add("Processing condition:" + rl.ToString(), level);
                    param p = parameters.First(a => a.param_id == rl.par.param_id);
                    if (paramValues[p] == null)
                        if (ReccurentSearch(p, level + 1) == false)
                            return false;
                    if (!CheckCondition(rl))
                    {
                        log.Add("Condition failed", level);
                        ruleOK = false;
                    }
                    else
                    {
                        log.Add("Condition passed", level);
                    }
                }
                if (ruleOK == true)
                {
                    rules.Remove(r);
                    param p = parameters.First(a => a.param_id == r.result.param_id);
                    if (p.param_type == Param_type.p_double)
                    {
                        Parser x = new Parser();
                        List<string> vars = r.resultvalue.Split(new Char[] { '-', '+', '/', '*', ')', '(' }).ToList().ConvertAll<string>(s => s.Trim());
                        foreach (string var in vars)
                        {
                            param par = parameters.Find(f => f.param_name == var);
                            if (par!=null && paramValues[par] == null)
                                ReccurentSearch(par, level + 1);
                        }
                        List<creditparam> localparameters = new List<creditparam>();
                        foreach (creditparam crp in parameters)
                            localparameters.Add(crp);
                        foreach (creditparam crp in cparam.Keys)
                            localparameters.Add(crp);
                        try
                        {
                            x.Parse((string)r.resultvalue, localparameters.ConvertAll<string>(a => a.param_name));
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        Dictionary<string,double> localvalues = paramValues.ToValueList();
                        foreach (KeyValuePair<creditparam, string> crp in cparam)
                            if (crp.Key.param_type == Param_type.p_double)
                                localvalues.Add(crp.Key.param_name, double.Parse(crp.Value.ToString()));
                        paramValues[p] = x.Calculate(localvalues);
                    }
                    else
                    {
                        paramValues[p] = r.resultvalue;
                    }
                    log.Add("Rule passed => " + p.param_name + "=" + paramValues[p], level);
                    return true;
                }
                else
                {
                    log.Add("Rule failed", level);
                }
            }
            return false;
        }

        public bool CheckCondition(Condition rl)
        {
            param p = parameters.First(pp => pp.param_id == rl.par.param_id);
            
            switch (rl.par.param_type)
            {
                case Param_type.p_bool:
                    {
                        if (rl.value is creditparam)
                        {
                            creditparam crp = cparam.Keys.First(cp => cp.param_id == (rl.value as creditparam).param_id);
                            switch (rl.comparision)
                            {

                                case Comparision.Greater: return ((bool.Parse(paramValues[p].ToString())).CompareTo(bool.Parse(cparam[crp])) > 0);
                                case Comparision.Less: return ((bool.Parse(paramValues[p].ToString())).CompareTo(bool.Parse(cparam[crp])) < 0);
                                case Comparision.GreaterOrEquals: return ((bool.Parse(paramValues[p].ToString())).CompareTo(bool.Parse(cparam[crp])) >= 0);
                                case Comparision.LessOrEquals: return ((bool.Parse(paramValues[p].ToString())).CompareTo(bool.Parse(cparam[crp])) <= 0);
                                case Comparision.Equals: return (bool.Parse(paramValues[p].ToString()) == bool.Parse(cparam[crp]));
                                case Comparision.NotEquals: return (bool.Parse(paramValues[p].ToString()) != bool.Parse(cparam[crp]));
                            }
                        }
                        else
                        {
                            switch (rl.comparision)
                            {
                                case Comparision.Greater: return ((bool.Parse(paramValues[p].ToString())).CompareTo((bool)rl.value) > 0);
                                case Comparision.Less: return ((bool.Parse(paramValues[p].ToString())).CompareTo((bool)rl.value) < 0);
                                case Comparision.GreaterOrEquals: return ((bool.Parse(paramValues[p].ToString())).CompareTo((bool)rl.value) >= 0);
                                case Comparision.LessOrEquals: return ((bool.Parse(paramValues[p].ToString())).CompareTo((bool)rl.value) <= 0);
                                case Comparision.Equals: return (bool.Parse(paramValues[p].ToString()) == (bool)rl.value);
                                case Comparision.NotEquals: return (bool.Parse(paramValues[p].ToString()) != (bool)rl.value);
                            }
                        }
                        break;
                    }
                case Param_type.p_double:
                    {
                        if (rl.value is creditparam)
                        {
                            creditparam crp = cparam.Keys.First(cp => cp.param_id == (rl.value as creditparam).param_id);
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((double)paramValues[p] == double.Parse(cparam[crp]));
                                case Comparision.NotEquals: return ((double)paramValues[p] != double.Parse(cparam[crp]));
                                case Comparision.Greater: return ((double)paramValues[p] > double.Parse(cparam[crp]));
                                case Comparision.Less: return ((double)paramValues[p] < double.Parse(cparam[crp]));
                                case Comparision.GreaterOrEquals: return ((double)paramValues[p] >= double.Parse(cparam[crp]));
                                case Comparision.LessOrEquals: return ((double)paramValues[p] <= double.Parse(cparam[crp]));
                            }
                        }
                        else
                        {
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((double)paramValues[p] == (double)rl.value);
                                case Comparision.NotEquals: return ((double)paramValues[p] != (double)rl.value);
                                case Comparision.Greater: return ((double)paramValues[p] > (double)rl.value);
                                case Comparision.Less: return ((double)paramValues[p] < (double)rl.value);
                                case Comparision.GreaterOrEquals: return ((double)paramValues[p] >= (double)rl.value);
                                case Comparision.LessOrEquals: return ((double)paramValues[p] <= (double)rl.value);
                            }
                        }
                        
                        break;
                    }
                case Param_type.p_string:
                    {
                        if (rl.value is creditparam)
                        {
                            creditparam crp = cparam.Keys.First(cp => cp.param_id == (rl.value as creditparam).param_id);
                            switch (rl.comparision)
                            {
                                case Comparision.Equals: return ((string)paramValues[p] == cparam[crp]);
                                case Comparision.NotEquals: return ((string)paramValues[p] != cparam[crp]);
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
