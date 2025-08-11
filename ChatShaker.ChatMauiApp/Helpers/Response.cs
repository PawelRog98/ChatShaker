using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Helpers
{
    public class Response<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string[] Errors { get; set; } = null;
        public dynamic? MetaData { get; set; } = null;
        public string Message { get; set; } = string.Empty;

        public Response()
        {
        }
    }
}
