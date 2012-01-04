using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    public enum ParamType
    {
        [StringValue("double")]
        PDouble,
        [StringValue("string")]
        PString,
        [StringValue("bool")]
        PBool,
        [StringValue("fuzzy")]
        PFuzzy
    }

    public class CreditParameter : IComparable<CreditParameter>
    {
        public int ParamId;
        public string ParamName;
        public ParamType ParamType;
        public bool ParamUsed;

        public CreditParameter(int id, string name, string type, int used)
        {
            ParamId = id;
            ParamName = name;
            switch (type)
            {
                case "int":
                case "double": ParamType = ParamType.PDouble; break;
                case "string": ParamType = ParamType.PString; break;
                case "bool": ParamType = ParamType.PBool; break;
                case "fuzzy": ParamType = ParamType.PFuzzy; break;
            }
            switch (used)
            {
                case 1: ParamUsed = true; break;
                case 0: ParamUsed = false; break;
            }
        }

        public int CompareTo(CreditParameter obj)
        {
            return ParamName.CompareTo(obj.ParamName);
        }

        public override string ToString()
        {
            return ParamName;
        }
    }


    public class Term : IComparable<Term>
    {
        public int TermId;
        //public int ParmId;
        public string TermName;
        public string TermFunction;
        public bool TermUsed;
        public int RightRange;
        public int LeftRange;

        public Term(int id, /*int id,*/ string name, string func, int used, int left_range, int right_range)
        {
            TermId = id;
            TermName = name;
            TermFunction = func;
            TermFunction = func;
            RightRange = right_range;
            LeftRange = left_range;
            switch (used)
            {
                case 1: TermUsed = true; break;
                case 0: TermUsed = false; break;
            }
        }

        public int CompareTo(Term obj)
        {
            return TermName.CompareTo(obj.TermName);
        }

        public override string ToString()
        {
            return TermName;
        }
    }

    public class Parameter : CreditParameter
    {
        public string Question;
        public List<Term> Term = new List<Term>();

        public Parameter(int id, string name, string type, int used, string question, List<Term> p_term) : base(id,name,type,used)
        {
            Question = question;
            Term = p_term;
        }

    }

}
