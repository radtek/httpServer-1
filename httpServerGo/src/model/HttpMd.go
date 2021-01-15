
package model

type ComRst struct {
	Success		bool				`json:"success"`;
	Data		interface{}			`json:"data"`;
	// InfoType	ErrType.ErrType		`json:"type"`;
	ErrInfo		string				`json:"errInfo"`;
}

type HttpParamMd struct {
	Ip			string;
	Port		string;
	IsHttps		bool;
	Path		string;
	VirtualPath	string;
	Proxy		string;
}

func NewComRst() ComRst {
	rst := ComRst{};
	rst.Success = false;
	rst.Data = nil;
	rst.ErrInfo = "";

	return rst;
}
