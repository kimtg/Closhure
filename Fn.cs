using System;
using System.Collections.Generic;

namespace Closhure
{

    internal class Fn : IComparer<object>
    {
        public virtual object Invoke(IList<object> args)
        {
            return null;
        }

        public void run()
        {
            try
            {
                Invoke(new List<object>());
            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

        public virtual int Compare(object arg0, object arg1)
        {
            List<object> a = new List<object>();
            a.Add(arg0);
            a.Add(arg1);
            try
            {
                return (int)Invoke(a);
            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                return 0;
            }
        }

        public object call()
        {
            try
            {
                return Invoke(new List<object>());
            }
            catch (Exception e)
            {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                return null;
            }
        }

        public void InvokeDelegate(params object[] args)
        {
            Invoke(new List<object>(args));
        }

        // to invoke a delegate with no arguments, to avoid ambiguity with the params version
        public void InvokeDelegate0()
        {
            Invoke(new List<object>());
        }

    }
}
