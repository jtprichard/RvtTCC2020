using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtRigging
{
    public static class TCCRiggingSettings
    {
        public static Guid IsLinesetGuid { get; private set; }                                      //GUID for Shared Parameter TCC_IS_RIGGING_LINESET
        public static Guid IsMotorizedGuid { get; private set; }                                    //GUID for Shared Parameter TCC_IS_MOTORIZED
        public static Guid IsStaticGuid { get; private set; }                                       //GUID for Shared Parameter TCC_IS_STATIC
        public static Guid IsCounterweightGuid { get; private set; }                                //GUID for Shared Parameter TCC_IS_COUNTERWEIGHT
        public static Guid IsStageDrapeGuid { get; private set; }                                   //GUID for Shared Parameter TCC_IS_DRAPE_STAGE
        public static Guid IsAcousticDrapeGuid { get; private set; }                                //GUID for Shared Parameter TCC_IS_DRAPE_ACOUSTIC
        public static Guid LinesetNumberGuid { get; private set; }                                  //GUID for Shared Parameter TCC_LINESET_NUMBER
        public static Guid LinesetDistanceGuid { get; private set; }                                //GUID for Shared Parameter TCC_LINESET_DISTANCE
        public static Guid LinesetNameGuid { get; private set; }                                    //GUID for Shared Parameter TCC_LINESET_NAME



        static TCCRiggingSettings()
        {
            //ADD ABILITY TO STORE THIS IN CONFIG FILE
            
            IsLinesetGuid = Guid.Parse("381c03df-a222-4b69-a152-71ed2d1c123f");                     //Input GUID for TCC_IS_RIGGING_LINESET
            IsMotorizedGuid = Guid.Parse("4e426dfc-d325-404f-82a4-59a186af832c");                   //Input GUID for TCC_IS_MOTORIZED
            IsStaticGuid = Guid.Parse("13135a1d-614a-4fca-b9b0-733fe2770477");                      //Input GUID for TCC_IS_STATIC
            IsCounterweightGuid = Guid.Parse("87842e96-a7cf-46e5-a012-d67e7aa33063");               //Input GUID for TCC_IS_COUNTERWEIGHT
            IsStageDrapeGuid = Guid.Parse("ddb0d263-591f-418a-9bc0-2613ddf88ce6");                  //Input GUID for TCC_IS_DRAPE_STAGE
            IsAcousticDrapeGuid = Guid.Parse("666ba46c-bed9-4e1f-bcd9-6e421d10a504");               //Input GUID for TCC_IS_DRAPE_ACOUSTIC
            LinesetNumberGuid = Guid.Parse("e7205f5a-4fc5-4cb7-8dfc-61a5d33f7ddc");                     //Input GUID for TCC_LINESET_NUMBER
            LinesetDistanceGuid = Guid.Parse("0b6abe0f-3ba1-4cd4-b0a5-4a7dd68a984b");                   //Input GUID for TCC_IS_LINESET_DISTANCE
            LinesetNameGuid = Guid.Parse("71afe759-9cfd-4fe3-a5a0-e5fdf2f5a3d1");                       //Input GUID for TCC_IS_LINESET_NAME

        }
    }
}
