using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace Closhure
{
    internal class ReifyProxy : RealProxy
    {
        private Dictionary<string, UserFn> methods;
        private ReifyProxy(Type myType) : base(myType)
        {
        }

        public static object Create(Type myType, Dictionary<string, UserFn> methods)
        {
            var rp = new ReifyProxy(myType);
            rp.methods = methods;
            return rp.GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = (IMethodCallMessage)msg;
            var method = (MethodInfo)methodCall.MethodBase;

            try
            {
                UserFn fn = methods[method.Name];
                var result = fn.Invoke(methodCall.InArgs);
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
