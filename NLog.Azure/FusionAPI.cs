using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NLog.Azure
{
    // http://stackoverflow.com/questions/19456547/how-to-programmatically-determine-if-net-assembly-is-installed-in-gac
    // https://msdn.microsoft.com/en-us/library/ms404523.aspx

    internal static class FusionAPI
    {
        // http://blogs.msdn.com/b/bclteam/archive/2007/02/13/long-paths-in-net-part-1-of-3-kim-hamilton.aspx
        private const int AssemblyPathMax = 260*2;

        [DllImport("fusion.dll")]
        private static extern int CreateAssemblyCache(out FusionAPI.IAssemblyCache ppAsmCache, int reserved);

        public static string GetAssemblyPath(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var assemblyInfo = new AssemblyInfo {cchBuffer = AssemblyPathMax };

            assemblyInfo.currentAssemblyPath = new string((char) 0, assemblyInfo.cchBuffer);

            IAssemblyCache assemblyCache;
            if (CreateAssemblyCache(out assemblyCache, 0) >= 0 && assemblyCache.QueryAssemblyInfo(0, name, ref assemblyInfo) < 0)
                return (string) null;

            return assemblyInfo.currentAssemblyPath;
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        [ComImport]
        private interface IAssemblyCache
        {
            int Dummy1();

            [PreserveSig()]
            int QueryAssemblyInfo(int flags, [MarshalAs(UnmanagedType.LPWStr)] string assemblyName, ref AssemblyInfo assemblyInfo);

            int Dummy2();
            int Dummy3();
            int Dummy4();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AssemblyInfo
        {
            public int cbAssemblyInfo;
            public int assemblyFlags;
            public long assemblySizeInKB;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string currentAssemblyPath;

            public int cchBuffer;
        }
    }
}
