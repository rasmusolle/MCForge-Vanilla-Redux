using System;
using System.IO;
namespace Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists("MCForge_.dll"))
            {
                openServer(args);
            }
        }
        static void openServer(string[] args)
        {
            MCForge_.Gui.Program.Main(args);
        }
    }
}
