using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace Closhure
{
    internal class ReifyProxy : RealProxy
    {
        private Dictionary<string, UserFn> methods;
        private object instance;
        private ReifyProxy(Type myType) : base(myType)
        {
        }

        public static object Create(object instance, Dictionary<string, UserFn> methods)
        {
            var rp = new ReifyProxy(typeof(MarshalByRefObject));
            rp.methods = methods;
            rp.instance = instance;
            return rp.GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            try
            {
                UserFn fn = methods[method.Name];
                List<object> args = new List<object> { instance }; // The first argument is 'this'.
                args.AddRange(methodCall.InArgs);
                var result = fn.Invoke(args);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                if (e is TargetInvocationException && e.InnerException != null)
                {
                    return new ReturnMessage(e.InnerException, msg as IMethodCallMessage);
                }

                return new ReturnMessage(e, msg as IMethodCallMessage);
            }
        }
    }
}
