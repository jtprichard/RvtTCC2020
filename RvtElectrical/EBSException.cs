using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtElectrical
{
    class EBSException:Exception
    {
        public EBSException()
        {

        }

        public EBSException(string ex)
            :base(string.Format("Runtime Error - Contact Provider\n" + ex))
        {

        }
    }
}
