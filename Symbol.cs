using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Closhure.Builtin;

namespace Closhure
{
    public class Symbol
    {
        public static Dictionary<string, int> symcode = new Dictionary<string, int>();
        public static List<string> symname = new List<string>();
        public int code;

        public static int toCode(string name)
        {
            int r;
            if (symcode.TryGetValue(name, out r))
            {
                return r;
            }
            else
            {
                r = symcode.Count;
                symcode[name] = r;
                symname.Add(name);
                return r;
            }
        }

        public Symbol(string name)
        {
            code = toCode(name);
        }

        public override string ToString()
        {
            return symname[code];
        }

        public override bool Equals(object o)
        {
            return o is Symbol && code == ((Symbol)o).code;
        }

        public override int GetHashCode() { return code; }
    }

}
