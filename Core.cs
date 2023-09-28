using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Closhure
{
    public sealed class Core
    {
        public const string VERSION = "0.6";

        // no instance
        private Core()
        {

        }

        internal static readonly Symbol sym_set_e = new Symbol("set!");
        internal static readonly Symbol sym_def = new Symbol("def");
        internal static readonly Symbol sym_and = new Symbol("and");
        internal static readonly Symbol sym_or = new Symbol("or");
        internal static readonly Symbol sym_if = new Symbol("if");
        internal static readonly Symbol sym_quote = new Symbol("quote");
        internal static readonly Symbol sym_fn = new Symbol("fn");
        internal static readonly Symbol sym_do = new Symbol("do");
        internal static readonly Symbol sym__dot = new Symbol(".");
        internal static readonly Symbol sym_new = new Symbol("new");
        internal static readonly Symbol sym_doseq = new Symbol("doseq");
        internal static readonly Symbol sym_let = new Symbol("let");
        internal static readonly Symbol sym_import = new Symbol("import");
        internal static readonly Symbol sym_reify = new Symbol("reify");
        internal static readonly Symbol sym_recur = new Symbol("recur");
        internal static readonly Symbol sym_loop = new Symbol("loop");
        internal static readonly Symbol sym_quasiquote = new Symbol("quasiquote");
        internal static readonly Symbol sym_unquote = new Symbol("unquote");
        internal static readonly Symbol sym_unquote_splicing = new Symbol("unquote-splicing");
        internal static readonly Symbol sym_try = new Symbol("try");
        internal static readonly Symbol sym_catch = new Symbol("catch");
        internal static readonly Symbol sym_finally = new Symbol("finally");
        internal static readonly Symbol sym_defmacro = new Symbol("defmacro");

        internal static Environment globalEnv = new Environment(); // variables. compile-time
        internal static List<string> imports = new List<string>();
        internal static Dictionary<string, UserFn> macros = new Dictionary<string, UserFn>();
        internal static Dictionary<string, Type> getClassCache = new Dictionary<string, Type>();

        public static object testField;
        public static object testProperty { get; set; }

        static Core()
        {
            set("+", new Builtin._plus());
            set("-", new Builtin._minus());
            set("*", new Builtin._star());
            set("/", new Builtin._slash());
            set("quot", new Builtin.quot());
            set("mod", new Builtin.mod());
            set("=", new Builtin._eq());
            set("==", new Builtin._eq_eq());
            set("not=", new Builtin.Not_eq());
            set("<", new Builtin._lt());
            set(">", new Builtin._gt());
            set("<=", new Builtin._lt_eq());
            set(">=", new Builtin._gt_eq());
            set("not", new Builtin.not());
            set("read-string", new Builtin.read_string());
            set("type", new Builtin.type());
            set("eval", new Builtin.eval());
            set("list", new Builtin.list());
            set("apply", new Builtin.apply());
            set("fold", new Builtin.fold());
            set("map", new Builtin.map());
            set("filter", new Builtin.filter());
            set("pr", Builtin.pr1);
            set("prn", new Builtin.prn());
            set("print", Builtin.print1);
            set("println", new Builtin.println());
            set("read-line", new Builtin.read_line());
            set("slurp", new Builtin.slurp());
            set("spit", new Builtin.spit());
            set("str", new Builtin.str());
            set("symbol", new Builtin.symbol());
            set("macroexpand", new Builtin.macroexpand());
            set("read", new Builtin.read());
            set("load-string", new Builtin.load_string());
            set("nth", new Builtin.nth());
            set("instance?", new Builtin.instance_q());
            set("range", new Builtin.range());

            try
            {
                load_string(
@"(import System)
(defmacro defn (name & body) `(def ~name (fn ~@body)))
(defmacro when (cond & body) `(if ~cond (do ~@body)))
(defn nil? (x) (= nil x))
(defmacro while (test & body) `(loop () (when ~test ~@body (recur))))
(def gensym
  (let (gs-counter 0)
    (fn ()
      (symbol (str ""G__"" (set! gs-counter (+ gs-counter 1)))))))
(defmacro dotimes (binding & body)
  (let (g (gensym), var (binding 0), limit (binding 1))
    `(let (~g ~limit) (loop (~var 0) (when (< ~var ~g) ~@body (recur (+ ~var 1)))))))
(defn load-file (file) (load-string (slurp file)))");
            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                //Console.Write(e.StackTrace);
            }
        }

        internal static int intValue(object value)
        {
            if (value is int || value is long || value is double)
            {
                return Convert.ToInt32(value);
            }
            else
            {
                return int.Parse(value.ToString());
            }
        }

        internal static double doubleValue(object value)
        {
            if (value is int || value is long || value is double)
            {
                return Convert.ToDouble(value);
            }
            else
            {
                return double.Parse(value.ToString());
            }
        }

        internal static long longValue(object value)
        {
            if (value is int || value is long || value is double)
            {
                return Convert.ToInt64(value);
            }
            else
            {
                return long.Parse(value.ToString());
            }
        }

        internal static bool booleanValue(object value)
        { // null is false, other type is true.
            if (value == null)
            {
                return false;
            }
            if (value is bool?)
            {
                return ((bool?)value).Value;
            }
            else
            {
                return true;
            }
        }

        internal static IList listValue(object value)
        {
            return (IList)value;
        }

        internal static IEnumerable<object> iterableValue(object value)
        {
            return (IEnumerable<object>)value;
        }

        internal static Type type(object value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return value.GetType();
            }
        }

        internal static string toReadableString(object value)
        {
            if (value == null)
            {
                return "nil";
            }
            else if (value is string)
            {
                return "\"" + escape((string)value) + "\"";
            }
            else if (value is Regex)
            {
                return "#\"" + escape(((Regex)value).ToString()) + "\"";
            }
            else if (value is System.Collections.IList)
            {
                string openParen = "(", closeParen = ")";
                if (value is ArrayList) // []
                {
                    openParen = "[";
                    closeParen = "]";
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(openParen);
                bool first = true;
                foreach (object o in (IList)value)
                {
                    if (!first)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(toReadableString(o));
                    first = false;
                }
                sb.Append(closeParen);
                return sb.ToString();
            }
            else if (value is Type)
            {
                return ((Type)value).FullName;
            }
            else if (value is bool)
            {
                return (bool)value ? "true" : "false";
            }
            else if (value is char)
            {
                switch ((char)value)
                {
                    case '\n': return "\\newline";
                    case ' ': return "\\space";
                    case '\t': return "\\tab";
                    case '\f': return "\\formfeed";
                    case '\b': return "\\backspace";
                    case '\r': return "\\return";
                    default:
                       return "\\" + value.ToString();
                }
            }
            else
            {
                return value.ToString();
            }
        }

        internal static string escape(string value)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string toString(object value)
        {
            return value.ToString();
        }

        public static Symbol symbolValue(object value)
        {
            return (Symbol)value;
        }

        internal static object apply(object func, IList<object> args)
        {
            if (func is Fn)
            {
                return ((Fn)func).Invoke(args);
            }
            else
            {
                // implicit indexing
                if (func is IList<object>)
                {
                    return ((IList<object>)func)[Core.intValue(args[0])];
                }
                else if (func.GetType().IsArray)
                {
                    return ((Array)func).GetValue(Core.intValue(args[0]));
                }
                else
                {
                    Console.Error.WriteLine("Unknown function: [" + func.ToString() + "]");
                    return null;
                }
            }
        }

        internal static void printCollection(ICollection<string> coll)
        {
            foreach (string key in new SortedSet<string>(coll))
            {
                Console.Write(" " + key);
            }
            Console.WriteLine();
        }

        public static void printLogo()
        {
            Console.WriteLine("Closhure " + VERSION);
            Console.WriteLine("Special forms:");
            List<string> fields = new List<string>();
            foreach (System.Reflection.FieldInfo f in typeof(Core).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (f.FieldType == typeof(Symbol))
                    fields.Add(f.GetValue(null).ToString());
            }
            printCollection(fields);

            List<string> functions = new List<string>();
            Console.WriteLine("Defined symbols:");
            foreach (int x in globalEnv.env.Keys)
            {
                functions.Add(Symbol.symname[x]);
            }
            printCollection(functions);

            Console.WriteLine("Macros:");
            printCollection(macros.Keys);
        }

        internal static object macroexpand(object n)
        {
            if (n is List<object>)
            {
                IList<object> expr = (IList<object>)Core.listValue(n);
                if (expr.Count == 0)
                {
                    return n;
                }
                object prefix = expr[0];
                string ps = prefix.ToString();
                if (prefix is Symbol && ps.Equals("quote"))
                {
                    return n;
                }
                if (prefix is Symbol && ps.Length >= 3 && ps.Contains("/"))
                { // e.g. (Math/Cos 0) -> (. Math Cos 0)
                    int sepPos = ps.IndexOf('/');
                    string head = ps.Substring(0, sepPos);
                    string tail = ps.Substring(sepPos + 1);
                    List<object> newForm = new List<object>();
                    newForm.Add(sym__dot);
                    newForm.Add(new Symbol(head));
                    newForm.Add(new Symbol(tail));
                    for (int i = 1; i < expr.Count; i++)
                    {
                        newForm.Add(expr[i]);
                    }
                    return macroexpand(newForm);
                }
                if (prefix is Symbol && ps.Length >= 2 && ps.StartsWith(".", StringComparison.Ordinal))
                { // e.g.(.ToLower "abc") -> (. "abc" ToLower)
                    string tail = ps.Substring(1);
                    List<object> newForm = new List<object>();
                    newForm.Add(sym__dot);
                    newForm.Add(expr[1]);
                    newForm.Add(new Symbol(tail));
                    for (int i = 2; i < expr.Count; i++)
                    {
                        newForm.Add(expr[i]);
                    }
                    return macroexpand(newForm);
                }
                if (prefix is Symbol && ps.Length >= 2 && ps.EndsWith(".", StringComparison.Ordinal))
                { // e.g. (Date.)
                  // (new Date)
                    string head = ps.Substring(0, ps.Length - 1);
                    List<object> newForm = new List<object>();
                    newForm.Add(sym_new);
                    newForm.Add(new Symbol(head));
                    for (int i = 1; i < expr.Count; i++)
                    {
                        newForm.Add(expr[i]);
                    }
                    return macroexpand(newForm);
                }
                UserFn func;
                if (prefix is Symbol && macros.TryGetValue(ps, out func))
                {
                    // build arguments
                    List<object> args = new List<object>();
                    int len = expr.Count;
                    for (int i = 1; i < len; i++)
                    {
                        args.Add(expr[i]);
                    }
                    object r = apply(func, args);
                    return macroexpand(r); // macroexpand again
                }
                else
                {
                    // macroexpand elements
                    List<object> r = new List<object>();
                    foreach (object n2 in expr)
                    {
                        r.Add(macroexpand(n2));
                    }
                    return r;
                }
            }
            else if (n is ArrayList)
            {
                // macroexpand elements
                IList expr = Core.listValue(n);
                ArrayList r = new ArrayList();
                foreach (object n2 in expr)
                {
                    r.Add(macroexpand(n2));
                }
                return r;
            }
            else if (n is Symbol)
            {
                string ns = n.ToString();
                if (ns.Length >= 3 && ns.Contains("/"))
                { // e.g. Math/PI
                  // (. Math -PI)
                    int sepPos = ns.IndexOf('/');
                    string head = ns.Substring(0, sepPos);
                    string tail = ns.Substring(sepPos + 1);
                    List<object> newForm = new List<object>();
                    newForm.Add(sym__dot);
                    newForm.Add(new Symbol(head));
                    newForm.Add(new Symbol("-" + tail));
                    return macroexpand(newForm);
                }
            }
            // no expansion
            return n;
        }

        internal static object eval(object n, Environment env)
        {
        startEval:
            if (n is Symbol)
            {
                object r = env.get(((Symbol)n).code);
                return r;
            }
            else if (n is ArrayList) // []
            {
                List<object> r = new List<object>();
                foreach (object x in (ArrayList)n)
                {
                    r.Add(eval(x, env));
                }
                return r;
            }
            else if (n is List<object>) // ()
            { // function (FUNCTION
              // ARGUMENT ...)
                IList<object> expr = (IList<object>)Core.listValue(n);
                if (expr.Count == 0)
                {
                    return null;
                }
                object e0 = expr[0];
                if (e0 is Symbol)
                {
                    int code = ((Symbol)e0).code;
                    if (code == sym_set_e.code)
                    { // (set! SYMBOL-OR-FIELD VALUE) ; set the SYMBOL-OR-FIELD's value
                        object dest = expr[1];
                        if (dest is Symbol)
                        {
                            object value = eval(expr[2], env);
                            env.set(((Symbol)dest).code, value);
                            return value;
                        }
                        else
                        { // field
                          // host interoperability
                          // (set! (. CLASS-OR-object -FIELD INDEX*) VALUE) ; set host field or property. INDEX is optional (0 or more).
                            try
                            {
                                IList<object> dl = (List<object>)dest;
                                // get class
                                Type cls = tryGetClass(dl[1].ToString());
                                object obj = null;
                                if (cls != null)
                                {
                                    // class's static method e.g. (. SystemMath Floor 1.5)
                                }
                                else
                                {
                                    // object's method e.g. (. "abc" -Length)
                                    obj = eval(dl[1], env);
                                    cls = obj.GetType();
                                }

                                object value = eval(expr[2], env);
                                string fieldName = dl[2].ToString().Substring(1);
                                System.Reflection.FieldInfo field = cls.GetField(fieldName);
                                if (field != null)
                                {
                                    field.SetValue(obj, value);
                                }
                                else
                                {
                                    System.Reflection.PropertyInfo property = cls.GetProperty(fieldName);
                                    var index = dl.Skip(3);
                                    property.SetValue(obj, value, index.ToArray());
                                }
                                return value;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                //Console.Write(e.StackTrace);
                                return null;
                            }
                        }
                    }
                    else if (code == sym_def.code)
                    { // (def SYMBOL VALUE ...) ; set in the global
                      // environment
                        object ret = null;
                        int len1 = expr.Count;
                        for (int i = 1; i < len1; i += 2)
                        {
                            object value = eval(expr[i + 1], env);
                            ret = globalEnv.def(((Symbol)expr[i]).code, value);
                        }
                        return ret;
                    }
                    else if (code == sym_and.code)
                    { // (and X ...) short-circuit
                        for (int i = 1; i < expr.Count; i++)
                        {
                            if (!Core.booleanValue(eval(expr[i], env)))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    else if (code == sym_or.code)
                    { // (or X ...) short-circuit
                        for (int i = 1; i < expr.Count; i++)
                        {
                            if (Core.booleanValue(eval(expr[i], env)))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    else if (code == sym_if.code)
                    { // (if CONDITION THEN_EXPR [ELSE_EXPR])
                        object cond = expr[1];
                        if (Core.booleanValue(eval(cond, env)))
                        {
                            // return eval(expr[2], env);

                            // tail-call optimization
                            n = expr[2];
                            goto startEval;
                        }
                        else
                        {
                            if (expr.Count <= 3)
                            {
                                return null;
                            }
                            //return eval(expr[3], env);

                            // tail-call optimization
                            n = expr[3];
                            goto startEval;
                        }
                    }
                    else if (code == sym_quote.code)
                    { // (quote X)
                        return expr[1];
                    }
                    else if (code == sym_fn.code)
                    {
                        // anonymous function. lexical scoping
                        // (fn (ARGUMENT ...) BODY ...)
                        List<object> r = new List<object>();
                        for (int i = 1; i < expr.Count; i++)
                        {
                            r.Add(expr[i]);
                        }
                        return new UserFn(r, env);
                    }
                    else if (code == sym_do.code)
                    { // (do X ...)
                        int last = expr.Count - 1;
                        if (last <= 0)
                        {
                            return null;
                        }
                        for (int i = 1; i < last; i++)
                        {
                            eval(expr[i], env);
                        }
                        //return eval(expr[last], env);

                        // tail-call optimization
                        n = expr[last];
                        goto startEval;
                    }
                    else if (code == sym__dot.code)
                    {
                        string methodName = expr.ElementAt(2).ToString();
                        if (methodName.StartsWith("-", StringComparison.Ordinal)) // field
                        {
                            // host interoperability
                            // (. CLASS-OR-object -FIELD INDEX*) ; get host field or property. INDEX is optional (0 or more).
                            try
                            {
                                // get class
                                Type cls = tryGetClass(expr.ElementAt(1).ToString());
                                object obj = null;
                                if (cls != null)
                                {
                                    // class's static method e.g. (. System.Math Floor 1.5)
                                }
                                else
                                {
                                    // object's method e.g. (. [1 2] Count)
                                    obj = eval(expr.ElementAt(1), env);
                                    cls = obj.GetType();
                                }

                                // try field
                                string fieldName = methodName.Substring(1);
                                System.Reflection.FieldInfo field = cls.GetField(fieldName);

                                if (field != null)
                                {
                                    return field.GetValue(obj);
                                }
                                else
                                {
                                    // property e.g. (. "abc" -Length)
                                    System.Reflection.PropertyInfo property = cls.GetProperty(fieldName);
                                    var index = expr.Skip(3);
                                    return property.GetValue(obj, index.ToArray());
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                                //Console.Write(e.StackTrace);
                                return null;
                            }
                        }
                        // host interoperability
                        // (. CLASS-OR-object METHOD ARGUMENT ...) ; host method invocation
                        try
                        {
                            // get class
                            Type cls = tryGetClass(expr.ElementAt(1).ToString());
                            object obj = null;
                            if (expr.ElementAt(1) is Symbol && cls != null)
                            {
                                // class's static method e.g. (. System.Math Floor 1.5)
                            }
                            else
                            {
                                // object's method e.g. (. "abc" length)
                                obj = eval(expr.ElementAt(1), env);
                                cls = obj.GetType();
                            }

                            Type[] parameterTypes = new Type[expr.Count - 3];
                            List<object> parameters = new List<object>();
                            int last = expr.Count - 1;
                            for (int i = 3; i <= last; i++)
                            {
                                object a = eval(expr.ElementAt(i), env);
                                object param = a;
                                parameters.Add(param);
                                parameterTypes[i - 3] = param.GetType();
                            }

                            try
                            {
                                System.Reflection.MethodInfo m = cls.GetMethod(methodName, parameterTypes);
                                return m.Invoke(obj, parameters.ToArray());
                            }
                            catch (TargetException)
                            {
                                foreach (System.Reflection.MethodInfo m in cls.GetMethods())
                                {
                                    // find a method with the same number of parameters
                                    if (m.Name.Equals(methodName) && m.GetParameters().Length == expr.Count - 3)
                                    {
                                        try
                                        {
                                            return m.Invoke(obj, parameters.ToArray());
                                        }
                                        catch (System.ArgumentException)
                                        {
                                            // try next method
                                            continue;
                                        }
                                    }
                                }
                            }
                            throw new System.ArgumentException(expr.ToString());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            //Console.Write(e.StackTrace);
                            return null;
                        }
                    }

                    else if (code == sym_new.code)
                    {
                        // host interoperability
                        // (new CLASS ARG ...) ; create new host object

                        string className = expr.ElementAt(1).ToString();
                        Type cls = getClass(className);
                        Type[] parameterTypes = new Type[expr.Count - 2];
                        List<object> parameters = new List<object>();
                        int last = expr.Count - 1;
                        for (int i = 2; i <= last; i++)
                        {
                            object a = eval(expr.ElementAt(i), env);
                            object param = a;
                            parameters.Add(param);
                            Type paramClass;

                            if (param == null)
                            {
                                paramClass = null;
                            }
                            else
                            {
                                paramClass = param.GetType();
                            }
                            parameterTypes[i - 2] = paramClass;
                        }

                        try
                        {
                            System.Reflection.ConstructorInfo c = cls.GetConstructor(parameterTypes);
                            return c.Invoke(parameters.ToArray());
                        }
                        catch (TargetException)
                        {
                            foreach (System.Reflection.ConstructorInfo c in cls.GetConstructors())
                            {
                                // find a constructor with the same number of parameters
                                if (c.GetParameters().Length == expr.Count - 2)
                                {
                                    try
                                    {
                                        return c.Invoke(parameters.ToArray());
                                    }
                                    catch (System.ArgumentException)
                                    {
                                        // try next constructor
                                        continue;
                                    }
                                }
                            }
                        }
                        throw new System.ArgumentException(expr.ToString());
                    }
                    else if (code == sym_doseq.code) // (doseq (VAR SEQ) EXPR ...)
                    {
                        Environment env2 = new Environment(env);
                        int varCode = Core.symbolValue(Core.listValue(expr[1])[0]).code;
                        System.Collections.IEnumerable seq = (System.Collections.IEnumerable)eval(Core.listValue(expr[1])[1], env);
                        int len1 = expr.Count;
                        foreach (object x in seq)
                        {
                            env2.def(varCode, (object)x);
                            for (int i = 2; i < len1; i++)
                            {
                                eval(expr.ElementAt(i), env2);
                            }
                        }
                        return null;
                    }
                    else if (code == sym_let.code) // (let (VAR VALUE ...) BODY ...)
                    {
                        Environment env2 = new Environment(env);
                        IList bindings = (IList)Core.listValue(expr.ElementAt(1));
                        for (int i = 0; i < bindings.Count; i += 2)
                        {
                            env2.def(Core.symbolValue(bindings[i]).code, eval(bindings[i + 1], env2));
                        }
                        object ret = null;
                        for (int i = 2; i < expr.Count; i++)
                        {
                            ret = eval(expr.ElementAt(i), env2);
                        }
                        return ret;
                    }
                    else if (code == sym_import.code) // (import & import-symbols-or-lists-or-prefixes)
                    {
                        Type lastImport = null;
                        for (int i = 1; i < expr.Count; i++)
                        {
                            object x = expr.ElementAt(i);
                            if (x is Symbol)
                            { // e.g. System.Math
                                string s = x.ToString();
                                lastImport = tryGetClass(s);
                                if (lastImport == null) // not a class (e.g. System)
                                {
                                    if (!imports.Contains(s))
                                    {
                                        imports.Add(s);
                                    }
                                }
                            }
                            else if (x is System.Collections.IList)
                            { // e.g. (System ArrayList Math)
                                IList xl = (IList)listValue(x);
                                string prefix = xl[0].ToString();
                                for (int j = 1; j < xl.Count; j++)
                                {
                                    string s = xl[j].ToString();
                                    lastImport = getClass(prefix + "." + s);
                                    getClassCache[s] = lastImport;
                                }
                            }
                            else
                            {
                                throw new Exception("Syntax error");
                            }
                        }
                        return lastImport;
                    }

                    else if (code == sym_reify.code) // (reify INTERFACE (METHOD (ARGS ...) BODY ...) ...)
                    {
                        // Note that the first parameter must be supplied to
                        // correspond to the target object ('this' in C# parlance). Thus
                        // methods for interfaces will take one more argument than do the
                        // interface declarations.
                        //
                        // Example:
                        // > (str (reify Object (ToString [this] (str "reified object: " this))))
                        // "reified object: System.Object"

                        Type interfaceType = getClass(expr[1].ToString());
                        var methods = new Dictionary<string, UserFn>();
                        for (int i = 2; i < expr.Count; i++)
                        {
                            List<object> methodDef = (List<object>)expr[i];
                            methods.Add(methodDef[0].ToString(), new UserFn(new List<object>(methodDef.Skip(1)), env));
                        }
                        return ReifyProxy.Create(Activator.CreateInstance(interfaceType), methods);
                    }
                    else if (code == sym_recur.code) // (recur ARG ...)
                    {
                        List<object> args1 = new List<object>();
                        for (int i = 1; i < expr.Count; i++)
                        {
                            args1.Add(eval(expr.ElementAt(i), env));
                        }
                        throw new Recur(args1);
                    }
                    else if (code == sym_loop.code) // (loop (VAR VALUE ...) BODY ...)
                    {
                        // separate formal and actual parameters
                        IList bindings = (IList)Core.listValue(expr.ElementAt(1));
                        List<object> formalParams = new List<object>();
                        List<object> actualParams = new List<object>();
                        Environment env2 = new Environment(env);
                        for (int i = 0; i < bindings.Count; i += 2)
                        {
                            formalParams.Add(bindings[i]);
                            actualParams.Add(eval(bindings[i + 1], env2));
                        }

                        while (true)
                        {
                            // fill the environment
                            for (int i = 0; i < formalParams.Count; i++)
                            {
                                env2.def(Core.symbolValue(formalParams[i]).code, actualParams[i]);
                            }
                            // evaluate body
                            object ret = null;
                            for (int i = 2; i < expr.Count; i++)
                            {
                                try
                                {
                                    ret = eval(expr.ElementAt(i), env2);
                                }
                                catch (Recur e)
                                {
                                    actualParams = e.args;
                                    goto loopStartContinue; // recur this loop (effectively goto)
                                }
                            }
                            return ret;
                        loopStartContinue:;
                        }
                    }
                    else if (code == sym_quasiquote.code) // (quasiquote S-EXPRESSION)
                    {
                        return quasiquote(expr.ElementAt(1), env);
                    }
                    else if (code == sym_try.code) // (try EXPR ... (catch CLASS VAR EXPR ...) ... (finally EXPR ...))
                    {
                        int i = 1, len1 = expr.Count;
                        object ret = null;
                        try
                        {
                            for (; i < len1; i++)
                            {
                                object e = expr.ElementAt(i);
                                if (e is List<object>)
                                {
                                    object prefix = ((List<object>)e)[0];
                                    if (prefix.Equals(sym_catch) || prefix.Equals(sym_finally))
                                    {
                                        break;
                                    }
                                }
                                ret = eval(e, env);
                            }
                        }
                        catch (Exception t)
                        {
                            for (; i < len1; i++)
                            {
                                object e = expr.ElementAt(i);
                                if (e is List<object>)
                                {
                                    List<object> exprs = (List<object>)e;
                                    if (exprs[0].Equals(sym_catch) && getClass(exprs[1].ToString()).IsInstanceOfType(t))
                                    {
                                        Environment env2 = new Environment(env);
                                        env2.def(Symbol.toCode(exprs[2].ToString()), t);
                                        for (int j = 3; j < exprs.Count; j++)
                                        {
                                            ret = eval(exprs[j], env2);
                                        }
                                        return ret;
                                    }
                                }
                            }
                            throw t;
                        }
                        finally
                        {
                            for (; i < len1; i++)
                            {
                                object e = expr.ElementAt(i);
                                if (e is List<object> && ((List<object>)e)[0].Equals(sym_finally))
                                {
                                    List<object> exprs = (List<object>)e;
                                    for (int j = 1; j < exprs.Count; j++)
                                    {
                                        eval(exprs[j], env);
                                    }
                                }
                            }
                        }
                        return ret;
                    }
                    else if (code == sym_defmacro.code) // (defmacro add (a & more) `(+ ~a ~@more)) ; define macro
                    {
                        macros[expr.ElementAt(1).ToString()] = new UserFn(new List<object>(expr.Skip(2)), globalEnv);
                        return null;
                    }
                }
                // evaluate arguments
                object func = eval(expr.ElementAt(0), env);
                List<object> args = new List<object>();
                int len = expr.Count;
                for (int i = 1; i < len; i++)
                {
                    args.Add(eval(expr.ElementAt(i), env));
                }
                return apply(func, args);
            }
            else
            {
                // return n.clone();
                return n;
            }
        }

        private static object quasiquote(object arg, Environment env)
        {
            if (arg is List<object> && ((List<object>)arg).Count > 0)
            {
                List<object> arg2 = (List<object>)arg;
                object head = arg2[0];
                if (head is Symbol && ((Symbol)head).code == sym_unquote.code)
                {
                    return macroexpandEval(arg2[1], env);
                }
                List<object> ret = new List<object>();
                foreach (object a in arg2)
                {
                    if (a is List<object>)
                    {
                        List<object> a2 = (List<object>)a;
                        if (a2.Count > 0)
                        {
                            object head2 = a2[0];
                            if (head2 is Symbol && ((Symbol)head2).code == sym_unquote_splicing.code)
                            {
                                ret.AddRange((IEnumerable<object>)Core.listValue(macroexpandEval(a2[1], env)));
                                continue;
                            }
                        }
                    }
                    ret.Add(quasiquote(a, env));
                }
                return ret;
            }
            else
            {
                return arg;
            }
        }

        // cached. throws if not found.
        internal static Type getClass(string className)
        {
            Type s;
            if (getClassCache.TryGetValue(className, out s))
            {
                return s;
            }
            else
            {

                Type value = Type.GetType(className);
                if (value != null)
                {
                    getClassCache[className] = value;
                    return value;
                }
                else
                {
                    foreach (string prefix in imports)
                    {
                        Type value1 = Type.GetType(prefix + "." + className);
                        if (value1 != null)
                        {
                            getClassCache[className] = value1;
                            return value1;
                        }
                        else
                        {
                            // try next import prefix
                            continue;
                        }
                    }
                    //return null;
                    throw new TypeLoadException(className);
                }
            }
        }

        // returns null if not found
        internal static Type tryGetClass(string className)
        {
            try
            {
                return getClass(className);
            }
            catch (TypeLoadException)
            {
                return null;
            }
        }

        public static object load_string(string s)
        {
            s = "(" + s + "\n)";
            object result = null;
            foreach (object o in Core.listValue(parse(new StringReader(s))))
            {
                result = macroexpandEval(o, globalEnv);
            }
            return result;
        }

        internal static void prompt()
        {
            Console.Write("> ");
        }

        // read-eval-print loop
        public static void repl()
        {
            MyReader reader = new MyReader(Console.In);
            while (true)
            {
                try
                {
                    prompt();
                    object expr = parse(reader);
                    Console.WriteLine(toReadableString(macroexpandEval(expr, globalEnv)));
                }
                catch (EndOfStreamException) // Ctrl+Z in Windows
                {
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //Console.Write(e.StackTrace);
                }
            }
        }

        // extracts characters from URL or filename
        public static string slurp(string urlOrFileName, string charsetName)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(urlOrFileName);
            }
        }

        // Opposite of slurp. Writes str to filename.
        public static int spit(string fileName, string str)
        {
            StreamWriter bw;
            try
            {
                bw = new StreamWriter(fileName);
                bw.Write(str);
                bw.Close();
                return str.Length;
            }
            catch (IOException)
            {
                return -1;
            }
        }

        // for embedding
        public static object ElementAt(string s)
        {
            return globalEnv.get(Symbol.toCode(s));
        }

        // for embedding
        public static object set(string s, object o)
        {
            return globalEnv.def(Symbol.toCode(s), o);
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                IList<string> argsList1 = new List<string>();
                set("*command-line-args*", argsList1);
                printLogo();
                repl();
                Console.WriteLine();
                return;
            }
            else if (args.Length >= 1)
            {
                if (args[0].Equals("-h"))
                {
                    Console.WriteLine("Usage: Closhure.exe [OPTION] [ARGS...]");
                    Console.WriteLine();
                    Console.WriteLine("Options:");
                    Console.WriteLine("    FILE  run a script.");
                    Console.WriteLine("    -h    print this screen.");
                    Console.WriteLine("    -r    run a REPL.");
                    Console.WriteLine("    -v    print version.");
                    Console.WriteLine("Operation:");
                    Console.WriteLine("    Binds *command-line-args* to a list of strings containing command line args that appear after FILE.");
                    return;
                }
                else if (args[0].Equals("-r"))
                {
                    IList<string> argsList1 = new List<string>();
                    for (int i = 1; i < args.Length; i++)
                    {
                        argsList1.Add(args[i]);
                    }
                    set("*command-line-args*", argsList1);
                    printLogo();
                    repl();
                    Console.WriteLine();
                    return;
                }
                else if (args[0].Equals("-v"))
                {
                    Console.WriteLine(Core.VERSION);
                    return;
                }
            }

            // execute the file
            IList<string> argsList = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                argsList.Add(args[i]);
            }
            set("*command-line-args*", argsList);
            try
            {
                load_file(args[0]);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                //Console.Write(e.StackTrace);
            }
        }

        public static object load_file(string file)
        {
            return load_string(Core.slurp(file, "UTF-8"));
        }

        public static object macroexpandEval(object @object, Environment env)
        {
            return eval(macroexpand(@object), env);
        }

        public static bool eof(TextReader r)
        {
            int c = r.Peek();
            if (c < 0) return true;
            return false;
        }

        public static char peek(TextReader r)
        {
            int p = r.Peek();
            if (p < 0) throw new EndOfStreamException("EOF while reading");
            return (char)p;
        }

        public static string readToken(TextReader r)
        {
            const string ws = " \t\r\n,";
            const string delim = "()[] \t\r\n,;\"\\";
            const string prefix = "()[]'`\"#\\";

            while (true)
            {
                char p;
                char c;

                // skip whitespaces
                while (true)
                {
                    p = peek(r);
                    if (ws.IndexOf(Convert.ToChar(p)) < 0)
                    {
                        break;
                    }
                    r.Read();
                }

                p = peek(r);

                if (prefix.IndexOf(Convert.ToChar(p)) >= 0)
                { // prefix
                    StringBuilder acc = new StringBuilder(); // accumulator
                    if (p == '#')
                    {
                        r.Read();
                        acc.Append((char)p);
                        p = peek(r);
                    }
                    if (p == '"')
                    { // string
                        r.Read();
                        acc.Append((char)p);
                        while (true)
                        {
                            c = (char)r.Read();
                            if (c == '"')
                            {
                                break;
                            }
                            if (c == '\\')
                            { // escape
                                char next = (char)r.Read();
                                if (next == 'r')
                                {
                                    next = '\r';
                                }
                                else if (next == 'n')
                                {
                                    next = '\n';
                                }
                                else if (next == 't')
                                {
                                    next = '\t';
                                }
                                else if (next == 'b')
                                {
                                    next = '\b';
                                }
                                else if (next == 'f')
                                {
                                    next = '\f';
                                }
                                else if (next == '\\')
                                {
                                    next = '\\';
                                }
                                else
                                {
                                    acc.Append('\\'); // Unsupported escape character: do not escape
                                }
                                acc.Append(next);
                            }
                            else
                            {
                                acc.Append((char)c);
                            }
                        }
                        return acc.ToString();
                    }
                    else if (p == '\\') // character
                    {
                        StringBuilder acc1 = new StringBuilder(); // accumulator
                                                                 // read until delim
                        while (!eof(r))
                        {
                            if (delim.IndexOf(peek(r)) >= 0 && acc1.Length > 1)
                            {
                                break;
                            }
                            c = Convert.ToChar(r.Read());
                            acc1.Append(c);
                        }
                        return acc1.ToString();
                    }
                    else
                    {
                        c = (char)r.Read();
                        return "" + (char)c;
                    }
                }
                else if (p == '~')
                { // unquote
                    StringBuilder acc = new StringBuilder(); // accumulator
                    c = Convert.ToChar(r.Read());
                    acc.Append((char)c);
                    if (peek(r) == '@')
                    { // unquote-splicing
                        c = Convert.ToChar(r.Read());
                        acc.Append((char)c);
                    }
                    return acc.ToString();
                }
                else if (p == ';')
                { // end-of-line comment
                    while (!eof(r) && peek(r) != '\n')
                    {
                        r.Read();
                    }
                    continue;
                }
                else
                { // other
                    StringBuilder acc = new StringBuilder(); // accumulator
                    // read until delim
                    while (!eof(r))
                    {
                        if (peek(r) == '|') // a| symbol with special characters| -> a symbol with special characters
                        {
                            r.Read(); // opening |
                            while ((c = Convert.ToChar(r.Read())) != '|')
                            {
                                acc.Append(c);
                            }
                        }

                        if (delim.IndexOf(peek(r)) >= 0)
                        {
                            break;
                        }
                        c = Convert.ToChar(r.Read());
                        acc.Append(c);
                    }
                    return acc.ToString();
                }
            }
        }

        public static object parse(TextReader r)
        {
            return parse(r, readToken(r));
        }

        public static object parse(TextReader r, string tok)
        {
            if (tok == null) return null;
            if (tok[0] == '"')
            { // double-quoted string
                return tok.Substring(1);
            }
            else if (tok[0] == '#' && tok[1] == '"')
            { // regex
                return new Regex(tok.Substring(2));
            }
            else if (tok.Equals("'"))
            { // quote
                List<object> ret = new List<object>();
                ret.Add(sym_quote);
                ret.Add(parse(r));
                return ret;
            }
            else if (tok.Equals("`"))
            { // quasiquote
                List<object> ret = new List<object>();
                ret.Add(sym_quasiquote);
                ret.Add(parse(r));
                return ret;
            }
            else if (tok.Equals("~"))
            { // unquote
                List<object> ret = new List<object>();
                ret.Add(sym_unquote);
                ret.Add(parse(r));
                return ret;
            }
            else if (tok.Equals("~@"))
            { // unquote-splicing
                List<object> ret = new List<object>();
                ret.Add(sym_unquote_splicing);
                ret.Add(parse(r));
                return ret;
            }
            else if (tok.Equals("("))
            { // list
                return parseList(r);
            }
            else if (tok.Equals("["))
            {
                return parseVector(r);
            }
            else if (tok[0] == '\\')
            { // char
              // Characters - preceded by a backslash: \c. \newline, \space, \tab, \formfeed, \backspace, and \return yield the corresponding characters. Unicode characters are represented with \\uNNNN as in C#. Octals are represented with \\oNNN.
                if (tok.Length == 2)
                {
                    return tok[1];
                }
                else
                {
                    switch (tok)
                    {
                        case "\\newline":
                            return '\n';
                        case "\\space":
                            return ' ';
                        case "\\tab":
                            return '\t';
                        case "\\formfeed":
                            return '\f';
                        case "\\backspace":
                            return '\b';
                        case "\\return":
                            return '\r';
                        default:
                            if (tok[1] == 'u' && tok.Length == 6)
                            { // Unicode: \\uNNNN
                                int codePoint = int.Parse(tok.Substring(2));
                                return Convert.ToChar(codePoint);
                            }
                            else if (tok[1] == 'o' && tok.Length == 5)
                            { // Octal: \\oNNN
                                int codePoint = Convert.ToInt32(tok.Substring(2), 8);
                                return Convert.ToChar(codePoint);
                            }
                            throw new Exception("Unsupported character: " + tok);
                    }
                }
            }
            else if (char.IsDigit(tok[0]) || tok[0] == '-' && tok.Length >= 2 && char.IsDigit(tok[1]))
            { // number
                if (tok.IndexOf('.') != -1 || tok.IndexOf('e') != -1)
                { // double
                    return double.Parse(tok);
                }
                else if (tok.EndsWith("L", StringComparison.Ordinal) || tok.EndsWith("l", StringComparison.Ordinal))
                { // long
                    return long.Parse(tok.Substring(0, tok.Length - 1));
                }
                else
                {
                    try
                    {
                        return int.Parse(tok);
                    }
                    catch (System.FormatException)
                    {
                        return long.Parse(tok); // parse big number to long
                    }
                }
            }
            else
            { // symbol
              // other literals
                switch (tok)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    case "nil":
                        return null;
                }
                if (tok.StartsWith(":", StringComparison.Ordinal))
                { // keyword
                    return new Keyword(tok);
                }
                // normal symbol
                return new Symbol(tok);
            }
        }

        private static List<object> parseList(TextReader r)
        {
            List<object> ret = new List<object>();
            while (!eof(r))
            {
                string tok = readToken(r);
                if (tok.Equals(")"))
                { // end of list
                    break;
                }
                else
                {
                    object o = parse(r, tok);
                    ret.Add(o);
                }
            }
            return ret;
        }

        private static ArrayList parseVector(TextReader r)
        {
            ArrayList ret = new ArrayList();
            while (!eof(r))
            {
                string tok = readToken(r);
                if (tok.Equals("]"))
                { // end of list
                    break;
                }
                else
                {
                    object o = parse(r, tok);
                    ret.Add(o);
                }
            }
            return ret;
        }
    }
}