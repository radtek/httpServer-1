using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.dll {

	public class HttpServerGo {

		[StructLayout(LayoutKind.Sequential)]
		public struct HttpGlobalInfo {
			public IntPtr HttpsCrt;
			public IntPtr HttpsKey;
			public IntPtr MapContextType;
			public int Timeout;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct HttpParamMd {
			public IntPtr Ip;
			public IntPtr Port;
			public bool IsHttps;
			public IntPtr Path;
			public IntPtr VirtualPath;
			public IntPtr Proxy;
		}

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Init")]
		public static extern void Init();

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Aaa")]
		public static extern long Aaa(ref HttpParamMd param);

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetHttpGlobalInfo")]
		public static extern void SetHttpGlobalInfo(ref HttpGlobalInfo info);

		//[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetKey")]
		//public static extern void SetKey(char[] HttpsCrt, char[] HttpsKey);

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RemoveServer")]
		public static extern void RemoveServer(long id);

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UpdateServer")]
		public static extern void UpdateServer(long id, ref HttpParamMd param);

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "RestartServer")]
		public static extern void RestartServer(long id);

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CreateServer")]
		public static extern long CreateServer(ref HttpParamMd param);

		[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "StopServer")]
		public static extern void StopServer(long id);

		//[DllImport("httpServerGo.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Clear")]
		//public static extern void Clear();

	}
}
