using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class Kernel32 {
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
		
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaxSizeHi, uint dwMaxSizeLow, string lpName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr OpenFileMapping(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr MapViewOfFile(IntPtr hFileMapping, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool UnmapViewOfFile(IntPtr pvBaseAddress);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32", EntryPoint = "GetLastError")]
		public static extern int GetLastError();

		[DllImport("kernel32.dll")]
		public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

		// 取得当前线程编号（线程钩子需要用到）
		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();

		//打开进程获取句柄
		[DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
		public static extern IntPtr OpenProcess(int desiredAccess, bool heritHandle, int pocessID);
		[DllImport("kernel32.dll")]
		public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int size, out int numBytesRead);
		[DllImport("kernel32.dll")]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int size, out int numBytesWrite);

		public const int MEM_COMMIT = 0x1000;       //已物理分配

		public struct MEMORY_BASIC_INFORMATION {
			public int BaseAddress;
			public int AllocationBase;
			public int AllocationProtect;
			public int RegionSize;      //区域大小  
			public int State;           //状态  
			public int Protect;         //保护类型  
			public int lType;
		}

		public struct SYSTEM_INFO {
			public ushort processorArchitecture;
			public ushort reserved;
			public uint pageSize;
			public IntPtr minimumApplicationAddress;
			public IntPtr maximumApplicationAddress;
			public IntPtr activeProcessorMask;
			public uint numberOfProcessors;
			public uint processorType;
			public uint allocationGranularity;
			public ushort processorLevel;
			public ushort processorRevision;
		}

		public const int ERROR_ALREADY_EXISTS = 183;

		public const int FILE_MAP_COPY = 0x0001;
		public const int FILE_MAP_WRITE = 0x0002;
		public const int FILE_MAP_READ = 0x0004;
		public const int FILE_MAP_ALL_ACCESS = 0x0002 | 0x0004;

		public const int PAGE_READONLY = 0x02;
		public const int PAGE_READWRITE = 0x04;
		public const int PAGE_WRITECOPY = 0x08;
		public const int PAGE_EXECUTE = 0x10;
		public const int PAGE_EXECUTE_READ = 0x20;
		public const int PAGE_EXECUTE_READWRITE = 0x40;

		public const int SEC_COMMIT = 0x8000000;
		public const int SEC_IMAGE = 0x1000000;
		public const int SEC_NOCACHE = 0x10000000;
		public const int SEC_RESERVE = 0x4000000;

		public const uint GENERIC_READ = 0x80000000;
		public const uint GENERIC_WRITE = 0x40000000;
		public const uint GENERIC_EXECUTE = 0x20000000;
		public const uint GENERIC_ALL = 0x10000000;
		public const uint FILE_SHARE_READ = 0x00000001;
		public const uint FILE_SHARE_WRITE = 0x00000002;
		public const uint FILE_SHARE_DELETE = 0x00000004;
		
		public const uint CREATE_NEW = 1;
		public const uint CREATE_ALWAYS = 2;
		public const uint OPEN_EXISTING = 3;
		public const uint OPEN_ALWAYS = 4;
		public const uint TRUNCATE_EXISTING = 5;

		public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
		public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

		public const int INVALID_HANDLE_VALUE = -1;
	}
}
