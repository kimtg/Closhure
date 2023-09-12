using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Closhure
{
    internal class Builtin
    {
        public static Type coerceNumberType(IList<object> args)
        {
            Type r = typeof(int);
            foreach (object x in args)
            {
                if (x is double?)
                {
                    return typeof(double);
                }
                else if (x is long?)
                {
                    r = typeof(long);
                }
            }
            return r;
        }
        internal class _plus : Fn
        {
            public override object Invoke(IList<object> args)
            {
                int len = args.Count;
                if (len <= 0)
                {
                    return 0;
                }
                object type = coerceNumberType(args);
                object first = args[0];
                if ((Type)type == typeof(int))
                {
                    int acc = Core.intValue(first);
                    for (int i = 1; i < len; i++)
                    {
                        acc += Core.intValue(args[i]);
                    }
                    return acc;
                }
                else if ((Type)type == typeof(long))
                {
                    long acc = Core.longValue(first);
                    for (int i = 1; i < len; i++)
                    {
                        acc += Core.longValue(args[i]);
                    }
                    return acc;
                }
                else
                {
                    double acc = Core.doubleValue(first);
                    for (int i = 1; i < len; i++)
                    {
                        acc += Core.doubleValue(args[i]);
                    }
                    return acc;
                }
            }
        }
        internal class _minus : Fn
        {
            public override object Invoke(IList<object> args)
            {
                int len = args.Count;
                if (len <= 0)
                {
                    return 0;
                }
                object type = coerceNumberType(args);
                object first = args[0];
                if ((Type)type == typeof(int))
                {
                    int acc = Core.intValue(first);
                    if (len == 1)
                    {
                        return -acc;
                    }
                    for (int i = 1; i < len; i++)
                    {
                        acc -= Core.intValue(args[i]);
                    }
                    return acc;
                }
                else if ((Type)type == typeof(long))
                {
                    long acc = Core.longValue(first);
                    if (len == 1)
                    {
                        return -acc;
                    }
                    for (int i = 1; i < len; i++)
                    {
                        acc -= Core.longValue(args[i]);
                    }
                    return acc;
                }
                else
                {
                    double acc = Core.doubleValue(first);
                    if (len == 1)
                    {
                        return -acc;
                    }
                    for (int i = 1; i < len; i++)
                    {
                        acc -= Core.doubleValue(args[i]);
                    }
                    return acc;
                }
            }
        }
        internal class _star : Fn
        {
            public override object Invoke(IList<object> args)
            {
                int len = args.Count;
                if (len <= 0)
                {
                    return 1;
                }
                object type = coerceNumberType(args);
                object first = args[0];
                if ((Type)type == typeof(int))
                {
                    int acc = Core.intValue(first);
                    for (int i = 1; i < len; i++)
                    {
                        acc *= Core.intValue(args[i]);
                    }
                    return acc;
                }
                else if ((Type)type == typeof(long))
                {
                    long acc = Core.longValue(first);
                    for (int i = 1; i < len; i++)
                    {
                        acc *= Core.longValue(args[i]);
                    }
                    return acc;
                }
                else
                {
                    double acc = Core.doubleValue(first);
                    for (int i = 1; i < len; i++)
                    {
                        acc *= Core.doubleValue(args[i]);
                    }
                    return acc;
                }
            }
        }

        // always use doubleValue
        internal class _slash : Fn
        {
            public override object Invoke(IList<object> args)
            {
                int len = args.Count;
                if (len <= 0)
                {
                    return 1;
                }
                object first = args[0];
                double acc = Core.doubleValue(first);
                if (len == 1)
                {
                    return 1 / acc;
                }
                for (int i = 1; i < len; i++)
                {
                    acc /= Core.doubleValue(args[i]);
                }
                return acc;
            }
        }

        // quotient
        internal class quot : Fn
        {
            public override object Invoke(IList<object> args)
            {
                int len = args.Count;
                if (len <= 0)
                {
                    return 1;
                }
                object first = args[0];
                long acc = Core.longValue(first);
                if (len == 1)
                {
                    return 1 / acc;
                }
                for (int i = 1; i < len; i++)
                {
                    acc /= Core.longValue(args[i]);
                }
                return acc;
            }
        }

        internal class mod : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object first = args[0];
                object second = args[1];
                object type = coerceNumberType(args);
                if ((Type)type == typeof(int))
                {
                    return Core.intValue(first) % Core.intValue(second);
                }
                else if ((Type)type == typeof(long))
                {
                    return Core.longValue(first) % Core.longValue(second);
                }
                else
                {
                    return Core.doubleValue(first) % Core.doubleValue(second);
                }
            }
        }

        internal class _eq : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object v1 = args[0];
                if (v1 == null)
                {
                    for (int i = 1; i < args.Count; i++)
                    {
                        object v2 = args[i];
                        if (v2 != null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < args.Count; i++)
                    {
                        object v2 = args[i];
                        if (!v1.Equals(v2))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        internal class _eq_eq : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object first = args[0];
                double firstv = Core.doubleValue(first);
                for (int i = 1; i < args.Count; i++)
                {
                    if (Core.doubleValue(args[i]) != firstv)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal class Not_eq : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object v1 = args[0];
                if (v1 == null)
                {
                    for (int i = 1; i < args.Count; i++)
                    {
                        object v2 = args[i];
                        if (v2 != null)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < args.Count; i++)
                    {
                        object v2 = args[i];
                        if (!v1.Equals(v2))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        internal class _lt : Fn
        {
            public override object Invoke(IList<object> args)
            {
                for (int i = 0; i < args.Count - 1; i++)
                {
                    object first = args[i];
                    object second = args[i + 1];
                    if (!(Core.doubleValue(first) < Core.doubleValue(second)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal class _gt : Fn
        {
            public override object Invoke(IList<object> args)
            {
                for (int i = 0; i < args.Count - 1; i++)
                {
                    object first = args[i];
                    object second = args[i + 1];
                    if (!(Core.doubleValue(first) > Core.doubleValue(second)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal class _lt_eq : Fn
        {
            public override object Invoke(IList<object> args)
            {
                for (int i = 0; i < args.Count - 1; i++)
                {
                    object first = args[i];
                    object second = args[i + 1];
                    if (!(Core.doubleValue(first) <= Core.doubleValue(second)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal class _gt_eq : Fn
        {
            public override object Invoke(IList<object> args)
            {
                for (int i = 0; i < args.Count - 1; i++)
                {
                    object first = args[i];
                    object second = args[i + 1];
                    if (!(Core.doubleValue(first) >= Core.doubleValue(second)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal class not : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return !Core.booleanValue(args[0]);
            }
        }

        internal class read_string : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return Core.parse(new StringReader((string)args[0]));
            }
        }

        internal class type : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return Core.type(args[0]);
            }
        }

        internal class eval : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return Core.macroexpandEval(args[0], Core.globalEnv);
            }
        }

        internal class fold : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object f = args[0];
                IEnumerable<object> iterable = Core.iterableValue(args[1]);
                IEnumerator<object> iter = iterable.GetEnumerator();
                iter.MoveNext();
                object acc = iter.Current;
                List<object> args2 = new List<object>()
                {
                    null, // first argument
                    null // second argument
                }; // (ITEM1 ITEM2)
                while (iter.MoveNext())
                {
                    args2[0] = acc;
                    args2[1] = iter.Current;
                    acc = Core.apply(f, args2);
                }
                return acc;
            }
        }

        internal class map : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object f = args[0];
                IEnumerable<object> iterable = Core.iterableValue(args[1]);
                List<object> acc = new List<object>();
                List<object> args2 = new List<object>();
                args2.Add(null);
                foreach (object x in iterable)
                {
                    args2[0] = x;
                    acc.Add(Core.apply(f, args2));
                }
                return acc;
            }
        }

        internal class apply : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object args2 = args[1];
                IList<object> argsList;
                if (args2 is List<object>)
                {
                    argsList = (IList<object>) Core.listValue(args2);
                }
                else
                {
                    argsList = new List<object>();
                    foreach (object x in Core.iterableValue(args2))
                    {
                        argsList.Add(x);
                    }
                }
                return Core.apply(args[0], argsList);
            }
        }

        internal class filter : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object f = args[0];
                IEnumerable<object> iterable = Core.iterableValue(args[1]);
                List<object> acc = new List<object>();
                List<object> args2 = new List<object>();
                args2.Add(null);
                foreach (object x in iterable)
                {
                    args2[0] = x;
                    object ret = Core.apply(f, args2);
                    if (Core.booleanValue(ret))
                    {
                        acc.Add(x);
                    }
                }
                return acc;
            }
        }

        internal class pr : Fn
        {
            public override object Invoke(IList<object> args)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    if (i != 0)
                    {
                        Console.Write(" ");
                    }
                    Console.Write(Core.toReadableString(args[i]));
                }
                return null;
            }
        }

        internal static readonly pr pr1 = new pr();

        internal class prn : Fn
        {
            public override object Invoke(IList<object> args)
            {
                pr1.Invoke(args);
                Console.WriteLine();
                return null;
            }
        }

        internal class print : Fn
        {
            public override object Invoke(IList<object> args)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    if (i != 0)
                    {
                        Console.Write(" ");
                    }
                    Console.Write(args[i]);
                }
                return null;
            }
        }

        internal static readonly print print1 = new print();

        internal class println : Fn
        {
            public override object Invoke(IList<object> args)
            {
                print1.Invoke(args);
                Console.WriteLine();
                return null;
            }
        }

        internal class read_line : Fn
        {
            public override object Invoke(IList<object> args)
            {
                try
                {
                    return Console.ReadLine();
                }
                catch (IOException)
                {
                    return null;
                }
            }
        }

        // (slurp filename [encoding]) default encoding: UTF-8
        internal class slurp : Fn
        {
            public override object Invoke(IList<object> args)
            {
                string filename = Core.toString(args[0]);
                string charset = args.Count >= 2 ? args[args.Count - 1].ToString() : "UTF-8";
                return Core.slurp(filename, charset);
            }
        }

        internal class spit : Fn
        {
            public override object Invoke(IList<object> args)
            {
                string filename = Core.toString(args[0]);
                string str = Core.toString(args[1]);
                return Core.spit(filename, str);
            }
        }

        internal class list : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return args;
            }
        }

        internal class str : Fn
        {
            public override object Invoke(IList<object> args)
            {
                StringBuilder sb = new StringBuilder();
                foreach (object x in args)
                {
                    if (x != null)
                    {
                        sb.Append(x.ToString());
                    }
                }
                return sb.ToString();
            }
        }

        internal class symbol : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return new Symbol(Core.toString(args[0]));
            }
        }

        // (macroexpand X)
        internal class macroexpand : Fn
        {
            public override object Invoke(IList<object> args)
            {
                return Core.macroexpand(args[0]);
            }
        }

        // (read [Reader])
        internal class read : Fn
        {
            public override object Invoke(IList<object> args)
            {
                switch (args.Count)
                {
                    case 0:
                        //return Core.parse(Console.In); // bugged
                        return Core.parse(new MyReader(Console.In));
                    case 1:
                        return Core.parse((TextReader)args[0]);
                    default:
                        throw new System.ArgumentException();
                }
            }
        }

        // (load-string STRING)
        internal class load_string : Fn
        {
            public override object Invoke(IList<object> args)
            {
                switch (args.Count)
                {
                    case 1:
                        return Core.load_string((string)args[0]);
                    default:
                        throw new System.ArgumentException();
                }
            }
        }

        // (nth COLL INDEX)
        internal class nth : Fn
        {
            public override object Invoke(IList<object> args)
            {
                object coll = args[0];
                if (coll is System.Collections.IList)
                {
                    return ((IList<object>)coll)[Core.intValue(args[1])];
                }
                else if (coll is System.Collections.IEnumerable)
                {
                    int index = Core.intValue(args[1]);
                    IEnumerator<object> iter = ((IEnumerable<object>)coll).GetEnumerator();
                    for (int i = 0; i < index; i++)
                    {
                        iter.MoveNext();
                    }
                    return iter.MoveNext();
                }
                else if (coll.GetType().IsArray)
                {
                    return ((Array)coll).GetValue(Core.intValue(args[1]));
                }
                else
                {
                    throw new System.ArgumentException();
                }
            }
        }

        // (instance? c x)
        // Evaluates x and tests if it is an instance of the class c. Returns true or false
        internal class instance_q : Fn
        {
            public override object Invoke(IList<object> args)
            {
                if (args.Count != 2)
                {
                    throw new System.ArgumentException();
                }
                return ((Type)args[0]).IsInstanceOfType(args[1]);
            }
        }

        internal class range : Fn
        {
            public override object Invoke(IList<object> args)
            {
                switch (args.Count)
                {
                    case 1:
                        {
                            List<object> r = new List<object>();
                            for (int i = 0; i < (int)args[0]; i++) r.Add(i);
                            return r;
                        }                        
                    case 2:
                        {
                            List<object> r = new List<object>();
                            for (double i = Convert.ToDouble(args[0]); i < Convert.ToDouble(args[1]); i++) r.Add(i);
                            return r;
                        }
                    case 3:
                        {
                            List<object> r = new List<object>();
                            for (double i = Convert.ToDouble(args[0]); i < Convert.ToDouble(args[1]); i += Convert.ToDouble(args[2])) r.Add(i);
                            return r;
                        }
                    default:
                        throw new ArgumentException();
                }
            }
        }
    }
}
