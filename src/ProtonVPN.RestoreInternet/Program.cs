using System;
using System.Runtime.InteropServices;

namespace ProtonVPN.RestoreInternet
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Clearing wfp filters...");
            uint result = RemoveWfpObjects(0);
            if (result == 0)
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("Error: " + result);
            }

            Console.WriteLine("Hit enter to close this window");
            Console.ReadLine();
        }

        [DllImport(
            "ProtonVPN.InstallActions.dll",
            EntryPoint = "RemoveWfpObjects",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern uint RemoveWfpObjects(long handle);
    }
}