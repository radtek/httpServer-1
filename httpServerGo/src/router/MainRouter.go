package router

import (
    "fmt"
    // "io"
    "os"
	"net/http"
	// "bufio"
    Path "path"
    "io/ioutil"
	// "path/filepath"
	"encoding/json"
	// "time"
	// "strconv"
	// "regexp"
	// "os/exec"
	// "bytes"
	"mime"
	// "strings"
	// "sort"
	// "syscall"
	// "encoding/base64"
	// "strings"
	
	"github.com/gorilla/mux"
	
	. "model"
)

type MainRouter struct{
	WebPath string;
	Router *mux.Router;
}

func (c *MainRouter) Init(){
	// c.WebPath = "E:\\00work\\web\\sdk\\AR.js\\";
	c.WebPath = "E:\\00work\\web\\draft\\hhh\\";
	c.Router = mux.NewRouter();
	
	c.Router.HandleFunc("/{project}/server/{any:.*}", optionsHandler).Methods("OPTIONS");
	
	//static
	c.Router.HandleFunc("/{path:.*}", c.getStaticFileHandler);

	c.Router.Use(setOriginMiddleware);

	http.Handle("/", c.Router);
}

// static file
func (c *MainRouter) getStaticFileHandler(w http.ResponseWriter, r *http.Request){
	strPath := r.URL.Path;
	if strPath != "" && strPath[len(strPath)-1]=='/' {
		strPath = strPath + "index.html";
	}

	if(strPath == "" || strPath[0] != '/') {
		strPath = "/" + strPath;
	}

	fullPath := c.WebPath + strPath;

	// find path {exePath}/web/
	f, err := os.Stat(fullPath);
	if err != nil || f.IsDir() {
		w.WriteHeader(404);
		writeGzipStr(w, r, "404 page not found");
		return;
	}

	sufix := Path.Ext(fullPath);
	conType := mime.TypeByExtension(sufix);
	if conType != "" {
        w.Header().Set("Content-Type", conType)
    }

	bytes,_ := ioutil.ReadFile(fullPath);
	writeGzipByte(w, r, bytes);
}

// return error info
func (c *MainRouter) comErr(w http.ResponseWriter, r *http.Request, errInfo string) {
	rst := NewComRst();
	rst.ErrInfo = errInfo;
	jsonData, _ := json.Marshal(rst);
	writeGzipByte(w, r, jsonData);
}

// 
func (c *MainRouter) comSuccess(w http.ResponseWriter, r *http.Request, data interface{}) {
	rst := NewComRst();
	rst.Data = data;
	jsonData, _ := json.Marshal(rst);
	writeGzipByte(w, r, jsonData);
}

func (c *MainRouter) test(){
	fmt.Println("abc");
}
