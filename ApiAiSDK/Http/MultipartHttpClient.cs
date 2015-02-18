//
// API.AI .NET SDK - client-side libraries for API.AI
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************

using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;

namespace ApiAiSDK.Http
{
	internal class MultipartHttpClient
	{
		private const string delimiter = "--";
		private string boundary = "SwA" + DateTime.UtcNow.Ticks.ToString("x") + "SwA";
		private HttpWebRequest request;
		private BinaryWriter os;

		public MultipartHttpClient (HttpWebRequest request)
		{
			this.request = request;
		}

		public void connect()
		{
			request.ContentType = "multipart/form-data; boundary=" + boundary;
			request.SendChunked = true;
			request.KeepAlive = true;

			os = new BinaryWriter (request.GetRequestStream (), Encoding.UTF8);
		}

		public void addStringPart(string paramName, string data)
		{
			WriteString (delimiter + boundary + "\r\n");
			WriteString ("Content-Type: application/json\r\n");
			WriteString ("Content-Disposition: form-data; name=\"" + paramName + "\"\r\n");
			WriteString ("\r\n" + data + "\r\n");
		}

		public void addFilePart(string paramName, string fileName, Stream data)
		{
			WriteString (delimiter + boundary + "\r\n");
			WriteString ("Content-Disposition: form-data; name=\"" + paramName + "\"; filename=\"" + fileName + "\"\r\n");
			WriteString ("Content-Type: audio/wav\r\n");

			WriteString ("\r\n");

			int bufferSize = 4096;
			byte[] buffer = new byte[bufferSize];
			
			int bytesActuallyRead;

			bytesActuallyRead = data.Read (buffer, 0, bufferSize);
			while (bytesActuallyRead > 0) {
				os.Write (buffer, 0, bytesActuallyRead);
				bytesActuallyRead = data.Read (buffer, 0, bufferSize);
			}

			WriteString ("\r\n");
		}

		public void finish()
		{
			WriteString (delimiter + boundary + delimiter + "\r\n");
			os.Close ();
		}

		private void WriteString(string str)
		{
			os.Write(Encoding.UTF8.GetBytes(str));
		}

		public string getResponse()
		{
			try {
				var httpResponse = request.GetResponse () as HttpWebResponse;
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
					var result = streamReader.ReadToEnd ();
					return result;
				}
			} catch (WebException we) {
				using (var stream = we.Response.GetResponseStream()) {
					using (var reader = new StreamReader(stream)) {
						return reader.ReadToEnd ();
					}
				}
			}
		}

	}
}

