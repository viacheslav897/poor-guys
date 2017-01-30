using System.Security.Principal;
using Autofac;
using Microsoft.EntityFrameworkCore;
using PoorGuys.DataAccess.Context;

namespace PoorGuys.DataAccess
{
    public class DataAcessAutofacModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DataBaseContext>().AsSelf().As<DbContext>().InstancePerRequest();
            containerBuilder.Register<IContextWrapper>(x => new ContextWrapper(x.Resolve<DataBaseContext>())).InstancePerRequest();
        }
    }
}