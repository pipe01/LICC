using LICC.API;

namespace LICC.Console
{
    public class ConsoleImplementation : Frontend
    {
        public override void Write(string str) => System.Console.Write(str);
    }
}
