﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    public enum Param_type
    {
        [StringValue("int")]
        p_int,
        [StringValue("string")]
        p_string,
        [StringValue("bool")]
        p_bool
    }

    public class param
    {
        public int param_id;
        public string param_name;
        public Param_type param_type;
        public param(int id, string name, string type)
        {
            param_id = id;
            param_name = name;
            switch (type)
            {
                case "int": param_type = Param_type.p_int; break;
                case "string": param_type = Param_type.p_string; break;
                case "bool": param_type = Param_type.p_bool; break;
            }
        }
    }

}
