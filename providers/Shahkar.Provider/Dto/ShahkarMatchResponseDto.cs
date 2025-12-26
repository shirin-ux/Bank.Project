using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shahkar.Provider.Dto
{
    public class ShahkarMatchResponseDto
    {

        public bool isMatched { get; set; }

        public ResponseContextDto responseContext { get; set; } = default!;
    }

    public class ResponseContextDto
    {

        public ResponseStatusDto status { get; set; } = default!;

        public string? requestId { get; set; }

        public string? correlationId { get; set; }


        public string? navigationURI { get; set; }

   
        public string? nextStepToken { get; set; }

        public string? userSessionId { get; set; }

        public Dictionary<string, JsonElement>? custom { get; set; }
    }

    public class ResponseStatusDto
    {

        public int code { get; set; }


        public string? message { get; set; }

        public List<JsonElement>? details { get; set; }
    }
}

