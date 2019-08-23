using LICC.API;
using System.Reflection;

namespace LICC.Console
{
    public class ConsoleImplementation : Frontend
    {
        public void BeginRead()
        {
            while (true)
            {
                System.Console.Write("> ");
                OnLineInput(System.Console.ReadLine());
            }
        }

        public override void Write(string str) => System.Console.Write(str);

        public static void StartDefault()
        {
            var frontend = new ConsoleImplementation();
            var console = new CommandConsole(frontend);
            console.Commands.RegisterCommandsIn(Assembly.GetCallingAssembly());

            frontend.BeginRead();
        }
    }
}
