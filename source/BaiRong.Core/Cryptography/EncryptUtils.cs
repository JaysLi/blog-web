using System;
using System.Text;
using System.Security.Cryptography;

namespace BaiRong.Core.Cryptography
{
	/// <summary>
	/// �ԳƼ����㷨�ࡣ
	/// </summary>
	public class EncryptUtils
	{
		private HashAlgorithm mhash;
  
		public static EncryptUtils Instance = new EncryptUtils();

		/// <summary>
		/// �ԳƼ�����Ĺ��캯�������ɶԼ����ַ������н���
		/// </summary>
		private EncryptUtils()
		{
		}

		/// <summary>
		/// ���ܷ���
		/// </summary>
		/// <param name="Value">�����ܵĴ�</param>
		/// <returns>�������ܵĴ�</returns>
		public string EncryptString(string Value)
		{
			byte[] bytValue;
			byte[] bytHash;

			mhash = SetHash(true);

			// Convert the original string to array of Bytes
			bytValue = Encoding.UTF8.GetBytes(Value);

			// Compute the Hash, returns an array of Bytes
			bytHash = mhash.ComputeHash(bytValue);

			mhash.Clear();

			// Return a base 64 encoded string of the Hash value
			return Convert.ToBase64String(bytHash);
		}

		private HashAlgorithm SetHash(bool isMD5)
		{
			if(!isMD5)
				return new SHA1CryptoServiceProvider();
			else
				return new MD5CryptoServiceProvider();
		}

        public static string Md5(string str)
        {
            var cl = str;
            var pwd = string.Empty;
            var md5 = MD5.Create();
            var s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            for (var i = 0; i < s.Length; i++)
            {
                // ���õ����ַ���ʹ��ʮ���������͸�ʽ����ʽ����ַ���Сд����ĸ�����ʹ�ô�д��X�����ʽ����ַ��Ǵ�д�ַ� 

                pwd = pwd + s[i].ToString("x");

            }
            return pwd;
        }
	}
}
