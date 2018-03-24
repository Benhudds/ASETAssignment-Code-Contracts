using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASETAssignment.CarPark
{
    public enum SpaceEnumeration
    {
        Free,
        Reserved,
        Purchased
    }

    public class Space
    {
        public bool Premium { get; set; }
        public SpaceEnumeration State { get; set; }
        public int CustomerId { get; set; }
    }
}
