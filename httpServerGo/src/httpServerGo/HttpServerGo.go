package httpServerGo

import (
	// "fmt"
	"crypto/tls"
	"flag"
    // "net"
    "net/http"
	// "strconv"
	// "time"
	// "log"
	"os"
	// "io/ioutil"
	// "strings"
	// "database/sql"
	"path/filepath"
	// "sync"

	// . "dao"
	// . "server"
	. "router"
	. "model"
	tag "tag"
	// util "util"
)

type HttpServerMd struct {
	Id int64;
	Server *http.Server;
	Router *HttpServerRouter;
	Param *HttpParamMd;
}

var ins *HttpServerGo;

type HttpServerGo struct{
	// comMd	*ComModel;
	httpDataGlobalId int64;
	mapHttpData map[int64] *HttpServerMd;
	rootDir string;
	router	MainRouter;
}

func GetServer() (*HttpServerGo){
	if(ins == nil){
		ins = new(HttpServerGo);
		ins.httpDataGlobalId = 0;
		ins.mapHttpData = make(map[int64] *HttpServerMd);
	}
	return ins;
}

func getStringNotEmpty(val *string, defVal string) string {
	if(val == nil || *val == ""){
		//default path
		return defVal;
	} else{
		return *val;
	}
}

func (c *HttpServerGo) Run() {
	//show version
	// c.comMd = GetComModel();
	// c.comMd.Version = "v1.0.0";
	// version := "v1.0.0";
	// fmt.Println("HttpServerGo " + version);

	//get arguments
	ip := "localhost";
	port := "8060";
	key := "";
	isHttps := false;
	flag.StringVar(&ip, "ip", "localhost", "ip");
	flag.StringVar(&port, "port", "8060", "port");
	flag.StringVar(&key, "key", "00000000", "key");
	flag.BoolVar(&isHttps, "https", false, "https");
	flag.Parse();

	//set path
	exeDir, _ := filepath.Abs(filepath.Dir(os.Args[0]));
	rootDir := exeDir + "/";
	if(tag.GetTag().Debug) {
		workDir, _ := os.Getwd();
		rootDir = workDir + "/";
	}
	c.rootDir = rootDir;
}

func (c *HttpServerGo) SetHttpGlobalInfo(info *CHttpGlobalInfo){
	md := GetComMd();
	md.HttpGlobalInfo = *info;
}

func (c *HttpServerGo) RemoveServer(id int64){
	md,ok := c.mapHttpData[id];
	if(!ok) {
		return;
	}

	c._stopServer(md);

	delete(c.mapHttpData, id);
}

func (c *HttpServerGo) UpdateServer(id int64, param *HttpParamMd) {
	val,ok := c.mapHttpData[id];
	if(!ok) {
		return;
	}
	val.Param = param;
	val.Router.UpdateParam(*param);
}

func (c *HttpServerGo) RestartServer(id int64) {
	md,ok := c.mapHttpData[id];
	if(!ok) {
		return;
	}
	
	c._stopServer(md);

	c._createServer(md);
}

func (c *HttpServerGo) CreateServer(param *HttpParamMd) int64 {
	c.httpDataGlobalId++;

	md := HttpServerMd{};
	md.Id = c.httpDataGlobalId;
	// tmp := *param;
	md.Param = param;
	// md.Param = *param;
	
	md.Router = &HttpServerRouter{};
	md.Router.Create(*md.Param);

	c.mapHttpData[md.Id] = &md;
	return md.Id;
}

func (c *HttpServerGo) _createServer(md *HttpServerMd) {
	// ssl
	comMd := GetComMd();
	cert, _ := tls.X509KeyPair(comMd.HttpGlobalInfo.HttpsCrt, comMd.HttpGlobalInfo.HttpsKey);

	tlsConfig := &tls.Config{
		Certificates: []tls.Certificate { cert },
	};
	
	md.Server = &http.Server{
		// Other options
		Addr: md.Param.Ip + ":" + md.Param.Port,
		Handler: md.Router.Router,
		TLSConfig: tlsConfig,
	};

	// run
	md.Server.Addr = md.Param.Ip + ":" + md.Param.Port;
	if(!md.Param.IsHttps) {
		go (func() {
			md.Server.ListenAndServe();
		})();
	} else {
		go (func() {
			md.Server.ListenAndServeTLS("", "");
		})();
	}
}

func (c *HttpServerGo) _stopServer(md *HttpServerMd) {
	if(md.Server != nil) {
		md.Server.Close();
		md.Server = nil;
	}
}

func (c *HttpServerGo) StopServer(id int64) {
	md,ok := c.mapHttpData[id];
	if(!ok) {
		return;
	}
	c._stopServer(md);
}

func (c *HttpServerGo) Clear() {
	for _,md := range c.mapHttpData {
		c._stopServer(md);
	}

	c.mapHttpData = make(map[int64] *HttpServerMd);
}
