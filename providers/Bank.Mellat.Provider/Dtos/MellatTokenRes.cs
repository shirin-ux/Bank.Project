using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bank.Mellat.Provider.Dtos
{
  public  class MellatTokenRes
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }


        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }


        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }


        [JsonPropertyName("jti")]
        public string Jti { get; set; }
    }
}
