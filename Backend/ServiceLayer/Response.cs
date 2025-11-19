
using System.Text.Json.Serialization;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class Response
    {
        [JsonInclude]
        public string? ErrorMessage { get; set; }
        [JsonInclude]
        public object? ReturnValue { get; set; }

        [JsonIgnore]
        public bool ErrorOccurred => ErrorMessage != null;

        public Response() { }

        public Response(object value)
        {
            ReturnValue = value;
        }

        public Response(string errorMessage, bool is_error)
        {
            ErrorMessage = errorMessage;
        }

        public Response(string errorMessage, object value)
        {
            ErrorMessage = errorMessage;
            ReturnValue = value;
        }

    }
}

