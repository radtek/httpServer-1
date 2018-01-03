using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.control {
	class RequestCtl {
		private Dictionary<string, Action<string, HttpListenerContext>> mapUrl = new Dictionary<string, Action<string, HttpListenerContext>>();
		public RequestCtl() {
			mapUrl["savString"] = saveString;
		}

		public bool parse(string url, HttpListenerContext ctx) {
			bool isDeal = false;

			int idx = url.IndexOf("/", 1);

			Debug.WriteLine(url);

			return isDeal;
		}

		private void saveString(string url, HttpListenerContext ctx) {
			Dictionary<string, string> data = getParam(ctx);
		}

		private void saveBase64(string url, HttpListenerContext ctx) {

		}

		private Dictionary<string, string> getParam(HttpListenerContext ctx) {
			var parser = new MultipartFormDataParser(ctx.Request.InputStream);

			Dictionary<string, string> result = new Dictionary<string, string>();
			for(int i = 0; i < parser.Parameters.Count; ++i) {
				result[parser.Parameters[i].Name] = parser.Parameters[i].Data;
			}

			return result;
		}

	}
}
