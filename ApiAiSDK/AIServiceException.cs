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
using ApiAiSDK.Model;

namespace ApiAiSDK
{
	public class AIServiceException : Exception
	{
        public AIResponse Response { get; set; }

		public AIServiceException()
		{
		}

		public AIServiceException(string message) : base(message)
		{
		}

	    public AIServiceException(string message, Exception innerException) : base(message, innerException)
	    {
	    }

	    public AIServiceException(Exception e) : base(e.Message, e)
		{
		}

        public AIServiceException(AIResponse response)
        {
            Response = response;
        }

        public override string Message
        {
            get
            {
                if (Response != null && Response.IsError)
                {
                    if (!string.IsNullOrEmpty(Response.Status.ErrorDetails))
                    {
                        return Response.Status.ErrorDetails;
                    }
                }

                return base.Message;
            }
        }
	}
}

