using System;
using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.NRPT;

namespace ProtonVPN.RestoreInternet;

class Program
{
    static void Main()
    {
        Console.WriteLine("Deleting WFP filters...");
        uint result = RemoveWfpObjects(0);
        if (result == 0)
        {
            Console.WriteLine("OK");
        }
        else
        {
            Console.WriteLine("Error: " + result);
        }

        Console.WriteLine("Deleting NRPT rule...");
        StaticNrptInvoker.DeleteRule();

        Console.WriteLine("Hit enter to close this window");
        Console.ReadLine();
    }

    [DllImport(
        "ProtonVPN.InstallActions.dll",
        EntryPoint = "RemoveWfpObjects",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern uint RemoveWfpObjects(long handle);
}