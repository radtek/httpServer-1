package router

import (
	"fmt"
	"crypto/tls"
    // "io"
    "os"
	"net/http"
	// "bufio"
    Path "path"
    "io/ioutil"
	// "path/filepath"
	"encoding/json"
	"time"
	// "strconv"
	"regexp"
	// "os/exec"
	// "bytes"
	"mime"
	"strings"
	// "sort"
	// "syscall"
	// "encoding/base64"
	
	"github.com/gorilla/mux"
	"github.com/gorilla/websocket"
	
	. "model"
)

type ProxyMd struct{
	Key string;
	Val string;
	IsCheckFile bool;
	UseWebscoket bool;
}

type HttpServerRouter struct{
	md HttpParamMd;
	// Path string;
	// VirtualPath string;
	Router *mux.Router;

	arrProxy []ProxyMd;
	upgrader *websocket.Upgrader;
}

func (c *HttpServerRouter) Create(md HttpParamMd) {
	// c.WebPath = "E:\\00work\\web\\sdk\\AR.js\\";
	c.arrProxy = []ProxyMd{};
	c.UpdateParam(md);

	c.upgrader = &websocket.Upgrader{
        CheckOrigin: func(r *http.Request) bool {
            return true
        },
    };

	// c.Path = path;
	// c.VirtualPath = virtualPath;
	c.Router = mux.NewRouter();
	
	// c.router.HandleFunc("/{project}/server/{any:.*}", optionsHandler).Methods("OPTIONS");
	
	//static
	c.Router.HandleFunc("/{virtualPath}/{path:.*}", c.getVirtualStaticFileHandler);
	c.Router.HandleFunc("/{path:.*}", c.getStaticFileHandler);

	c.Router.Use(setOriginMiddleware);
}

func (c *HttpServerRouter) updateProxy(text string) {
	text = strings.Replace(text, "\r\n", "\n", -1);
	arrRst := []ProxyMd{};

	reg := regexp.MustCompile("^([w-]*)(?:['\"‘“]?)(.*?)(?:['\"‘“]?)(?:[\\s]*=[\\s]*)(?:['\"‘“]?)([^'\"‘“]*)(?:['\"‘“]?)");

	arr := strings.Split(text, "\n");
	for i:=0; i < len(arr); i++ {
		mat := reg.FindStringSubmatch(arr[i]);
		if(mat == nil || len(mat) != 4) {
			continue;
		}

		param := mat[1];

		md := ProxyMd{};
		md.IsCheckFile = (strings.Index(param, "-") >= 0);
		md.UseWebscoket = (strings.Index(param, "w") >= 0);
		md.Key = mat[2];
		md.Val = mat[3];
		arrRst = append(arrRst, md);
		// fmt.Println(mat[1] + ",  " + md.Key + ",   " + md.Val);
	}

	// fmt.Println("----------------");

	c.arrProxy = arrRst;
}

func (c *HttpServerRouter) UpdateParam(md HttpParamMd) {
	if(c.md.Proxy != md.Proxy) {
		c.updateProxy(md.Proxy);
	}

	c.md = md;
}

// static file
func (c *HttpServerRouter) getStaticFileHandler(w http.ResponseWriter, r *http.Request){
	params := mux.Vars(r);
	path := params["path"];
	c.logicGetStaticFileHandler(w, r, path, "");
}

// static file
func (c *HttpServerRouter) getVirtualStaticFileHandler(w http.ResponseWriter, r *http.Request){
	params := mux.Vars(r);
	vpath := params["virtualPath"];
	path := params["path"];

	if(c.md.VirtualPath == "") {
		path = vpath + "/" + path;
		vpath = "";
	}
	c.logicGetStaticFileHandler(w, r, path, vpath);
}

func (c *HttpServerRouter) proxyWebsocket(w http.ResponseWriter, r *http.Request, newUrl string) {
	conn, err := c.upgrader.Upgrade(w, r, nil);
	if (err != nil) {
		w.WriteHeader(500);
        return;
	}
	defer conn.Close();

	// fmt.Println(newUrl);
	// newHeader := make(http.Header);
	// for k,v := range r.Header {
	// 	key := strings.ToLower(k);
	// 	if(key == "sec-websocket-version") {
	// 		continue;
	// 	}
	// 	newHeader[key] = v;
	// }
	dialer := websocket.Dialer{ TLSClientConfig: &tls.Config{RootCAs: nil, InsecureSkipVerify: true }};
	connClient, _, err := dialer.Dial(newUrl, nil);
	// connClient, _, err := websocket.DefaultDialer.Dial(newUrl, nil);

	if err != nil {
		return;
	}
	defer connClient.Close();

	done := make(chan struct{});

	// client
	go func() {
		defer close(done)
		for {
			messageType, message, err := connClient.ReadMessage();
			if err != nil {
				return;
			}

			if err = conn.WriteMessage(messageType, message); err != nil {
				return;
			}
		}
	}();
	
	// server
	for {
		messageType, data, err := conn.ReadMessage();
        if (err != nil) {
            break;
        }
		if err = connClient.WriteMessage(messageType, data); err != nil {
			break;
		}
    }
}

func (c *HttpServerRouter) proxyHeandler(w http.ResponseWriter, r *http.Request, path string) bool {
	url := r.URL.Path;
	regHost := regexp.MustCompile("(?:.*?://)(.*?)(?:/)");
	regOrigin := regexp.MustCompile("(.*?://.*?)(?:/)");

	for i:=0; i < len(c.arrProxy); i++ {
		md := c.arrProxy[i];
		idx := strings.Index(url, md.Key);
		if(idx != 0) {
			continue;
		}

		if(md.IsCheckFile) {
			fullPath := c.getFilePath(path);
			f, err := os.Stat(fullPath);
			if err == nil && !f.IsDir() {
				return false;
			}
		}

		newUrl := md.Val + url[idx+len(md.Key):];

		// websocket
		if(md.UseWebscoket) {
			c.proxyWebsocket(w, r, newUrl);
			return true;
		}

		req, err := http.NewRequest(r.Method, newUrl, r.Body);
		if(err != nil) {
			w.WriteHeader(404);
			return true;
		}
		
		// host
		mat := regHost.FindStringSubmatch(newUrl);
		var host []string = nil;
		if(len(mat)>=1) {
			host = []string{ mat[1] };
		}
		
		// origin
		mat = regOrigin.FindStringSubmatch(newUrl);
		var origin []string = nil;
		if(len(mat)>=1) {
			origin = []string{ mat[1] };
		}

		header := make(http.Header);
		for key,val := range r.Header {
			keyTmp := strings.ToLower(key);
			if (keyTmp == "host") {
				if(host!=nil) {
					header[key] = host;
				}
			} else if (keyTmp == "host") {
				if(origin != nil) {
					header[key] = origin;
				}
			} else {
				header[key] = val;
			}
		}
		req.Header = header;

		// no verify https
		tr := &http.Transport{
			TLSClientConfig: &tls.Config{ InsecureSkipVerify: true },
		}

		comMd := GetComMd();
		client := &http.Client{
			Transport: tr,
			Timeout: time.Duration(comMd.HttpGlobalInfo.Timeout) * time.Millisecond,
		};
		res, err2 := client.Do(req);

		if err2 != nil {
			w.WriteHeader(404);
			return true;
		}

		header2 := w.Header();
		for key,val := range res.Header {
			header2[key] = val;
		}

		w.WriteHeader(res.StatusCode);

		body, _ := ioutil.ReadAll(res.Body);
		w.Write(body);
	
		return true;
	}

	return false;
}

func (c *HttpServerRouter) getFilePath(path string) string {
	strPath := path;

	if strPath == "" || strPath[len(strPath)-1]=='/' {
		strPath = strPath + "index.html";
	}

	if(strPath == "" || strPath[0] != '/') {
		strPath = "/" + strPath;
	}

	fullPath := c.md.Path + strPath;
	return fullPath;
}

// static file
func (c *HttpServerRouter) logicGetStaticFileHandler(w http.ResponseWriter, r *http.Request, path string, virtualPath string) {
	ok := c.proxyHeandler(w, r, path);
	if(ok) {
		return;
	}

	if(virtualPath != c.md.VirtualPath) {
		w.WriteHeader(404);
		return;
	}

	// fullPath := c.md.Path + strPath;
	fullPath := c.getFilePath(path);

	// find path {exePath}/web/
	f, err := os.Stat(fullPath);
	if err != nil || f.IsDir() {
		w.WriteHeader(404);
		writeGzipStr(w, r, "404 page not found");
		return;
	}

	sufix := Path.Ext(fullPath);
	comMd := GetComMd();
	conType,ok := comMd.HttpGlobalInfo.MapContextType[sufix];
	if(!ok) {
		conType = mime.TypeByExtension(sufix);
	}
	if(conType != ""){
        w.Header().Set("Content-Type", conType)
    }

	bytes,_ := ioutil.ReadFile(fullPath);
	writeGzipByte(w, r, bytes);
}

// return error info
func (c *HttpServerRouter) comErr(w http.ResponseWriter, r *http.Request, errInfo string) {
	rst := NewComRst();
	rst.ErrInfo = errInfo;
	jsonData, _ := json.Marshal(rst);
	writeGzipByte(w, r, jsonData);
}

// 
func (c *HttpServerRouter) comSuccess(w http.ResponseWriter, r *http.Request, data interface{}) {
	rst := NewComRst();
	rst.Data = data;
	jsonData, _ := json.Marshal(rst);
	writeGzipByte(w, r, jsonData);
}

func (c *HttpServerRouter) test(){
	fmt.Println("abc");
}
