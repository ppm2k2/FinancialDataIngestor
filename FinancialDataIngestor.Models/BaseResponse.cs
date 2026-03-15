using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Models
{
    public class BaseResponse
    {
        public bool IsSuccessful { get; set; }
        public List<string> Messages { get; set; }

    }
    public class Response
    {
        public bool IsSuccessful { get; set; }
        public List<string> Message { get; set; }

    }

    public class Response<T> where T : new()
    {
        public Response()
        {
            ServiceReponse = new Response();
            Result = new T();
        }
        public Response ServiceReponse { get; set; }
        public T Result { get; set; }
    }
    public class RepetedResponse<T> where T : class, new()
    {
        public RepetedResponse()
        {
            ServiceReponse = new Response();
            Result = new List<T>();
        }
        public Response ServiceReponse { get; set; }
        public List<T> Result { get; set; }
    }


}
