using System;
using System.Runtime.InteropServices;

namespace Randomizer.App
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
    }
}
