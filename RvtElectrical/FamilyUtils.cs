using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace RvtElectrical
{
    public static class FamilyUtils
    // UTILITY CLASS FOR SHARED FAMILY PROCEDURES
    {
        public static void AssociateParameters (List<Parameter> elementParams, List<FamilyParameter> famParams, Document doc)
        {
            //Associate Device Box Parameters
            foreach (Parameter deviceBoxParam in elementParams)
            {
                foreach (FamilyParameter famParam in famParams)
                {
                    if (deviceBoxParam.Definition.Name == famParam.Definition.Name && 
                        doc.FamilyManager.CanElementParameterBeAssociated(deviceBoxParam) &&
                        CanParamAssociate(deviceBoxParam))
                            doc.FamilyManager.AssociateElementParameterToFamilyParameter(deviceBoxParam, famParam);
                }
            }
        }

        public static DeviceId GetTemplateSystemCode (FamilyManager fm)
        // Return System Code from Family Plate Template
        {
            //Get System Code
            List<int> deviceIdList = new List<int>();
            FamilyParameter famDeviceIdParam =fm.get_Parameter(TCCElecSettings.DeviceIdGuid);

            //Iterate through types to get parameter value
            //(Apparently no way to access forumla value directly)
            foreach (FamilyType t in fm.Types)
            {
                deviceIdList.Add(Convert.ToInt32(t.AsInteger(famDeviceIdParam)));
            }

            //Error check that Device Code template is correct
            if (deviceIdList.Distinct().Count() != 1)
            {
                TaskDialog.Show("System Code Error", "Error in family TCC_DEVICE_ID parameter.  Check Template File");
                return null;
            }

            return new DeviceId(deviceIdList[0]);

        }

        public static string GetTemplateBoxClassificationCode(FamilyManager fm)
        // Return Box Classification Code from Family Plate Template
        {
            List<string> codeList = new List<string>();
            FamilyParameter param = fm.get_Parameter(TCCElecSettings.BoxClassificationCode);

            //Check to see if a family parameter for Box Classification exists.  If it doesn't, return dummy info.
            if (null != param)
            {
                //Iterate through types to get parameter value
                //(Apparently no way to access forumla value directly)
                foreach (FamilyType t in fm.Types)
                {
                    codeList.Add(t.AsString(param));
                }

                //Error check that Device Code template is correct
                if (codeList.Distinct().Count() != 1)
                {
                    TaskDialog.Show("Device Code Error", "Error in family TCC_BOX_CLASSIFICATION_CODE Code parameter.  Check Template File");
                    return null;
                }
            }

            else
                return null;


            return codeList[0];
        }

        public static double? GetTemplateCSLength(FamilyManager fm)
        // Return Connector Strip Length from Family Plate Template
        {
            List<Double?> lengths = new List<Double?>();
            FamilyParameter param = fm.get_Parameter(TCCElecSettings.CSLengthGuid);

            //Iterate through types to get parameter value
            //(Apparently no way to access forumla value directly)
            foreach (FamilyType t in fm.Types)
            {
                //lengths.Add(Math.Floor(Convert.ToDouble(t.AsDouble(param))));
                lengths.Add(Convert.ToDouble(t.AsDouble(param)));
            }

            //Error check that Device Code template is correct
            if (lengths.Distinct().Count() != 1)
            {
                TaskDialog.Show("Device Code Error", "Error in family TCC_CS_LENGTH Code parameter.  Check Template File");
                return null;
            }

            return lengths[0];
        }

        public static bool IsSetConnectorPosition(string boxClassificationCode)
        //Determines if parameter association should set connector positions in routine
        {
            bool success = true;
            switch (boxClassificationCode)
            {
                //Generic code for Box Template
                case "B":
                    success = true;
                    break;
                //Power Box
                case "PB":
                    success = true;
                    break;
                //Control Box
                case "CB":
                    success = true;
                    break;
                //Universal Box
                case "UB":
                    success = true;
                    break;
                //Architectural Box
                case "AB":
                    success = false;
                    break;
                //Architectural Sensor
                case "AS":
                    success = false;
                    break;
                //Bussway
                case "BW":
                    success = false;
                    break;
                //Connector Strip
                case "CS":
                    success = false;
                    break;
                //Touch Screen
                case "TS":
                    success = false;
                    break;
                default:
                    TaskDialog.Show("Box Classification Code Warning", "There is an Error in the " +
                        "Box Classification Code parameter in the Template.  Please verify it.");
                    break;
            }

            return success;
        }
        public static bool IsDeviceBox(string boxClassificationCode)
        {
            bool success = false;
            switch (boxClassificationCode)
            {
                //Generic code for Box Template
                case "B":
                    success = true;
                    break;
                //Power Box
                case "PB":
                    success = true;
                    break;
                //Control Box
                case "CB":
                    success = true;
                    break;
                //Universal Box
                case "UB":
                    success = true;
                    break;
                //Not a Device Box
                default:
                    success = false;
                    break;
            }
            return success;
        }

        public static string getPlateClassCode(string boxClassificationCode)
        //returns a plate classification code for faceplate box naming for odd devices
        {
            string result = "";
            switch (boxClassificationCode)
            {
                //Generic code for Box Template
                case "B":
                    result = "ERROR";
                    break;
                //Power Box
                case "PB":
                    result = "ERROR";
                    break;
                //Control Box
                case "CB":
                    result = "ERROR";
                    break;
                //Universal Box
                case "UB":
                    result = "ERROR";
                    break;
                //Architectural Box
                case "AB":
                    result = "B";
                    break;
                //Architectural Box
                case "AS":
                    result = "S";
                    break;
                //Bussway
                case "BW":
                    result = "W";
                    break;
                //Connector Strip
                case "CS":
                    result = "C";
                    break;
                //Touch Screen
                case "TS":
                    result = "TS";
                    break;
                default:
                    result = "ERROR";
                    break;
            }
            return result;
        }
        private static bool CanParamAssociate(Parameter param)
        {
            bool result = true;
            try
            {
                if (param.GUID == TCCElecSettings.ConnectorSubPositionGuid)
                    result = false;
            }
            catch
            {

            }

            return result;
        }
    }
}
