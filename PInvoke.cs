using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ModOrganizerHelper
{
    [SuppressMessage("ReSharper", "StyleCop.SA1305", Justification = "DllImport")]
    public static class PInvoke
    {
        public enum SYMBOLIC_LINK_FLAG
        {
            File = 0,

            Directory = 1
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SYMBOLIC_LINK_FLAG dwFlags);
    }
}
