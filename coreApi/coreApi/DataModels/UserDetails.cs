using Newtonsoft.Json;
using System.Collections.Generic;


namespace coreApi.DataModels
{
    public class UserDetails
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("username")]
        public string USERNAME { get; set; }

        [JsonProperty("userpasword")]
        public string USERPASSWORD { get; set; }

        [JsonProperty("employeename")]
        public string EMPLOYEENAME { get; set; }
    }

    public class UserDetailsResponse
    {
        [JsonProperty("userdetailslist")]
        public List<UserDetails> UserDetailsList { get; set; }

        [JsonProperty("responsestatus")]
        public string RESPONSESTATUS { get; set; }

        [JsonProperty("remarks")]
        public string REMARKS { get; set; }
    }

    public class SaveResponseModel
    {
        [JsonProperty]
        public string UniqueId { get; set; }

        [JsonProperty("responsestatus")]
        public string RESPONSESTATUS { get; set; }

        [JsonProperty("remarks")]
        public string REMARKS { get; set; }
    }
}
