using System;
using System.Collections.Generic;

namespace Closhure
{

    public class Environment
    {
        internal Dictionary<int, object> env = new Dictionary<int, object>();
        internal Environment outer;

        internal Environment()
        {
            this.outer = null;
        }

        internal Environment(Environment outer)
        {
            this.outer = outer;
        }

        internal virtual object get(int code)
        {
            if (env.ContainsKey(code))
            {
                return env[code];
            }
            else
            {
                if (outer != null)
                {
                    return outer.get(code);
                }
                else
                {
                    Type r = Core.tryGetClass(Symbol.symname[code]);
                    if (r != null)
                    {
                        return r;
                    }
                    else
                    {
                        throw new Exception("Unable to resolve symbol: " + Symbol.symname[code]);
                    }
                }
            }
        }

        // change an existing variable
        internal virtual object set(int code, object v)
        {
            if (env.ContainsKey(code))
            {
                env[code] = v;
                return v;
            }
            else
            {
                if (outer != null)
                {
                    return outer.set(code, v);
                }
                else
                {
                    throw new Exception("Unable to resolve symbol: " + Symbol.symname[code]);
                }
            }
        }

        // define a new variable
        internal virtual object def(int code, object v)
        {
            env[code] = v;
            return v;
        }
    }
}
