using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Models
{
    public class EnergyData
    {
        public string BuildId { get; set; }
        public string MeterCode { get; set; }
        public DateTime Time { get; set; }
        public float Value { get; set; }
        public bool Calced { get; set; }
    }
}
