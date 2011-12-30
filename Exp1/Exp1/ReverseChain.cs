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
        readonly List<Rule> _rules;
        readonly List<Parameter> _parameters;
        readonly Dictionary<Parameter, object> _paramValues;
        readonly Dictionary<CreditParameter,string> _cparam;
        readonly Logger _log;
        

        public ReverseChain(int creditId)
        {
            _cparam = Helpers.ReadCreditParams(creditId);
            _rules = Helpers.ReadRulesList();
            _paramValues = new Dictionary<Parameter, object>();
            _parameters = Helpers.ReadParametersList();
            _log = new Logger();

            foreach (Parameter par in _parameters)
                _paramValues.Add(par, null);
            
            //FIXME
            Parameter kredit = _parameters.First(p => p.ParamName == "Кредит");
            bool result = ReccurentSearch(kredit, 0);
            if (result)
                (new ResultsWindow("Результат определен:" + _paramValues[kredit], _log)).ShowDialog();
            else
                (new ResultsWindow("Ваши данные не подходят для определения результата", _log)).ShowDialog();
        }

        private bool ReccurentSearch(Parameter needed, int level)
        {
            _log.Add("Searching for " + needed.ParamName + " :", level);
            List<Rule> goodrules = _rules.FindAll(r => r.Result.ParamId == needed.ParamId);
            _log.Add("Rules found: " + goodrules.Count, level);
            if (goodrules.Count==0 || goodrules.All(rule => rule.Conditions.Exists(cond => cond.Parameter.ParamId == needed.ParamId)))
            {
                if (!String.IsNullOrEmpty(needed.Question))
                {
                    _log.Add("Asking user...", level);
                    var ask = new AskWindow();
                    _paramValues[needed] = ask.Ask(needed);
                    if (_paramValues[needed] != null)
                        return true;
                }    
                    return false;
            }

            foreach (Rule r in goodrules)
            {
                _log.Add("Processing rule:" + r, level);
                bool ruleOk = true;
                foreach (Condition rl in r.Conditions)
                {
                    _log.Add("Processing condition:" + rl, level);
                    Parameter p = _parameters.First(a => a.ParamId == rl.Parameter.ParamId);
                    if (_paramValues[p] == null)
                        if (ReccurentSearch(p, level + 1) == false)
                            return false;
                    if (!CheckCondition(rl))
                    {
                        _log.Add("Condition failed", level);
                        ruleOk = false;
                    }
                    else
                    {
                        _log.Add("Condition passed", level);
                    }
                }
                if (ruleOk)
                {
                    _rules.Remove(r);
                    Parameter p = _parameters.First(a => a.ParamId == r.Result.ParamId);
                    if (p.ParamType == ParamType.PDouble)
                    {
                        var x = new Parser();
                        List<string> vars = r.ResultValue.Split(new Char[] { '-', '+', '/', '*', ')', '(' }).ToList().ConvertAll<string>(s => s.Trim());
                        foreach (string var in vars)
                        {
                            Parameter par = _parameters.Find(f => f.ParamName == var);
                            if (par != null && _paramValues[par] == null)
                                if (ReccurentSearch(par, level + 1) == false)
                                    return false;
                        }
                        var localparameters = _parameters.Cast<CreditParameter>().ToList();
                        localparameters.AddRange(_cparam.Keys);

                        try
                        {
                            x.Parse(r.ResultValue, localparameters.ConvertAll(a => a.ParamName));
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return false;
                        }

                        Dictionary<string,double> localvalues = _paramValues.ToValueList();
                        foreach (var crp in _cparam.Where(crp => crp.Key.ParamType == ParamType.PDouble))
                            localvalues.Add(crp.Key.ParamName, double.Parse(crp.Value));
                        try
                        {
                            _paramValues[p] = x.Calculate(localvalues);
                            
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return false;
                        }
                    }
                    else if (p.ParamType == ParamType.PBool && _cparam.Keys.ToList().Exists(par => par.ParamName == r.ResultValue))
                    {
                        _paramValues[p] = _cparam.First(par => par.Key.ParamName == r.ResultValue).Value;
                    }
                    else
                    {
                        _paramValues[p] = r.ResultValue;
                    }
                    _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                    return true;
                }
                _log.Add("Rule failed", level);
            }

            if (!String.IsNullOrEmpty(needed.Question))
            {
                _log.Add("Asking user...", level);
                var ask = new AskWindow();
                _paramValues[needed] = ask.Ask(needed);
                if (_paramValues[needed] != null)
                    return true;
            }

            return false;
        }

        public bool CheckCondition(Condition rl)
        {
            Parameter p = _parameters.First(pp => pp.ParamId == rl.Parameter.ParamId);
            const double epsilon = 0.0001;
            switch (rl.Parameter.ParamType)
            {
                case ParamType.PBool:
                    {
                        if (rl.Value is CreditParameter)
                        {
                            var creditParameter = rl.Value as CreditParameter;
                            CreditParameter crp = _cparam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                            switch (rl.Comparision)
                            {

                                case Comparision.Greater: return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_cparam[crp])) > 0);
                                case Comparision.Less: return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_cparam[crp])) < 0);
                                case Comparision.GreaterOrEquals: return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_cparam[crp])) >= 0);
                                case Comparision.LessOrEquals: return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_cparam[crp])) <= 0);
                                case Comparision.Equals: return (bool.Parse(_paramValues[p].ToString()) == bool.Parse(_cparam[crp]));
                                case Comparision.NotEquals: return (bool.Parse(_paramValues[p].ToString()) != bool.Parse(_cparam[crp]));
                            }
                        }
                        else
                        {
                            switch (rl.Comparision)
                            {
                                case Comparision.Greater: return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool)rl.Value) > 0);
                                case Comparision.Less: return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool)rl.Value) < 0);
                                case Comparision.GreaterOrEquals: return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool)rl.Value) >= 0);
                                case Comparision.LessOrEquals: return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool)rl.Value) <= 0);
                                case Comparision.Equals: return (bool.Parse(_paramValues[p].ToString()) == (bool)rl.Value);
                                case Comparision.NotEquals: return (bool.Parse(_paramValues[p].ToString()) != (bool)rl.Value);
                            }
                        }
                        break;
                    }
                case ParamType.PDouble:
                    {
                        if (rl.Value is CreditParameter)
                        {
                            var creditParameter = rl.Value as CreditParameter;
                            CreditParameter crp = _cparam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                            switch (rl.Comparision)
                            {
                                case Comparision.Equals: return (Math.Abs((double)_paramValues[p] - double.Parse(_cparam[crp])) < epsilon);
                                case Comparision.NotEquals: return (Math.Abs((double)_paramValues[p] - double.Parse(_cparam[crp])) > epsilon);
                                case Comparision.Greater: return ((double)_paramValues[p] > double.Parse(_cparam[crp]));
                                case Comparision.Less: return ((double)_paramValues[p] < double.Parse(_cparam[crp]));
                                case Comparision.GreaterOrEquals: return ((double)_paramValues[p] >= double.Parse(_cparam[crp]));
                                case Comparision.LessOrEquals: return ((double)_paramValues[p] <= double.Parse(_cparam[crp]));
                            }
                        }
                        else
                        {
                            switch (rl.Comparision)
                            {
                                case Comparision.Equals: return (Math.Abs((double)_paramValues[p] - (double)rl.Value) < epsilon);
                                case Comparision.NotEquals: return (Math.Abs((double)_paramValues[p] - (double)rl.Value) > epsilon);
                                case Comparision.Greater: return ((double)_paramValues[p] > (double)rl.Value);
                                case Comparision.Less: return ((double)_paramValues[p] < (double)rl.Value);
                                case Comparision.GreaterOrEquals: return ((double)_paramValues[p] >= (double)rl.Value);
                                case Comparision.LessOrEquals: return ((double)_paramValues[p] <= (double)rl.Value);
                            }
                        }
                        
                        break;
                    }
                case ParamType.PString:
                    {
                        if (rl.Value is CreditParameter)
                        {
                            var creditParameter = rl.Value as CreditParameter;
                            CreditParameter crp = _cparam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                            switch (rl.Comparision)
                            {
                                case Comparision.Equals: return ((string)_paramValues[p] == _cparam[crp]);
                                case Comparision.NotEquals: return ((string)_paramValues[p] != _cparam[crp]);
                            }
                        }
                        else
                        {
                            switch (rl.Comparision)
                            {
                                case Comparision.Equals: return ((string)_paramValues[p] == (string)rl.Value);
                                case Comparision.NotEquals: return ((string)_paramValues[p] != (string)rl.Value);
                            }
                        }
                        break;
                    }
            }
            return false;
        }
    }
}
