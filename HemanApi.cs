using System.Runtime.InteropServices;

using CC = System.Runtime.InteropServices.CallingConvention;

namespace HemanWinUI
{
    internal class HemanApi
    {
        public int GetNumberOfThreads()
        {
            return NativeMethods.heman_get_num_threads();
        }

        private static class NativeMethods
        {
            const string DLL = "heman.dll";

            [DllImport(DLL, CallingConvention = CC.Cdecl)]
            internal static extern int heman_get_num_threads();
        }
    }
}
