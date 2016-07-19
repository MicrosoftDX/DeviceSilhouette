using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;


namespace StateManagmentServiceWebApiTest.CustomRules
{

    public class JsonExtractionRule : ExtractionRule


    {
        public String Token { get; set; }


        public override void Extract(object sender, ExtractionEventArgs e)
        {
            
            var jsonString = e.Response.BodyString;
            var json = JObject.Parse(jsonString);
            JToken jToken = null;

            if (json == null)
            {

                e.Success = false;
                e.Message = "Response received not in JSON format";
            }
            else
            {
                jToken = json.SelectToken(Token);
                if (jToken == null)
                {
                    e.Success = false;
                    e.Message = String.Format("{0} : Not found", Token);
                }
                else
                {
                    e.Success = true;
                    //e.Success = validateSchema(json);
                    e.WebTest.Context.Add(this.ContextParameterName, jToken);
                }
            }
        }



        // TODO: Solve this bug
        //    This method reqiuers to install Nuget pakage: Newtonsoft.Json.Schema
        //    But once installed it is imposible to add Extraction Rule to a web test
        //    There is an exception about the Newtonsoft.Json version
        private bool validateSchema(JObject jsonToValidate)
        {

            string silhouetteSchema =
             @"{
                  'type': 'object',
                  'properties': {
                    'deviceId': {
                      'type': 'string'
                    },
                    'timestamp': {
                      'type': 'string'
                    },
                    'version': {
                      'type': 'integer'
                    },
                    'correlationId': {
                      'type': 'string'
                    },
                    'messageType': {
                      'type': 'string'
                    },
                    'messageStatus': {
                      'type': 'string'
                    },
                    'appMetadata': {
                      'type': 'string'
                    },
                    'values': {
                      'type': 'string'
                    }
                  },
                  'required': [
                    'deviceId',
                    'timestamp',
                    'version',
                    'correlationId',
                    'messageType',
                    'messageStatus',
                    'appMetadata',
                    'values'
                  ]
                }";

            //JSchema schema = JSchema.Parse(silhouetteSchema);

            // jsonToValidate.IsValid(schema);
            return true;


        }
    }



}
