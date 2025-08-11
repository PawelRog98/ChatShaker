using ChatShaker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Entities
{
    public class Suspension
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; }
        public SuspensionStatus Status { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long SuspendedById { get; set; }
        public User SuspendedBy { get; set; }
    }
}
