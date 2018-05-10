using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	/// <summary>
	/// 加密/解密
	/// </summary>
	public class EncryptDES {
		public string desKey = "A8FE0S2M";   //DES加密密钥
		public string desVi = "OK3A5G2F";    //DES加密向量

		public EncryptDES() {
			//randKey();
		}

		public void randKey() {
			Random rd = new Random();
			int keyCount = 8;

			desKey = "";
			desVi = "";
			List<char> lstChar = new List<char>();
			for(int i = 0; i < 26; ++i) {
				lstChar.Add((char)('a' + i));
				lstChar.Add((char)('A' + i));
			}
			for(int i = 0; i < 10; ++i) {
				lstChar.Add((char)('0' + i));
			}

			for(int i = 0; i < keyCount; ++i) {
				int idx = rd.Next(lstChar.Count - 1);
				desKey += lstChar[idx];
				int idx2 = rd.Next(lstChar.Count - 1);
				desVi += lstChar[idx2];
			}
		}

		/// <summary>
		/// DES加密
		/// </summary>
		/// <param name="sInputString"></param>
		/// <param name="sKey"></param>
		/// <param name="sIV"></param>
		/// <returns></returns>
		public string encode(string sInputString) {
			try {
				byte[] data = Encoding.UTF8.GetBytes(sInputString);

				DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

				DES.Key = ASCIIEncoding.ASCII.GetBytes(desKey);

				DES.IV = ASCIIEncoding.ASCII.GetBytes(desVi);

				ICryptoTransform desencrypt = DES.CreateEncryptor();

				byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);

				String strResult = BitConverter.ToString(result);
				string[] sOut = strResult.Split("-".ToCharArray());
				strResult = "";
				for(int i = 0; i < sOut.Length; i++) {
					strResult += sOut[i];
				}

				//return BitConverter.ToString(result);
				return strResult;
			} catch {
			
			}

			return "";
		}

		/// <summary>
		/// 解密
		/// </summary>
		/// <param name="sInputString"></param>
		/// <param name="sKey"></param>
		/// <param name="sIV"></param>
		/// <returns></returns>
		public string decode(string sInputString) {
			try {
				if(sInputString.Length % 2 != 0) {
					sInputString = "0" + sInputString;
				}

				string[] sInput = new String[sInputString.Length / 2];
				for(int i = 0; i < sInputString.Length; i += 2) {
					sInput[i / 2]  = sInputString[i].ToString();
					sInput[i / 2] += sInputString[i + 1].ToString();
				}
				//string[] sInput = sInputString.Split("-".ToCharArray());

				byte[] data = new byte[sInput.Length];

				for(int i = 0; i < sInput.Length; i++) {
					data[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
				}

				DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

				DES.Key = ASCIIEncoding.ASCII.GetBytes(desKey);

				DES.IV = ASCIIEncoding.ASCII.GetBytes(desVi);

				ICryptoTransform desencrypt = DES.CreateDecryptor();

				byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);

				return Encoding.UTF8.GetString(result);
			} catch { }

			return "";
		}

	}

}
