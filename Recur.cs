using System;
using System.Collections.Generic;

namespace Closhure
{
    public class Recur : Exception
    {
        private const long serialVersionUID = 1L;
        internal List<object> args;

        public Recur(List<object> args)
        {
            this.args = args;
        }

        // the message shown when not caught
        public override string ToString()
        {
            return "recur is used outside a fn or a loop";
        }
    }
}
