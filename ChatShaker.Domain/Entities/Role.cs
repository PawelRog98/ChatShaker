using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Entities
{
    public class Role
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string RoleName { get; set; }
    }
}
