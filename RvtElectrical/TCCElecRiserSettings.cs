using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtElectrical
{
    public class TCCElecRiserSettings
    {

        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

        public string Bar { get; set; }

        //RISER LABELS
        public static Guid RiserCircuit1 { get; private set; }
        public static Guid RiserCircuit2 { get; private set; }
        public static Guid RiserCircuit3 { get; private set; }
        public static Guid RiserCircuit4 { get; private set; }
        public static Guid RiserCircuit5 { get; private set; }
        public static Guid RiserCircuit6 { get; private set; }


        public TCCElecRiserSettings()
        {

            //RISER CIRCUITS
            RiserCircuit1 = Guid.Parse("2012b68f-64d2-406c-a589-ae8373c382e1");
            RiserCircuit2 = Guid.Parse("fe2913dd-6054-42aa-bfa7-0841d7ad4391");
            RiserCircuit3 = Guid.Parse("b034ac56-f71a-4a9d-b9db-7b53ee60efec");
            RiserCircuit4 = Guid.Parse("e280deb4-7228-472c-baaa-a6b75a8d7214");
            RiserCircuit5 = Guid.Parse("6d479dfb-df6e-4d87-a387-50d424cc8c8d");
            RiserCircuit6 = Guid.Parse("fa04ae57-b895-4723-be02-c72a736dccd3");

        }



    }


}
