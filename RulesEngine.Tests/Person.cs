using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine.Tests
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double Age { get; set; }
        public List<string> Hobbies { get; set; }

        public bool Vaccinated { get; set; }
    }
}
