using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class ChangeRolePayload
    {
        public string? UserName { get; set; }
        public IEnumerable<string>? Roles { get; set; }
    }

    public class UserPayload
    {
        public string? UserName { get; set; }
        public string? UserMail { get; set; }
        public string? Password { get; set; }
    }
}
