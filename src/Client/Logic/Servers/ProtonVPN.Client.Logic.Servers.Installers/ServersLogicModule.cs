namespace ProtonVPN.Client.Logic.Servers.Installers;

using Autofac;

public class ServersLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ServerManager>().AsImplementedInterfaces().SingleInstance();
    }
}