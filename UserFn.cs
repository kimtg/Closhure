using Closhure;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Closhure
{

    internal class UserFn : Fn
    { // anonymous function
        internal IList<object> def; // definition
        internal Environment outer_env;

        internal UserFn(IList<object> def, Environment outer_env)
        {
            this.def = def;
            this.outer_env = outer_env;
        }

        public override string ToString()
        {
            IList<object> d = new List<object>();
            d.Add(Core.sym_fn);
            ((List<object>)d).AddRange(def);
            return Core.toReadableString(d);
        }

        public override object Invoke(IList<object> args)
        {
            // anonymous function application. lexical scoping
            // ((ARGUMENT ...) BODY ...)
            while (true)
            {
                Environment local_env = new Environment(this.outer_env);
                IList arg_syms = (IList)this.def[0];

                int len = arg_syms.Count;
                for (int i = 0; i < len; i++)
                { // assign arguments
                    Symbol sym = (Symbol)arg_syms[i];
                    if (sym.ToString().Equals("&"))
                    { // variadic arguments
                        sym = Core.symbolValue(arg_syms[i + 1]);
                        local_env.def(sym.code, new List<object>(args.Skip(i)));
                        break;
                    }
                    object n2 = args[i];
                    local_env.def(sym.code, n2);
                }

                len = this.def.Count;
                object ret = null;
                for (int i = 1; i < len; i++)
                { // body
                    try
                    {
                        ret = Core.eval(this.def[i], local_env);
                    }
                    catch (Recur e)
                    {
                        args = e.args;
                        goto fnStartContinue; // recur this function (effectively goto)
                    }
                }
                return ret;
            fnStartContinue:;
            }
        }
    }
}
