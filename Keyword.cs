using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Closhure
{
    public class Keyword
    {
        internal Symbol symbol;

        public Keyword(string x)
        {
            this.symbol = new Symbol(x);
        }

        public override bool Equals(object x)
        {
            return x is Keyword && symbol.Equals(((Keyword)x).symbol);
        }

        public override int GetHashCode() { return symbol.code; }

        public override string ToString()
        {
            return symbol.ToString();
        }
    }

}
