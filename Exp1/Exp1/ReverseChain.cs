using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Exp1
{
    class ReverseChain
    {
        readonly List<Rule> _rules;
        readonly List<Parameter> _parameters;
        readonly Dictionary<Parameter, object> _paramValues;
        readonly Dictionary<Parameter, Dictionary<Term,double>> _defuzparamValues;
        readonly Dictionary<CreditParameter,string> _cparam;
        readonly Logger _log;
        

        public ReverseChain(int creditId)
        {
            _cparam = Helpers.ReadCreditParams(creditId);
            _rules = Helpers.ReadRulesList();
            _paramValues = new Dictionary<Parameter, object>();
            _defuzparamValues = new Dictionary<Parameter, Dictionary<Term, double>>();
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
                return AskQuestion(needed, level);
            }

            var localfuzzys = new Dictionary<Term, double>();
            foreach (Rule r in goodrules)
            {
                _log.Add("Processing rule:" + r, level);
                bool ruleOk = true;
                double rulefuzzys = 1; 
                foreach (Condition con in r.Conditions.Where(cond=> cond.Parameter.ParamType!=ParamType.PFuzzy))
                {
                    _log.Add("Processing condition:" + con, level);
                    Parameter p = _parameters.First(a => a.ParamId == con.Parameter.ParamId);
                    if (_paramValues[p] == null)
                        if (ReccurentSearch(p, level + 1) == false)
                            return false;
                    if (!CheckCondition(con))
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
                    foreach (Condition rl in r.Conditions.Where(cond => cond.Parameter.ParamType == ParamType.PFuzzy))
                    {
                        _log.Add("Processing Fuzzy condition:" + rl, level);
                        Parameter p = _parameters.First(a => a.ParamId == rl.Parameter.ParamId);
                        if (_paramValues[p] == null)
                            if (ReccurentSearch(p, level + 1) == false)
                                return false;
                        double d;
                        if (rl.Value is CreditParameter && (rl.Value as CreditParameter).ParamType != ParamType.PFuzzy || rl.Value is double)
                        {
                            ruleOk = CheckCondition(rl) && ruleOk;
                        }
                        else
                        {
                            rulefuzzys = Math.Min(rulefuzzys, CheckFuzzyCondition(rl));
                        }
                    }

                if (ruleOk)
                {
                    _rules.Remove(r);
                    Parameter p = _parameters.First(a => a.ParamId == r.Result.ParamId);

                    if (p.ParamType == ParamType.PDouble || (p.ParamType == ParamType.PFuzzy && !p.termGroup.Terms.Exists(term => term.TermName == r.ResultValue)))
                    {
                        var x = new Parser();
                        List<string> vars = r.ResultValue.Split(new[] { '-', '+', '/', '*', ')', '(' }).ToList().ConvertAll(s => s.Trim());
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
                            if (p.ParamType == ParamType.PFuzzy)
                                CalculateTermValues(p);
                            
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return false;
                        }
                        _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                        return true;
                    }
                    if (p.ParamType == ParamType.PBool && _cparam.Keys.ToList().Exists(par => par.ParamName == r.ResultValue))
                    {
                        _paramValues[p] = _cparam.First(par => par.Key.ParamName == r.ResultValue).Value;
                        _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                        return true;
                    }
                    if (p.ParamType == ParamType.PFuzzy)
                    {
                        Term t = p.termGroup.Terms.First(term => term.TermName == r.ResultValue);
                        if (localfuzzys.Keys.ToList().Exists(term => term.TermName == t.TermName))
                            localfuzzys[t] = Math.Max(rulefuzzys, localfuzzys[t]);
                        else
                            localfuzzys[t] = rulefuzzys;
                    }
                    else
                    {
                        _paramValues[p] = r.ResultValue;
                        _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                        return true;
                    }
                }
                _log.Add("Rule failed", level);
            }
            if (needed.ParamType == ParamType.PFuzzy)
            {
                Parameter p = _parameters.First(a => a.ParamId == needed.ParamId);
                _defuzparamValues[p] = localfuzzys;
                _paramValues[p] = Defuz(p);
                return true;
            }

            return AskQuestion(needed, level);
        }

        private double Defuz(Parameter parameter)
        {
            var termValues = _defuzparamValues[parameter];
            string function = null;
            foreach (var kvp in termValues)
            {
                if (String.IsNullOrEmpty(function))
                    function = kvp.Value.ToString() + "*" + kvp.Key.TermFunction;
                else
                    function = String.Format("min({0};{1})", function, kvp.Value.ToString() + "*" + kvp.Key.TermFunction);
            }
            Parser parser = new Parser();
            parser.Parse(function, new List<string> {"x"});
            int left = parameter.termGroup.Terms[0].LeftRange;
            int right = parameter.termGroup.Terms[0].RightRange;
            double max = double.MinValue;
            for (double i = left; i < right; i += ((double)right - left) / 1000)
            {
                max = Math.Max(max, parser.Calculate(new Dictionary<string, double> { { "x", i } }));
            }
            return max;
        }

        private bool AskQuestion(Parameter needed, int level)
        {
            if (!String.IsNullOrEmpty(needed.Question))
            {
                _log.Add("Asking user...", level);
                var ask = new AskWindow();
                _paramValues[needed] = ask.Ask(needed);
                if (needed.ParamType == ParamType.PFuzzy)
                {
                    CalculateTermValues(needed);
                }
                if (_paramValues[needed] != null)
                    return true;
            }
            return false;
        }

        private void CalculateTermValues(Parameter needed)
        {
            var dict = new Dictionary<Term, double>();
            foreach (Term term in needed.termGroup.Terms)
            {
                Parser parser = new Parser();
                parser.Parse(term.TermFunction, new List<string> {"x"});
                double value;
                if (_paramValues[needed] is double)
                    value = (double) _paramValues[needed];
                else
                    value = double.Parse(_paramValues[needed].ToString());
                dict[term] = parser.Calculate(new Dictionary<string, double> { { "x", value } });
            }
            _defuzparamValues[needed] = dict;
        }

        private double CheckFuzzyCondition(Condition rl)
        {
            Parameter p = _parameters.First(pp => pp.ParamId == rl.Parameter.ParamId);
            var values = _defuzparamValues[p];
            Term t;
            if (rl.Value is CreditParameter)
            {
                var creditParameter = rl.Value as CreditParameter;
                CreditParameter crp = _cparam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                t = p.termGroup.Terms.Find(ter => ter.TermName == _cparam[crp]);
            }
            else
            {
                t = p.termGroup.Terms.Find(ter => ter.TermName == (string) rl.Value);
            }

            switch (rl.Comparision)
            {
                case Comparision.Equals:
                    return values[t];
                case Comparision.NotEquals:
                    return (values.Where(term => term.Key.TermName != t.TermName).Max(term => term.Value));
                case Comparision.Greater:
                    return (values.Where(term => term.Key.ComparableNum > t.ComparableNum).Max(term => term.Value));
                case Comparision.Less:
                    return (values.Where(term => term.Key.ComparableNum < t.ComparableNum).Max(term => term.Value));
                case Comparision.GreaterOrEquals:
                    return (values.Where(term => term.Key.ComparableNum >= t.ComparableNum).Max(term => term.Value));
                case Comparision.LessOrEquals:
                    return (values.Where(term => term.Key.ComparableNum <= t.ComparableNum).Max(term => term.Value));
            }
            throw new Exception("Wrong comparision");
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
                default:
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
            }
            return false;
        }
    }
}
