using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.superscene.util {
	public class Kernel32 {
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);
	}
}
