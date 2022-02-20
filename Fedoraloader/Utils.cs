using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Fedoraloader
{
    internal static class Utils
    {
        private static readonly Random _random = new();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static bool AddDefender(string pDirectory)
        {
            try
            {
                PowerShell.Create()
                    .AddScript(@"Add-MpPreference -ExclusionPath '" + pDirectory + "'")
                    .Invoke();

                Debug.WriteLine("Added folder to defender: " + pDirectory);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
