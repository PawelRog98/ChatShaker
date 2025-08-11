using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Abstractions
{
    public interface ICommonData
    {
        long Id { get; set; }
        Guid PublicId { get; set; }
    }
}
