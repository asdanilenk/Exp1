using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exp1
{
    class Credit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Credit(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
