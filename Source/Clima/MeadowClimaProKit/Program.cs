using System.Threading;
using System.Threading.Tasks;
using Meadow;

namespace MeadowClimaProKit
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new MeadowApp();
            System.Console.WriteLine($"{nameof(Program)}: Initializing app.");
            var t = ((MeadowApp)app).Initialize();
            /*System.Console.WriteLine($"{nameof(Program)}: Application initialized. Awaiting.");
            while (!t.IsCompleted)
            {
                System.Console.WriteLine($"{nameof(Program)}: Sleeping 5s.");
                Thread.Sleep(5000);
            }*/
            System.Console.WriteLine($"{nameof(Program)}: Application init task finished. Sleeping.");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}