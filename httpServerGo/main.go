package main

/*
#include "main.h"
*/
import "C"

import (
	"fmt"
	// "crypto/tls"
	// "net/http"
	"strings"
	"sync"
	
	. "model"
	// . "router"
	. "httpServerGo"
)

var app *HttpServerGo;

//export Init
func Init() {
	app = GetServer();
}

// // export Aaa
// func Aaa(param *C.HttpParamMd) C.int64_t {
// // func Aaa(ip *C.char) C.int64_t {
// 	fmt.Println(C.GoString(param.Ip));
// 	// fmt.Println(C.GoString(ip));
// 	return C.int64_t(286);
// }

func createHttpGlobalInfo(_info *C.HttpGlobalInfo) *CHttpGlobalInfo {
	var info CHttpGlobalInfo;
	info.HttpsCrt = []byte(C.GoString(_info.HttpsCrt));
	info.HttpsKey = []byte(C.GoString(_info.HttpsKey));

	str := C.GoString(_info.MapContextType);

	mapType := make(map[string] string);
	arr := strings.Split(str, "!!");
	for i := 0; i < len(arr); i++ {
		arrOne := strings.Split(arr[i], "!");
		if(len(arrOne) != 2) {
			continue;
		}

		key := strings.ToLower(arrOne[0]);
		val := arrOne[1];
		mapType[key] = val;
	}

	info.MapContextType = mapType;
	info.Timeout = int(_info.Timeout);
	return &info;
}

//export SetHttpGlobalInfo
func SetHttpGlobalInfo(_info *C.HttpGlobalInfo){
	info := createHttpGlobalInfo(_info);

	app.SetHttpGlobalInfo(info);
}

//export RemoveServer
func RemoveServer(id C.int64_t){
	app.RemoveServer(int64(id));
}

func createHttpParamMd(_param *C.HttpParamMd) *HttpParamMd {
	var param HttpParamMd;
	param.Ip = C.GoString(_param.Ip);
	param.Port = C.GoString(_param.Port);
	param.IsHttps = (_param.IsHttps != C.false);
	param.Path = C.GoString(_param.Path);
	param.VirtualPath = C.GoString(_param.VirtualPath);
	param.Proxy = C.GoString(_param.Proxy);

	return &param;
}

//export UpdateServer
func UpdateServer(id C.int64_t, _param *C.HttpParamMd) {
	// param = *_param;
	param := createHttpParamMd(_param);

	app.UpdateServer(int64(id), param);
}

//export RestartServer
func RestartServer(id C.int64_t) {
	app.RestartServer(int64(id));
}

//export StopServer
func StopServer(id C.int64_t) {
	app.StopServer(int64(id));
}

//export CreateServer
func CreateServer(_param *C.HttpParamMd) C.int64_t {
	param := createHttpParamMd(_param);
	id := app.CreateServer(param);
	return C.int64_t(id);
}

//export Clear
func Clear() {
	app.Clear();
}

func testSetKey() {
	endl := "\r\n";
	strCrt := "" +
	"-----BEGIN CERTIFICATE-----" + endl +
	"MIIDXzCCAkegAwIBAgIJALbFlP61tDbbMA0GCSqGSIb3DQEBCwUAMEUxCzAJBgNV" + endl +
	"BAYTAkFVMRMwEQYDVQQIDApTb21lLVN0YXRlMSEwHwYDVQQKDBhJbnRlcm5ldCBX" + endl +
	"aWRnaXRzIFB0eSBMdGQwIBcNMjAxMjE3MDczMzU0WhgPMjEyMDExMjMwNzMzNTRa" + endl +
	"MEUxCzAJBgNVBAYTAkFVMRMwEQYDVQQIDApTb21lLVN0YXRlMSEwHwYDVQQKDBhJ" + endl +
	"bnRlcm5ldCBXaWRnaXRzIFB0eSBMdGQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAw" + endl +
	"ggEKAoIBAQCp92YAXia7KpKT0FLkN3E9xhvQudkz8tn4F6+0k9pITbRS7YKFudDd" + endl +
	"/SbQafdwk71u0DTvJfiuUNUHXmPhf92mMoBF875ZCzf5Lsk1NM8qLXrU/RLD2oG2" + endl +
	"yjP92M0cldbC2hxw8Z/JSGrzqUPNRDfMw4M2sB+o1fsy3iI7xu6Y58uayjEm+J7d" + endl +
	"nzbo2oCAdN92gjv5g4z90ebnii1sph5mMo8mFFex54Ag6Ogtk2zNQirVdld9GMt2" + endl +
	"RmLx8JyxhqTtvparxR84bzIaOJDL7tU9D34e123wZ+bla3NacvUg8HRqE4hoUfFO" + endl +
	"zGzbPIFRcJbgnT05IBBLZ2BqzklgO//LAgMBAAGjUDBOMB0GA1UdDgQWBBTiVr5I" + endl +
	"3Y4O/QMdieD4plaYOkxfijAfBgNVHSMEGDAWgBTiVr5I3Y4O/QMdieD4plaYOkxf" + endl +
	"ijAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQA8Wl44b++EHIxibC3N" + endl +
	"qfVurelt1fAmNS2cqVtTUDiu/cjsS7qm8BN7SDnRMfMT8dLGUDSBVfYkLiwrHhTt" + endl +
	"s871U7W5oo1QGUYZlqzg0YqWEKOflOVXvQz6sGrJdmJBmqwcadWQacHwCtlZXvG/" + endl +
	"vjQOE9f/MpR1RJPH4k7R0QJQTK7R5WiXVQ+4gNLxC5r+elZp/zwqujaaHAppV/t3" + endl +
	"7+74fqMs5kVTpRFv85sM9wJIECBIwM8eoMRmZzEIHbo/WW9XPviw2+Qz7Lm47hFc" + endl +
	"sPK2iRzDNNho0P67WlK9I5nv+l3RHX0ntEjxL4dTPJplTv3OWZ5rJ/xo8iRlkOIl" + endl +
	"YBHI" + endl +
	"-----END CERTIFICATE-----" + endl +
	"";

	srcKey := "" +
	"-----BEGIN RSA PRIVATE KEY-----" + endl +
	"MIIEowIBAAKCAQEAqfdmAF4muyqSk9BS5DdxPcYb0LnZM/LZ+BevtJPaSE20Uu2C" + endl +
	"hbnQ3f0m0Gn3cJO9btA07yX4rlDVB15j4X/dpjKARfO+WQs3+S7JNTTPKi161P0S" + endl +
	"w9qBtsoz/djNHJXWwtoccPGfyUhq86lDzUQ3zMODNrAfqNX7Mt4iO8bumOfLmsox" + endl +
	"Jvie3Z826NqAgHTfdoI7+YOM/dHm54otbKYeZjKPJhRXseeAIOjoLZNszUIq1XZX" + endl +
	"fRjLdkZi8fCcsYak7b6Wq8UfOG8yGjiQy+7VPQ9+Htdt8Gfm5WtzWnL1IPB0ahOI" + endl +
	"aFHxTsxs2zyBUXCW4J09OSAQS2dgas5JYDv/ywIDAQABAoIBADNZo1+JEnqJqi8u" + endl +
	"SVzZw0S+jbjJ7W1cea4Suer8oH4nu8syJrTwJsJqsUdWPIOunxTToqp99lvz3+iS" + endl +
	"A+slDnof19FKir+sPAT0taV1hwFfLDUdIIY7heULwhl3XsC8JF5KNz2IQpY1Ytqq" + endl +
	"0Ok7KwCaMRJcs7P2siX0JrOYro1TPIswSRFop0snvXxDpTMFwzkZ1f7Fui70kdNU" + endl +
	"8/9/wDy9oCVFD5ZGf3LljPHwLY1FqdTIzjfIj5o2qbvxm4kxZv6HlEa+MX+TEifF" + endl +
	"TZHgZiOKdNY2b9z4VDKl1l9SC19op2WHGsqZlKo/1I8stCeKIIMRDWA/OILaGb67" + endl +
	"LJ6HPUECgYEA0xOt0tm7R4V+NkYEDrc8kSwS0igF9O/PdiwpNtJs37BSdhXV+xs7" + endl +
	"PfItOyhB5d5uq1Zy1cFfebZKF6BgcEBkQpmRVsrLB5t4BVRVIiDomC2Wli4Ch7lU" + endl +
	"3QxZnz91zGwvydEyGCfnNXxZsAwVRdea98+biJD9eblf3HtaMWqWHm8CgYEAziPa" + endl +
	"1uJeo1G6faLtLl0yOdNRHmgldJ6gelpFC3BIw3ZKd/qO+khl2WKyKXV6fmF+R5wl" + endl +
	"6V3su/DusXpgZMcGP15vaUY1C/Q0fTVLUJRLihFuCmAwJzs1kghQFTPOyR0rUpkb" + endl +
	"W4rvwn7rJoguU4Vw+qRhYHHFzDnemXBfXQgc4mUCgYA3I4zk57vkkxrFUsT8kyFf" + endl +
	"SwQfohTsRzkKxb5+c7m9NXJVJp8fWZ3AMONf8MRGKDUAzTRyYnFuehAg2+RVbnzC" + endl +
	"aRtucMtY0WQpAD69C5u3JTGRSbOfgOqKVA+Vah57qEdTkTJk3QepETLcqktkU05I" + endl +
	"LhOTOUz9308Laa3F+vH8QQKBgFoP7je+FSzzsy/a9BcynpQHEEThqaOm/mQWdl4G" + endl +
	"A7RqRSTgMoGFCgKNDVvxuL/opnxw61tnLtv56r9dwSmmjsM25iQviVOcsSYXF72v" + endl +
	"3MUI0nP0DHXHd6NCwIJS7UCO3vOqcvpu3Eu0mdZu3xackXzgPq8dZhbRHcEaCIAj" + endl +
	"4ELFAoGBALTCccZeHK7N9JGSSX6mL/u4CyWBP9YbVE8zrhnW7r9+LqvrrmfRJTWU" + endl +
	"nMAPPBt1BkvnxGmNzVeU/1EbHHHAiBiSNJaaNIGOxZT3yG9VOZM6NmYSgLh2JbDX" + endl +
	"I++D5GHwymC3daNt8ON/6AxHgOGYKq+OhzeW5dOqzQiRrkeq4OM8" + endl +
	"-----END RSA PRIVATE KEY-----" + endl +
	"";
	
	var info CHttpGlobalInfo;
	info.HttpsCrt = []byte(strCrt);
	info.HttpsKey = []byte(srcKey);
	info.MapContextType = make(map[string] string);
	info.Timeout = 3000;

	// app.SetKey([]byte(strCrt), []byte(srcKey));
	app.SetHttpGlobalInfo(&info);
}

func main() {
	// srv := GetServer();

	// srv.Run();

	app = GetServer();
	// httpDataGlobalId = 0;
	// mapHttpData = make(map[int64] *HttpServerMd);

	// test
	testSetKey();

	param := HttpParamMd{}
	param.Ip = "localhost";
	param.Port = "8060";
	param.IsHttps = true;
	param.Path = "E:\\00work\\web\\project\\packing3d\\packing3dWeb\\dist\\web";
	param.VirtualPath = "aaa";
	app.CreateServer(&param);

	var wg sync.WaitGroup;
	wg.Add(1);
	wg.Wait();
}
