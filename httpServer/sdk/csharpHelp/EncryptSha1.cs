using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace csharpHelp.util {
	/// <summary>
	/// Sha1
	/// </summary>
	public class EncryptSha1 {
		public static byte[] encode(byte[] data) {
			byte[] rst = new byte[0];
			try {
				SHA1 sha1 = new SHA1CryptoServiceProvider();
				//byte[] bytes_in = encode.GetBytes(content);
				rst = sha1.ComputeHash(data);
				sha1.Dispose();
				//string result = BitConverter.ToString(bytes_out);
				//result = result.Replace("-", "");
				//return result;
			} catch(Exception) {

			}

			return rst;
		}

		public static string hashToString(byte[] hash) {
			string rst = "";
			for(int i = 0; i < hash.Length; ++i) {
				rst += ((hash[i] >> 4)).ToString("x");
				rst += ((hash[i] & 0x0f)).ToString("x");
			}
			return rst;
		}
		
	}
}
