//
//  API.AI .NET SDK - client-side libraries for API.AI
//  =================================================
//
//  Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
//  https://www.api.ai
//
//  ***********************************************************************************************************************
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
//  the License. You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
//  an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
//  specific language governing permissions and limitations under the License.
//
//  ***********************************************************************************************************************
using System;
using System.Linq;
using System.Collections.Generic;
using ApiAiSDK.Model;

namespace ApiAiSDK
{
    public class RequestExtras
    {
        public List<AIContext> Contexts { get; set; }

        public List<Entity> Entities { get; set; }

        public bool HasContexts
        {
            get
            {
                if (Contexts != null && Contexts.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public bool HasEntities
        {
            get
            {
                if (Entities != null && Entities.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }


        public RequestExtras()
        {
        }

        public RequestExtras(List<AIContext> contexts, List<Entity> entities)
        {
            this.Contexts = contexts;
            this.Entities = entities;
        }

        public void CopyTo(AIRequest request)
        {
            if (HasContexts)
            {
                request.Contexts = Contexts;
            }

            if (HasEntities)
            {
                request.Entities = Entities;
            }
        }

    }
}

