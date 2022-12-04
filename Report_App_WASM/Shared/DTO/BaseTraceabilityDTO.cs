using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.DTO
{
    public class BaseTraceabilityDTO
    {
        public DateTime CreateDateTime { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string? CreateUser { get; set; }
        public DateTime ModDateTime { get; set; }
        [MaxLength(100)]
        public string? ModificationUser { get; set; }
    }
}
