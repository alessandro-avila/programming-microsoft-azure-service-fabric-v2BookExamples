using System;
using System.Collections.Generic;
using System.Fabric.Chaos.DataStructures;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaosTest
{
    class ChaosEventComparer : IEqualityComparer<ChaosEvent>
    {
        public bool Equals(ChaosEvent x, ChaosEvent y)
        {
            return x.TimeStampUtc.Equals(y.TimeStampUtc);
        }

        public int GetHashCode(ChaosEvent obj)
        {
            return obj.TimeStampUtc.GetHashCode();
        }
    }
}
