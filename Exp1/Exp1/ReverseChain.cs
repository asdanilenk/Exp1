using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Exp1
{
    internal class ReverseChain
    {
        private readonly Dictionary<CreditParameter, string> _creditParam;
        private readonly Dictionary<Parameter, Dictionary<Term, double>> _fuzparamValues;
        private readonly Logger _log;
        private readonly Dictionary<Parameter, object> _paramValues;
        private readonly List<Parameter> _parameters;
        private readonly List<Rule> _rules;

        public ReverseChain(int creditId)
        {
            _creditParam = Helpers.ReadCreditParams(creditId);
            _rules = Helpers.ReadRulesList();
            _paramValues = new Dictionary<Parameter, object>();
            _fuzparamValues = new Dictionary<Parameter, Dictionary<Term, double>>();
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
            if (goodrules.Count == 0 || goodrules.All(rule => rule.Conditions.Exists(cond => cond.Parameter.ParamId == needed.ParamId)))
            {
                return AskQuestion(needed, level);
            }

            var fuzzyTermValues = new Dictionary<Term, double>();
            foreach (Rule rule in goodrules)
            {
                _log.Add("Processing rule:" + rule, level);
                bool ruleOk = true;
                double rulefuzzys = 1;
                foreach (Condition condition in rule.Conditions.Where(cond => cond.Parameter.ParamType != ParamType.PFuzzy))
                {
                    _log.Add("Processing condition:" + condition, level);
                    Parameter p = _parameters.First(a => a.ParamId == condition.Parameter.ParamId);
                    if (_paramValues[p] == null)
                        if (ReccurentSearch(p, level + 1) == false)
                            return false;
                    if (!CheckCondition(condition))
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
                    foreach (Condition condition in rule.Conditions.Where(cond => cond.Parameter.ParamType == ParamType.PFuzzy))
                    {
                        _log.Add("Processing Fuzzy condition:" + condition, level);
                        Parameter p = _parameters.First(a => a.ParamId == condition.Parameter.ParamId);
                        if (_paramValues[p] == null)
                            if (ReccurentSearch(p, level + 1) == false)
                                return false;
                        if ((condition.Value is CreditParameter && (condition.Value as CreditParameter).ParamType != ParamType.PFuzzy) ||
                            condition.Value is double)
                        {
                            bool result = CheckCondition(condition);
                            ruleOk = result && ruleOk;
                            _log.Add("Fuzzy condition is not Fuzzy really. Result is " + result, level);
                        }
                        else
                        {
                            double result = CheckFuzzyCondition(condition);
                            rulefuzzys = Math.Min(rulefuzzys, result);
                            _log.Add("Fuzzy condition returned probability " + result, level);
                        }
                    }
                _log.Add("Finished processing conditions", level);

                if (ruleOk)
                {
                    _rules.Remove(rule);
                    Parameter p = _parameters.First(a => a.ParamId == rule.Result.ParamId);
                    _log.Add("Calculating result parameter " + rule.Result.ParamName + " using function " + rule.ResultValue, level);
                    if (p.ParamType == ParamType.PDouble ||
                        (p.ParamType == ParamType.PFuzzy && !p.termGroup.Terms.Exists(term => term.TermName == rule.ResultValue)))
                    {
                        var parser = new Parser();
                        List<string> vars = rule.ResultValue.Split(new[] {'-', '+', '/', '*', ')', '('}).ToList().ConvertAll(s => s.Trim());
                        foreach (string var in vars)
                        {
                            Parameter par = _parameters.Find(f => f.ParamName == var);
                            if (par != null && _paramValues[par] == null)
                                if (ReccurentSearch(par, level + 1) == false)
                                    return false;
                        }
                        List<CreditParameter> allParameters = _parameters.Cast<CreditParameter>().ToList();
                        allParameters.AddRange(_creditParam.Keys);
                        //localparameters now contains all parameters and creditparameters
                        List<string> parameterNames = allParameters.ConvertAll(a => a.ParamName);
                        try
                        {
                            parser.Parse(rule.ResultValue, parameterNames);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return false;
                        }

                        Dictionary<string, double> allParametersValues = _paramValues.ToValueList();
                        foreach (var crp in _creditParam.Where(crp => crp.Key.ParamType == ParamType.PDouble))
                            allParametersValues.Add(crp.Key.ParamName, double.Parse(crp.Value));
                        try
                        {
                            _paramValues[p] = parser.Calculate(allParametersValues);
                            if (p.ParamType == ParamType.PFuzzy)
                                CalculateTermValues(p, level);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return false;
                        }
                        _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                        return true;
                    }
                    if (p.ParamType == ParamType.PBool && _creditParam.Keys.ToList().Exists(par => par.ParamName == rule.ResultValue))
                    {
                        _paramValues[p] = _creditParam.First(par => par.Key.ParamName == rule.ResultValue).Value;
                        _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                        return true;
                    }
                    if (p.ParamType == ParamType.PFuzzy)
                    {
                        Term t = p.termGroup.Terms.First(term => term.TermName == rule.ResultValue);
                        if (fuzzyTermValues.Keys.ToList().Exists(term => term.TermName == t.TermName))
                            fuzzyTermValues[t] = Math.Max(rulefuzzys, fuzzyTermValues[t]);
                        else
                            fuzzyTermValues[t] = rulefuzzys;
                        _log.Add("Rule passed", level);
                    }
                    //for PString
                    else
                    {
                        _paramValues[p] = rule.ResultValue;
                        _log.Add("Rule passed => " + p.ParamName + "=" + _paramValues[p], level);
                        return true;
                    }
                }
                else
                    _log.Add("Rule failed", level);
            }
            if (needed.ParamType == ParamType.PFuzzy)
            {
                Parameter p = _parameters.First(a => a.ParamId == needed.ParamId);
                _fuzparamValues[p] = fuzzyTermValues;
                _paramValues[p] = Defuz(p, level);
                return true;
            }

            return AskQuestion(needed, level);
        }

        private double Defuz(Parameter parameter, int level)
        {
            _log.Add("Defuzzing variable:" + parameter.ParamName, level);
            Dictionary<Term,double> termValues = _fuzparamValues[parameter];
            string function = null;
            foreach (var termValue in termValues)
            {
                string localfunction = String.Format("min({0};{1})", termValue.Value.ToString(), termValue.Key.TermFunction);
                if (String.IsNullOrEmpty(function))
                    function = localfunction;
                else
                    function = String.Format("max({0};{1})", function, localfunction);
            }
            _log.Add("Compiled function: " + function, level);
            Parser parser = new Parser();
            parser.Parse(function, new List<string> {"x"});
            int left = parameter.termGroup.Terms[0].LeftRange;
            int right = parameter.termGroup.Terms[0].RightRange;
            double max = double.MinValue;
            double x = left;
            for (double i = left; i < right; i += ((double) right - left)/1000)
            {
                double value = parser.Calculate(new Dictionary<string, double> {{"x", i}});
                if (max<value)
                {
                    max = value;
                    x = i;
                }
            }
            _log.Add("Max value on x is: " + x, level);
            return x;
        }

        private bool AskQuestion(Parameter needed, int level)
        {
            if (!String.IsNullOrEmpty(needed.Question))
            {
                _log.Add("Asking user for " + needed.ParamType.GetStringValue() + " variable " + needed.ParamName + " ...", level);
                AskWindow ask = new AskWindow();
                _paramValues[needed] = ask.Ask(needed);
                _log.Add("User answered : " + _paramValues[needed], level);
                if (needed.ParamType == ParamType.PFuzzy)
                {
                    CalculateTermValues(needed, level);
                }
                if (_paramValues[needed] != null)
                    return true;
            }
            return false;
        }

        private void CalculateTermValues(Parameter needed, int level)
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
                if (value > term.RightRange)
                    value = term.RightRange;
                if (value < term.LeftRange)
                    value = term.LeftRange;
                dict[term] = parser.Calculate(new Dictionary<string, double> {{"x", value}});
                _log.Add("Term value for " + term.TermName + " : " + dict[term], level + 1);
            }
            _fuzparamValues[needed] = dict;
        }

        private double CheckFuzzyCondition(Condition condition)
        {
            Parameter p = _parameters.First(pp => pp.ParamId == condition.Parameter.ParamId);
            Dictionary<Term,double> fuzzvalues = _fuzparamValues[p];
            Term t;
            if (condition.Value is CreditParameter)
            {
                var creditParameter = condition.Value as CreditParameter;
                CreditParameter crp = _creditParam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                t = p.termGroup.Terms.Find(ter => ter.TermName == _creditParam[crp]);
            }
            else
            {
                t = p.termGroup.Terms.Find(ter => ter.TermName == (string) condition.Value);
            }

            switch (condition.Comparision)
            {
                case Comparision.Equals:
                    return fuzzvalues[t];
                case Comparision.NotEquals:
                    return (fuzzvalues.Where(term => term.Key.TermName != t.TermName).Max(term => term.Value));
                case Comparision.Greater:
                    return (fuzzvalues.Where(term => term.Key.ComparableNum > t.ComparableNum).Max(term => term.Value));
                case Comparision.Less:
                    return (fuzzvalues.Where(term => term.Key.ComparableNum < t.ComparableNum).Max(term => term.Value));
                case Comparision.GreaterOrEquals:
                    return (fuzzvalues.Where(term => term.Key.ComparableNum >= t.ComparableNum).Max(term => term.Value));
                case Comparision.LessOrEquals:
                    return (fuzzvalues.Where(term => term.Key.ComparableNum <= t.ComparableNum).Max(term => term.Value));
            }
            throw new Exception("Wrong comparision");
        }

        public bool CheckCondition(Condition condition)
        {
            Parameter p = _parameters.First(pp => pp.ParamId == condition.Parameter.ParamId);
            const double epsilon = 0.0001;
            switch (condition.Parameter.ParamType)
            {
                case ParamType.PBool:
                    {
                        if (condition.Value is CreditParameter)
                        {
                            var creditParameter = condition.Value as CreditParameter;
                            CreditParameter crp = _creditParam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                            switch (condition.Comparision)
                            {
                                case Comparision.Greater:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_creditParam[crp])) > 0);
                                case Comparision.Less:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_creditParam[crp])) < 0);
                                case Comparision.GreaterOrEquals:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_creditParam[crp])) >= 0);
                                case Comparision.LessOrEquals:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo(bool.Parse(_creditParam[crp])) <= 0);
                                case Comparision.Equals:
                                    return (bool.Parse(_paramValues[p].ToString()) == bool.Parse(_creditParam[crp]));
                                case Comparision.NotEquals:
                                    return (bool.Parse(_paramValues[p].ToString()) != bool.Parse(_creditParam[crp]));
                            }
                        }
                        else
                        {
                            switch (condition.Comparision)
                            {
                                case Comparision.Greater:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool) condition.Value) > 0);
                                case Comparision.Less:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool) condition.Value) < 0);
                                case Comparision.GreaterOrEquals:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool) condition.Value) >= 0);
                                case Comparision.LessOrEquals:
                                    return ((bool.Parse(_paramValues[p].ToString())).CompareTo((bool) condition.Value) <= 0);
                                case Comparision.Equals:
                                    return (bool.Parse(_paramValues[p].ToString()) == (bool) condition.Value);
                                case Comparision.NotEquals:
                                    return (bool.Parse(_paramValues[p].ToString()) != (bool) condition.Value);
                            }
                        }
                        break;
                    }
                case ParamType.PString:
                    {
                        if (condition.Value is CreditParameter)
                        {
                            var creditParameter = condition.Value as CreditParameter;
                            CreditParameter crp = _creditParam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                            switch (condition.Comparision)
                            {
                                case Comparision.Equals:
                                    return ((string) _paramValues[p] == _creditParam[crp]);
                                case Comparision.NotEquals:
                                    return ((string) _paramValues[p] != _creditParam[crp]);
                            }
                        }
                        else
                        {
                            switch (condition.Comparision)
                            {
                                case Comparision.Equals:
                                    return ((string) _paramValues[p] == (string) condition.Value);
                                case Comparision.NotEquals:
                                    return ((string) _paramValues[p] != (string) condition.Value);
                            }
                        }
                        break;
                    }
                default:
                    {
                        if (condition.Value is CreditParameter)
                        {
                            var creditParameter = condition.Value as CreditParameter;
                            CreditParameter crp = _creditParam.Keys.First(cp => cp.ParamId == creditParameter.ParamId);
                            switch (condition.Comparision)
                            {
                                case Comparision.Equals:
                                    return (Math.Abs((double) _paramValues[p] - double.Parse(_creditParam[crp])) < epsilon);
                                case Comparision.NotEquals:
                                    return (Math.Abs((double) _paramValues[p] - double.Parse(_creditParam[crp])) > epsilon);
                                case Comparision.Greater:
                                    return ((double) _paramValues[p] > double.Parse(_creditParam[crp]));
                                case Comparision.Less:
                                    return ((double) _paramValues[p] < double.Parse(_creditParam[crp]));
                                case Comparision.GreaterOrEquals:
                                    return ((double) _paramValues[p] >= double.Parse(_creditParam[crp]));
                                case Comparision.LessOrEquals:
                                    return ((double) _paramValues[p] <= double.Parse(_creditParam[crp]));
                            }
                        }
                        else
                        {
                            switch (condition.Comparision)
                            {
                                case Comparision.Equals:
                                    return (Math.Abs((double) _paramValues[p] - (double) condition.Value) < epsilon);
                                case Comparision.NotEquals:
                                    return (Math.Abs((double) _paramValues[p] - (double) condition.Value) > epsilon);
                                case Comparision.Greater:
                                    return ((double) _paramValues[p] > (double) condition.Value);
                                case Comparision.Less:
                                    return ((double) _paramValues[p] < (double) condition.Value);
                                case Comparision.GreaterOrEquals:
                                    return ((double) _paramValues[p] >= (double) condition.Value);
                                case Comparision.LessOrEquals:
                                    return ((double) _paramValues[p] <= (double) condition.Value);
                            }
                        }

                        break;
                    }
            }
            return false;
        }
    }
}