using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    public enum Param_type
    {
        [StringValue("double")]
        p_double,
        [StringValue("string")]
        p_string,
        [StringValue("bool")]
        p_bool
    }

    public class CreditParameter : IComparable<CreditParameter>
    {
        public int param_id;
        public string param_name;
        public Param_type param_type;
        public bool param_used;

        public CreditParameter(int id, string name, string type, int used)
        {
            param_id = id;
            param_name = name;
            switch (type)
            {
                case "int":
                case "double": param_type = Param_type.p_double; break;
                case "string": param_type = Param_type.p_string; break;
                case "bool": param_type = Param_type.p_bool; break;
            }
            switch (used)
            {
                case 1: param_used = true; break;
                case 0: param_used = false; break;
            }
        }

        public int CompareTo(CreditParameter obj)
        {
            return param_name.CompareTo(obj.param_name);
        }

        public override string ToString()
        {
            return param_name;
        }
    }

    public class Parameter : CreditParameter
    {
        public string question;

        public Parameter(int id, string name, string type, int used, string question) : base(id,name,type,used)
        {
            this.question = question;
        }
    }

}
