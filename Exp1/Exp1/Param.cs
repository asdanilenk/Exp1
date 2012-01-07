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


   
    public class Term : IComparable<Term>
    {
        public int TermId;
        public string TermName;
        public string TermFunction;
        public bool TermUsed;
        public int RightRange;
        public int LeftRange;
        public int ComparableNum;
        //public int GroupId;
        //public string GroupName;

        public Term(int id, string name, string func, int used, int left_range, int right_range, int comparable_num/*, int group_id, string group_name*/)
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
            //GroupId = group_id;
            //GroupName = group_name;
            ComparableNum = comparable_num;
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

    public class TermGroup : IComparable<TermGroup>
    {
        public int TermGroupId;
        //public int ParmId;
        public string TermGroupName;
        public bool TermGroupUsed;
        public List<Term> Terms = new List<Term>();


        public TermGroup(int id, string name, int used, List<Term> p_terms)
        {
            TermGroupId = id;
            TermGroupName = name;

            switch (used)
            {
                case 1: TermGroupUsed = true; break;
                case 0: TermGroupUsed = false; break;
            }
            Terms = p_terms;
        }

        public override string ToString()
        {
            return TermGroupName;
        }

        public int CompareTo(TermGroup obj)
        {
            return TermGroupName.CompareTo(obj.TermGroupName);
        }
    }


    public class CreditParameter : IComparable<CreditParameter>
    {
        public int ParamId;
        public string ParamName;
        public ParamType ParamType;
        public bool ParamUsed;
        public TermGroup termGroup;// = new TermGroup();


        public CreditParameter(int id, string name, string type, int used, TermGroup term_group)
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
            termGroup = term_group;
           
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


  

    /*public class TermGroup : IComparable<TermGroup>
    {
        public int TermGroupId;
        public string TermGroupName;
        public List<Term> Terms = new List<Term>();

        public TermGroup(int id, string name, List<Term> p_term)
        {
            TermGroupId = id;
            TermGroupName = name;
            Terms = p_term;
        }

        public override string ToString()
        {
            return TermGroupName;
        }
    }*/

    public class Parameter : CreditParameter
    {
        public string Question;
        
        public Parameter(int id, string name, string type, int used, string question, TermGroup term_group)
            : base(id, name, type, used, term_group)
        {
            Question = question;
        }

    }

}
