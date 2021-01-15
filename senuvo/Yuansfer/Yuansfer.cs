using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace Yuansfer.Helper
{
    public static class Util
    {
        private static string callAPI(object data, bool productiveEnvironment)
        {
            var _URL = "https://mapi.yuansfer.yunkeguan.com";            //Testing domain
            if (productiveEnvironment)
            {
                _URL = "https://mapi.yuansfer.com";  //Production domain
            }
            var url = _URL + "/online/v2/secure-pay";
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;
            request.Method = "POST";
            string serializedData = new JavaScriptSerializer().Serialize(data);
            byte[] byteArray = Encoding.UTF8.GetBytes(serializedData);
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        private static string CreateMD5(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public static YuansferSignatureResponse Login(string vendor, double amount,  string orderNumber, bool productiveEnvironment)
        {
            var token = "af5b6b580da443aaaf434392570f8501";
            var merchantnro = "202546";
            var storenro = "301942";
            string callbackUrl = "https://senuvo.shop.directscalestage.com/www/Complete";
            string callbackUrlIPN = "https://senuvo.corpadmin.directscalestage.com/Command/ClientAPI/senuvoservices/callback";
            if (productiveEnvironment)
            {
                token = "2f41e5bf2d0ca09db9240261839a32fb";
                merchantnro = "201681";
                storenro = "301754";
                callbackUrl = "https://senuvo.shop.directscale.com/www/Complete";
                callbackUrlIPN = "https://senuvo.corpadmin.directscale.com/Command/ClientAPI/senuvoservices/callback";
            }
            YuansferSignature para = new YuansferSignature
            {
                amount = amount,
                callbackUrl = callbackUrl,// + "?yuansferId={yuansferId}&status={status}&amount={amount}&time={time}&reference={reference}",
                currency = "USD",
                ipnUrl = callbackUrlIPN,
                merchantNo = merchantnro,
                reference = orderNumber,
                storeNo = storenro,
                terminal = "ONLINE",
                timeout = "120",
                vendor = vendor,
            };
            var queryString = para.ToString();
            string verifySign = CreateMD5(queryString + "&" + CreateMD5(token));
            para.verifySign = verifySign;
            string result = callAPI(para, productiveEnvironment);
            YuansferSignatureResponse data = JsonConvert.DeserializeObject<YuansferSignatureResponse>(result);
            return data;
        }
    }
    public class YuansferSignature
    {
        public double amount { get; set; }
        public string callbackUrl { get; set; }
        public string currency { get; set; }
        public string ipnUrl { get; set; }
        public string merchantNo { get; set; }
        public string reference { get; set; }
        public string storeNo { get; set; }
        public string terminal { get; set; }
        public string timeout { get; set; }
        public string vendor { get; set; }
        public string verifySign { get; set; }

        public override string ToString()
        {
            return String.Format("amount={0}&callbackUrl={1}&currency={2}&ipnUrl={3}&merchantNo={4}&reference={5}&storeNo={6}&terminal={7}&timeout={8}&vendor={9}",
                this.amount, this.callbackUrl, this.currency, this.ipnUrl, this.merchantNo, this.reference, this.storeNo, this.terminal, this.timeout, this.vendor);
        }
    }

    public class YuansferSignatureResponse
    {
        public string ret_code { get; set; }
        public YuansferSignatureResponseResult result { get; set; }
    }

    public class YuansferSignatureResponseResult
    {
        public string cashierUrl { get; set; }
    }
}