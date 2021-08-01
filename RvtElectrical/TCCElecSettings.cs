using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtElectrical
{
    public static class TCCElecSettings
    {
        public static Guid BoxIdGuid { get; private set; }                                             //GUID for Shared Parameter TCC_BOX_ID
        public static int BoxIdEID { get; private set; }                                               //ElementID for Shared Parameter TCC_BOX_ID
        public static Guid PlateCodeGuid { get; private set; }                                         //GUID for Shared Parameter TCC_PLATE_CODE
        public static Guid DeviceIdGuid { get; private set; }                                          //GUID for Shared Parameter TCC_DEVICE_ID
        public static Guid ConnectorCircuitGuid { get; private set; }                                  //GUID for Shared Parameter TCC_CONNECTOR_CIRCUIT
        public static int ConnectorCircuitEID { get; private set; }                                    //ElementID for Shared Parameter TCC_CONNECTOR_CIRCUIT
        public static Guid ConnectorCircuitConcatGuid { get; private set; }                            //GUID for Shared Parameter TCC_CONNECTOR_CIRCUIT_CONCAT
        public static Guid ConnectorPositionGuid { get; private set; }                                 //GUID for Shared Parameter TCC_CONNECTOR_POSITION
        public static Guid ConnectorSubPositionGuid { get; private set; }                              //GUID FOR Shared Parameter TCC_CONNECTOR_SUB_POSITION
        public static Guid ConnectorLabelPrefixGuid { get; private set; }                              //GUID for Shared Parameter TCC_CONNECTOR_LABEL_PREFIX
        public static int ConnectorLabelPrefixEID { get; private set; }                                //ElementID for Shared Paramter TCC_CONNECTOR_LABEL_PREFIX
        public static Guid ConnectorLabelOtherGuid { get; private set; }                               //GUID for Shared Parameter TCC_CONNECTOR_LABEL_OTHER
        public static int ConnectorLabelOtherEID { get; private set; }                                 //ElementID for Shared Parameter TCC_CONNECTOR_LABEL_OTHER
        public static Guid VenueGuid { get; private set; }                                             //GUID for Shared Parameter TCC_VENUE
        public static Guid PanelStartAddress { get; private set; }                                     //GUID for Shared Parameter TCC_PANEL_ADDRESS_START
        public static Guid PanelEndAddress { get; private set; }                                       //GUID for Shared Parameter TCC_PANEL_ADDRESS_END
        public static Guid ConnectorGroupCodeGuid { get; private set; }                                //GUID for Shared Parameter TCC_CONNECTOR_GROUP_CODE
        public static Guid BackboxSizeGuid { get; private set; }                                       //GUID for Shared Parameter TCC_BOX_SIZE
        public static Guid BackboxCodeGuid { get; private set; }                                       //GUID for Shared Parameter TCC_BACKBOX_CODE
        public static Guid PlateFamilyCodeGuid { get; private set; }                                   //GUID for Shared Parameter TCC_PLATE_FAMILY_CODE
        public static Guid BoxClassificationCode { get; private set; }                                 //GUID for Share Parameter TCC_BOX_CLASSIFICATION_CODE
        public static Guid CSLengthGuid { get; private set; }                                          //GUID for Share Parameter TCC_CS_LENGTH


        static TCCElecSettings()
        {
            //ADD ABILITY TO STORE THIS IN CONFIG FILE
            
            BoxIdGuid = Guid.Parse("db446056-e38b-48a7-88ce-8f2e3279a214");                     //Input GUID for TCC_BOX_ID here
            //BoxIdEID = 2528989;                                                                 //Input Element ID for TCC_BOX_ID here
            PlateCodeGuid = Guid.Parse("d6e3c843-f345-423a-ae7c-eb745db1540c");                 //Input GUID for TCC_PLATE_CODE here
            DeviceIdGuid = Guid.Parse("0bfecf3e-35f3-4245-b8b4-970a9faa5dc5");                  //Input GUID for TCC_DEVICE_ID here
            ConnectorCircuitGuid = Guid.Parse("fa543fe0-ac03-45ca-addc-3ca4f013f390");          //Input GUID for TCC_CONNECTOR_CIRCUIT_GUID here
            //ConnectorCircuitEID = 2528990;                                                      //Input Element ID for TCC_CONNECTOR_CIRCUIT here
            ConnectorCircuitConcatGuid = Guid.Parse("426d707f-6786-4b7d-be45-174dafad3c6c");    //Input GUID for TCC_CONNECTOR_CIRCUIT_CONCAT_GUID here
            ConnectorPositionGuid = Guid.Parse("94823938-8ab4-43c9-ab93-3a2c023e24b4");         //Input GUID for TCC_CONNECTOR_POSITION here
            ConnectorSubPositionGuid = Guid.Parse("8d166dbb-06a1-4c30-8718-ce2827bcf113");      //Input GUID for TCC_CONNECTOR_SUB_POSITION here
            ConnectorLabelOtherGuid = Guid.Parse("3ae80c00-b583-4c9b-aee1-40c646091616");       //Input GUID for TCC_CONNECTOR_LABEL_OTHER here
            //ConnectorLabelOtherEID = 3847511;                                                   //Input Element ID for TCC_CONNECTOR_lABEL_OTHER here
            ConnectorLabelPrefixGuid = Guid.Parse("ca878977-7356-4c4a-9102-96dcda5155df");      //Input GUID for TCC_CONNECTOR_LABEL_PREFIX here
            //ConnectorLabelPrefixEID = 3847513;                                                  //Inputer Element ID for TCC_CONNECTOR_LABEL_PREFIX here
            VenueGuid = Guid.Parse("06cd13cc-a2fb-4324-99cb-69becc15d0e1");                     //Input GUID for TCC_VENUE here
            PanelStartAddress = Guid.Parse("3e48b7f5-aa12-4ca3-86ba-956c209d205f");             //Input GUID for TCC_PANEL _ADDRESS_START
            PanelEndAddress = Guid.Parse("fae4a59d-69ad-402e-bd3d-ecb8d4181906");               //Input GUID for TCC_PANEL _ADDRESS_END
            ConnectorGroupCodeGuid = Guid.Parse("02e83556-85a3-4f36-b06f-989d0772adcf");        //Input GUID for TCC_CONNECTOR_GROUP_CODE
            BackboxSizeGuid = Guid.Parse("8575f2d3-f556-4066-8043-39f58fce37b4");               //Input GUID for TCC_BOX_SIZE
            BackboxCodeGuid = Guid.Parse("a8dadd06-2281-4264-9d92-dc3c48cefcbb");               //Input GUID for TCC_BACKBOX_CODE   
            PlateFamilyCodeGuid = Guid.Parse("17cb1c20-8320-4ca1-bdac-7f7a856b9e33");           //Input GUID for TCC_PLATE_FAMILY_CODE
            BoxClassificationCode = Guid.Parse("43e68510-ebb5-4761-bbc4-85c438bba8c7");         //Input GUID for TCC_BOX_CLASSIFICATION_CODE
            CSLengthGuid = Guid.Parse("79a02055-1d34-4ba0-a5d7-a14ecf89b960");                  //Input GUID for TCC_CS_LENGTH

        }
    }
}
