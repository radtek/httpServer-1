using com.superscene.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpServer.services {
	class CmdServ {
		public static Cmd cfgWaitSave = new Cmd();	//未保存
		public static Cmd cfgSaved = new Cmd();		//保存完成
	}
}
