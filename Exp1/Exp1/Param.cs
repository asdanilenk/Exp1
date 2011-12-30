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
        PBool
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

    public class Parameter : CreditParameter
    {
        public string Question;

        public Parameter(int id, string name, string type, int used, string question) : base(id,name,type,used)
        {
            Question = question;
        }
    }

}
