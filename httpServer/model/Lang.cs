using csharpHelp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.model {
	public class Lang {
		public static Lang ins = new Lang();

		//public string start = "Start";
		//[Bridge("btnRestart.Content")]	public string restart = "Restart";
		//[Bridge("btnLog.Content")]		public string log = "Log";
		//[Bridge("lblLog.Content")]		public string log1 = "Log：";
		//[Bridge("btnStop.Content")]		public string stop = "Stop";
		//[Bridge("btnDel.Content")]		public string delete = "Delete";
		//[Bridge("btnNew.Content")]		public string create = "Create";

		//[Bridge("txtDesc.Content")]		public string desc1 = "Desc：";
		//[Bridge("cbxIp.Content")]		public string ip1 = "IP";
		//[Bridge("txtPort.Tip")] [Bridge("txtPort.ToolTip")] public string port = "Port";
		//[Bridge("txtPath.Content")]		public string path1 = "Path：";
		//[Bridge("txtUrlParam.Content")]	public string urlParam1 = "url param：";
		//[Bridge("lblCopyUrl.Content")]	public string copy = "Copy";
		//[Bridge("lblRewrite.Content")]	public string redirect1 = "Rewrite";
		//[Bridge("txtRewrite.Content")]  public string redirectDesc = "start with '-'：redirect only if file not exist&#x0a;'/originUrl'='http://rewriteDomain:80/rewriteUrl';&#x0a;-'/originUrl'='http://rewriteDomain:80/rewriteUrl';";

		public string start = "启动";
		[Bridge("btnRestart.Content")]	public string restart = "重启";
		[Bridge("btnLog.Content")]		public string log = "日志";
		[Bridge("lblLog.Content")]		public string log1 = "日志：";
		[Bridge("btnStop.Content")]		public string stop = "停止";
		[Bridge("btnDel.Content")]		public string delete = "删除";
		[Bridge("btnNew.Content")]		public string create = "新建";

		[Bridge("txtDesc.Content")]		public string desc1 = "描述：";
		[Bridge("cbxIp.Content")]		public string ip1 = "IP";
		[Bridge("txtPort.Tip")] [Bridge("txtPort.ToolTip")] public string port = "端口号";
		[Bridge("txtPath.Content")]		public string path1 = "路径：";
		[Bridge("txtUrlParam.Content")]	public string urlParam1 = "url参数：";
		[Bridge("lblCopyUrl.Content")]	public string copy = "复制";
		[Bridge("lblRewrite.Content")]	public string redirect1 = "重定向";
		[Bridge("txtRewrite.Content")]  public string redirectDesc = "'-'号开头：只有文件不存在时才会重定向&#x0a;'/originUrl'='http://rewriteDomain:80/rewriteUrl';&#x0a;-'/originUrl'='http://rewriteDomain:80/rewriteUrl';";
	}
}
