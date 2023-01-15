using Autofac;

namespace Protfi.Common
{
    public abstract class ServerModule : Module
    {
        protected ServerModule() { }
       
        protected abstract string ModuleName { get; }
    }
}