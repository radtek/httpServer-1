package model

type CHttpGlobalInfo struct {
	HttpsCrt []byte;
	HttpsKey []byte;
	MapContextType map[string] string;
	Timeout int;
}

type ComMd struct {
	HttpGlobalInfo CHttpGlobalInfo;
}

var GetComMd = (func() (func() (*ComMd)) {
	var ins *ComMd;

	return func() (*ComMd) {
		if(ins == nil) {
			ins = new(ComMd);
			
			ins.HttpGlobalInfo.HttpsCrt = []byte{};
			ins.HttpGlobalInfo.HttpsKey = []byte{};
			ins.HttpGlobalInfo.MapContextType = make(map[string] string);

		}
		return ins;
	}
})();
