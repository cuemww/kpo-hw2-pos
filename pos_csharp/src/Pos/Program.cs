using Microsoft.Extensions.DependencyInjection;
using Pos;
using Pos.Application;
using Pos.UI;

using var provider = Composition.Build("data");

if (args.Length > 0 && args[0] == "--demo")
{
    Demo.Run();
    return;
}

new ConsoleApp(provider.GetRequiredService<PosFacade>()).Run();
