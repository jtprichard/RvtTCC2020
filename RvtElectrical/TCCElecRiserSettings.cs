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
        public static Guid RiserCircuitR1 { get; private set; }
        public static Guid RiserCircuitR2 { get; private set; }
        public static Guid RiserCircuitR3 { get; private set; }
        public static Guid RiserCircuitR4 { get; private set; }
        public static Guid RiserCircuitR5 { get; private set; }
        public static Guid RiserCircuitR6 { get; private set; }
        public static Guid RiserCircuitR7 { get; private set; }
        public static Guid RiserCircuitR8 { get; private set; }
        public static Guid RiserCircuitR9 { get; private set; }
        public static Guid RiserCircuitR10 { get; private set; }
        public static Guid RiserCircuitR11 { get; private set; }
        public static Guid RiserCircuitR12 { get; private set; }

        public static Guid RiserCircuitL1 { get; private set; }
        public static Guid RiserCircuitL2 { get; private set; }
        public static Guid RiserCircuitL3 { get; private set; }
        public static Guid RiserCircuitL4 { get; private set; }
        public static Guid RiserCircuitL5 { get; private set; }
        public static Guid RiserCircuitL6 { get; private set; }
        public static Guid RiserCircuitL7 { get; private set; }
        public static Guid RiserCircuitL8 { get; private set; }
        public static Guid RiserCircuitL9 { get; private set; }
        public static Guid RiserCircuitL10 { get; private set; }
        public static Guid RiserCircuitL11 { get; private set; }
        public static Guid RiserCircuitL12 { get; private set; }



        public TCCElecRiserSettings()
        {

            //RISER CIRCUITS
            RiserCircuitR1 = Guid.Parse("2012b68f-64d2-406c-a589-ae8373c382e1");
            RiserCircuitR2 = Guid.Parse("fe2913dd-6054-42aa-bfa7-0841d7ad4391");
            RiserCircuitR3 = Guid.Parse("b034ac56-f71a-4a9d-b9db-7b53ee60efec");
            RiserCircuitR4 = Guid.Parse("e280deb4-7228-472c-baaa-a6b75a8d7214");
            RiserCircuitR5 = Guid.Parse("6d479dfb-df6e-4d87-a387-50d424cc8c8d");
            RiserCircuitR6 = Guid.Parse("fa04ae57-b895-4723-be02-c72a736dccd3");
            RiserCircuitR7 = Guid.Parse("c208e768-9b93-4624-b6ea-710feec866ef");
            RiserCircuitR8 = Guid.Parse("f6df0b6c-6129-4944-8e4b-907dcdca3625");
            RiserCircuitR9 = Guid.Parse("4805e801-2e4e-4ff8-b59d-04705d722a7d");
            RiserCircuitR10 = Guid.Parse("d6042875-4590-4e23-ba25-443c0c10e481");
            RiserCircuitR11 = Guid.Parse("78890bba-62c8-4d0d-be3e-14f64d5d8742");
            RiserCircuitR12 = Guid.Parse("8bda87a9-d225-483b-9424-76f85db05be5");

            RiserCircuitL1 = Guid.Parse("cf691b5a-ff89-4f8b-9531-ade4d8d56de7");
            RiserCircuitL2 = Guid.Parse("7245fca8-43c5-4d82-94aa-7ab44e0d690b");
            RiserCircuitL3 = Guid.Parse("4b9b4e0e-299e-4fe4-ad59-d0644211a48c");
            RiserCircuitL4 = Guid.Parse("4d3cd4d2-6306-463c-a392-4aa59a109e84");
            RiserCircuitL5 = Guid.Parse("910c5f14-90e6-4b94-af45-44de8e6c119e");
            RiserCircuitL6 = Guid.Parse("5766d648-007d-43ab-8432-da8486797bd4");
            RiserCircuitL7 = Guid.Parse("9da3873e-1a99-495e-86fc-116adaacaf6a");
            RiserCircuitL8 = Guid.Parse("a399b031-0c08-437d-94ad-ce14a2dd5d36");
            RiserCircuitL9 = Guid.Parse("59280f6e-feed-422b-8d6a-3eaae4734e8a");
            RiserCircuitL10 = Guid.Parse("45cdb366-4e79-48c4-a9c1-da829087dff2");
            RiserCircuitL11 = Guid.Parse("c46c4383-21aa-4ef7-ae6b-defe6c2507b1");
            RiserCircuitL12 = Guid.Parse("49130ffa-9cbe-435b-bcba-9e2bce50e7ed");

        }



    }


}
