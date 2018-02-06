using com.superscene.util;
using httpServer.assembly.page;
using httpServer.entity;
using httpServer.module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer.control.httpRevProxy {
	/// <summary>
	/// http反向代理客户端
	/// </summary>
	class HttpRevClient : IServerCtl {
		HttpRevModel md = null;

		Thread thCtl = null;
		TcpCtl tcpClient = null;
		List<byte[]> lstData = new List<byte[]>();
		HttpClient httpClient = null;

		public HttpRevClient() {
			httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
			httpClient.MaxResponseContentBufferSize = 10 * 1024 * 1024;
		}
		public HttpRevClient(ServerModule _md):this() {
			setModel(_md);
		}

		public void setModel(ServerModule _md) {
			md = (HttpRevModel)_md;
		}

		public void restartServer() {
			try {
				clear();

				thCtl = new Thread(cltProc);
				thCtl.Start();
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public void cltProc() {
			try {
				tcpClient = new TcpCtl(md.localCtlIp, md.localCtlPort);
				tcpClient.connect();
				tcpClient.listenData((data)=> {
					if(data.Length == 0) {
						return;
					}
					
					Task.Run(() =>{
						dealOne(data);
					});
				});
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}
		
		private void dealOne(byte[] data) {
			HttpPackModel httpModel = new HttpPackModel();
			httpModel.unPack(data);
			
			string strUrl = httpModel.url;
			strUrl = "http://" + md.localHttpIp + ":" + md.localHttpPort + strUrl;

			try {
				HttpResponseMessage response = httpClient.GetAsync(strUrl).Result;
				bool isResponseOk = response.IsSuccessStatusCode;
				HttpStatusCode code = response.StatusCode;
				int len = 0;
				byte[] result = new byte[0];
				if (!isResponseOk) {
				} else {
					Stream sResult = response.Content.ReadAsStreamAsync().Result;
					result = readStreamByte(sResult, out len);
				}

				HttpPackModel httpModelRst = new HttpPackModel();
				httpModelRst.httpIdx = httpModel.httpIdx;
				httpModelRst.url = httpModel.url;
				//httpModelRst.mapHead["type"] = response.typ;
				var arr = response.Headers.ToList();
				//Debug.WriteLine("bbb:" + arr.Count);
				for (int j = 0; j < arr.Count; ++j) {
					string key = arr[j].Key;
					string val = "";
					if (arr[j].Value.Count() > 0) {
						//val = ((List<string>)arr[i].Value)[0];
						val = arr[j].Value.FirstOrDefault();
					}
					httpModelRst.mapHead[key] = val;
				}
				httpModelRst.mapHead["Status"] = ((int)code).ToString();
				if (response.Content.Headers.ContentType != null) {
					httpModelRst.mapHead["Content-Type"] = response.Content.Headers.ContentType.ToString();
				} else {
					httpModelRst.mapHead["Content-Type"] = "";
				}

				httpModelRst.data = new byte[len];
				Array.Copy(result, 0, httpModelRst.data, 0, len);

				byte[] rst = httpModelRst.pack();
				rst = httpModelRst.pack();
				
				Task.Run(() => {
					tcpClient.sendAsync(rst);
				});
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		public byte[] readStreamByte(Stream stream, out int len) {
			//byte[] result = new byte[0];
			int bufSize = 1024;
			int totaLen = (int)stream.Length;
			byte[] data = new byte[totaLen + bufSize];
			int idx = 0;
			int readCount = 0;

			do {
				readCount = stream.Read(data, idx, bufSize);
				idx += readCount;
				
			} while (readCount >= bufSize);

			len = totaLen;
			return data;
		}

		public void clear() {
			try {
				tcpClient?.clear();
				tcpClient = null;

				thCtl?.Abort();
				thCtl = null;

				lstData = new List<byte[]>();
			} catch (Exception) {

			}
		}

	}
}
