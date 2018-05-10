using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	class TimeTest {
		[DllImport("kernel32.dll ")]
		static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);

		[DllImport("kernel32")]
		static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

		long freq = 0;
		long tmStart = 0;
		long tmTag= 0;
		//long tmEnd = 0;


		public void start() {
			QueryPerformanceFrequency(ref freq);

			QueryPerformanceCounter(ref tmStart);
			tmTag = tmStart;
		}

		public long tag() {
			long tmEnd = 0;
			QueryPerformanceCounter(ref tmEnd);

			//毫秒
			long rst = (long)((tmEnd - tmTag) / (decimal)freq * 1000);
			tmTag = tmEnd;
			
			return rst;
		}

		public long end() {
			long tmEnd = 0;
			QueryPerformanceCounter(ref tmEnd);
			//毫秒
			long rst = (long) ((tmEnd - tmStart) / (decimal)freq * 1000);
			tmStart = tmEnd;
			tmTag = tmEnd;
			return rst;
		}

	}
}
