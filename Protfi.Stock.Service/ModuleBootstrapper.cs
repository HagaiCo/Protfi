using Autofac;
using Protfi.Common;
using Protfi.Stock.Service.BL;
using Protfi.Stock.Service.Interfaces;

namespace Protfi.Stock.Service
{
    public class StockModule : ServerModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterTypes(builder);
        }

        protected override string ModuleName => "Stock";

        private static void RegisterTypes(ContainerBuilder builder)
        {
            builder.RegisterType<StockService>().As<IStockService>();
        }
    }
}