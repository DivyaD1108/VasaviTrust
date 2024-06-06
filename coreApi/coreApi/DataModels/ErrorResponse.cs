using Newtonsoft.Json;

namespace coreApi.DataModels
{
    public class ErrorResponse
    {
        [JsonProperty("errormessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("errorcode")]

        public int ErrorCode { get; set; }
    }
}
