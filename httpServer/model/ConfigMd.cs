using csharpHelp;
using httpServer.control;
using httpServer.view.page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace httpServer.model {
	[XmlRoot("httpServer")]
	public class ConfigMd {
		//public XmlSerialize xmlConfig = new XmlSerialize();
		//public ClassLink mdLink = new ClassLink();

		[XmlAttr("win.x")]		[Bridge("Left")]	public int x = 100;
		[XmlAttr("win.y")]		[Bridge("Top")]		public int y = 100;
		[XmlAttr("win.width")]	[Bridge("Width")]	public int width = 700;
		[XmlAttr("win.height")] [Bridge("Height")]	public int height = 300;
		[XmlAttr("win.showLog")] [Bridge("isShowLog")]	public bool showLog = false;

		[XmlValue("config.colServerWidth")]	public int colServerWidth = 160;
		[XmlValue("config.selectItem")]		public int selectItem = 0;
		[XmlValue("config.timeout")]		public int timeout = 3000;

		[XmlMap("config.contentType.attr", XmlKeyType.Attr, "name", XmlValueType.Attr, "value")]	public Dictionary<string, string> mapContextType = new Dictionary<string, string>();

		[XmlListChild("serverBox.server")] public List<HttpServerMd> lstHttpServer = new List<HttpServerMd>();
	}

	
}
