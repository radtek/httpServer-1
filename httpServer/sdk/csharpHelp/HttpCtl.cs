using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	public class HttpCtl {
		public string log = "";
		public string result = "";
		public bool isResponseOk = false;
		public HttpStatusCode code = HttpStatusCode.OK;

		private TimeSpan _timeout = new TimeSpan(0, 0, 0, 0, 0);
		public int timeout {
			set { _timeout = TimeSpan.FromMilliseconds(value); }
			get { return (int)_timeout.TotalMilliseconds; }
		}

		//http upload file as form
		public string httpUploadFile(string url, string filePath, string name = "file") {
			//log.Debug("[上传文件]<" + url + "><" + filePath + ">");
			string fileName = Path.GetFileName(filePath);
			string result = "";

			FileStream fs = null;

			try {
				fs = new FileStream(filePath, FileMode.Open);

				HttpContent fileStreamContent = new StreamContent(fs);
				using(var client = new HttpClient())
				using(var formData = new MultipartFormDataContent()) {
					if(_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}
					formData.Add(fileStreamContent, name, fileName);

					var response = client.PostAsync(url, formData).Result;
					isResponseOk = response.IsSuccessStatusCode;
					code = response.StatusCode;
					if(!isResponseOk) {
						log += "[error][httpUploadFile][code]" + code;
						result = "";
					} else {
						Stream sResult = response.Content.ReadAsStreamAsync().Result;
						result =  readStream(sResult);
					}
				}
			} catch(Exception ex) {
				//log.Error("上传失败,err=" + ex.ToString());
				log += "[error][httpUploadFile]" + ex.ToString();
			}

			fs.Close();

			return result;
		}

		public string httpPost(string url, string data = "", string name = "") {
			//log.Debug("[Post]<" + url + "><" + data + ">");
			string result = "";

			try {
				HttpContent stringContent = new StringContent(data);
				using(var client = new HttpClient()) {
					//using(var formData = new MultipartFormDataContent()) {
					//	formData.Add(stringContent, name, name);
					if(_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}

					var response = client.PostAsync(url, stringContent).Result;
					isResponseOk = response.IsSuccessStatusCode;
					code = response.StatusCode;
					if(!isResponseOk) {
						log += "[error][httpPost][code]" + code;
						result = "";
					} else {
						Stream sResult = response.Content.ReadAsStreamAsync().Result;
						result =  readStream(sResult);
					}
					//}
				}
			} catch(Exception ex) {
				log += "[error][httpPost]" + ex.ToString();
			}

			return result;
		}

		public string httpGet(string url, string data = "", string name = "") {
			//log.Debug("[Post]<" + url + "><" + data + ">");
			string result = "";

			try {
				//HttpContent stringContent = new StringContent(data);
				using(var client = new HttpClient()) {
					//using(var formData = new MultipartFormDataContent()) {
					//	formData.Add(stringContent, name, name);
					if(_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}

					var response = client.GetAsync(url).Result;
					isResponseOk = response.IsSuccessStatusCode;
					code = response.StatusCode;
					if(!isResponseOk) {
						log += "[error][httpPost][code]" + code;
						result = "";
					} else {
						Stream sResult = response.Content.ReadAsStreamAsync().Result;
						result = readStream(sResult);
					}
					//}
				}
			} catch(Exception ex) {
				log += "[error][httpPost]" + ex.ToString();
			}

			return result;
		}

		public byte[] httpGetByte(string url, out int len, string data = "", string name = "") {
			//log.Debug("[Post]<" + url + "><" + data + ">");
			len = 0;
			byte[] result = new byte[0];

			try {
				//HttpContent stringContent = new StringContent(data);
				using (var client = new HttpClient()) {
					//using(var formData = new MultipartFormDataContent()) {
					//	formData.Add(stringContent, name, name);
					if (_timeout.TotalMilliseconds != 0) {
						client.Timeout = _timeout;
					}

					var response = client.GetAsync(url).Result;
					isResponseOk = response.IsSuccessStatusCode;
					code = response.StatusCode;
					if (!isResponseOk) {
						log += "[error][httpPost][code]" + code;
						//result = "";
					} else {
						Stream sResult = response.Content.ReadAsStreamAsync().Result;
						//int len = 0;
						result = readStreamByte(sResult, out len);
					}
					//}
				}
			} catch (Exception ex) {
				log += "[error][httpPost]" + ex.ToString();
			}

			return result;
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

				//result += Encoding.UTF8.GetString(data, 0, readCount);
			} while (readCount >= bufSize);

			//result = Encoding.UTF8.GetString(data, 0, totaLen);

			len = totaLen;
			return data;
		}

		public string readStream(Stream stream) {
			string result = "";
			int bufSize = 1024;
			int totaLen = (int)stream.Length;
			byte[] data = new byte[totaLen + bufSize];
			int idx = 0;
			int readCount = 0;

			do {

				readCount = stream.Read(data, idx, bufSize);
				idx += readCount;

				//result += Encoding.UTF8.GetString(data, 0, readCount);
			} while(readCount >= bufSize);

			result = Encoding.UTF8.GetString(data, 0, totaLen);

			return result;
		}

	}
}
