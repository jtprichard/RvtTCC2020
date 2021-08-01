using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace RvtElectrical
{
    public class DeviceId
    {
        public int Value { get; private set; }                          //Device ID Value
        public System System                                            //System Value (10000000)
        {
            get
            {
                return (System)((Value / 10000000) * 10000000);
            }
            private set { }
        }         
        public SystemCode SystemCode
        {
            get
            {
                return (SystemCode)((Value / 10000000) * 10000000);
            }
            private set { }
        }
        public Device Device                                            //Device Value (1000000)
        {
            get
            {
                int sValue = ((Value % 10000000) / 1000000) * 1000000;
                return (Device)sValue;
            }
            private set { }
        }
        public Voltage Voltage                                              //Voltage Value (100000)
        {
            get
            {
                int vVoltage =(((Value % 1000000) / 100000) * 100000);
                return (Voltage)vVoltage;

            }
            private set { }
        }
        public int ConnectorGroup                                       //Connector Group Value (10000)
        {
            get
            {
                return (Value % 100000) / 1000;
            }
            private set { }
        }
        public int Connector                                            //Connector Value (999)
        {
            get
            {
                return (Value % 1000);
            }
            private set { }
        }
        public DeviceId(int id)
        {
            Value = id;
        }

        public bool IsLightingBox()
        //Returns True if device is a Lighting Box code
        {
            if (System == System.PerfLighting && Device == Device.Faceplate) return true;
            else return false;
        }
        public bool IsArchLightingBox()
        //Returns True if device is a Arch Lighting Box code
        {
            if (System == System.ArchLighting && Device == Device.Faceplate) return true;
            else return false;
        }
        public bool IsCablePass()
        //Returns True if device is a Cable Pass
        {
            if (System == System.CablePasses && Device == Device.Faceplate) return true;
            else return false;
        }

        public bool IsSVCBox()
        //Returns True if device is an SVC Box code
        {
            if (System == System.PerfSVC && Device == Device.Faceplate) return true;
            else return false;
        }
        public bool IsMachBox()
        //Returns True if device is a Machinery Box code
        {
            if (System == System.PerfMachinery && Device == Device.Faceplate) return true;
            else return false;
        }
        public bool IsGeneralBox()
        //Returns True if device is a General Box code
        {
            if (System == System.General && Device == Device.Faceplate) return true;
            else return false;
        }
        public bool IsDeviceBox()
        //Returns True if device is a Lighting, SVC, or Machiner Box code
        {

            if (IsLightingBox() || IsSVCBox() || IsMachBox() || IsArchLightingBox() || IsGeneralBox())
            {
                return true;
            }
            else return false;
        }
        public bool IsFaceplate()
        //Method determines if the device ID is for a faceplate
        {
            if (Device == Device.Faceplate) return true;
            else return false;
        }
        public bool IsMultConnector()
        //Method determins if the device ID is for a multi-conductor connector
        {
            if (Connector > 0 && Connector<100) return true;
            else return false;
        }
        public Boolean IsLightingPanel()
        {
            if (System == System.PerfLighting && Device == Device.Panel) return true;
            else return false;
        }
        public Boolean IsSVCPanel()
        {
            if (System == System.PerfSVC && Device == Device.Panel) return true;
            else return false;
        }
        public Boolean IsMachPanel()
        {
            if (System == System.PerfMachinery && Device == Device.Panel) return true;
            else return false;
        }
        public Boolean IsGeneralPanel()
        {
            if (System == System.General && Device == Device.Panel) return true;
            else return false;
        }
        public Boolean IsPanel()
        {
            if (Device == Device.Panel) return true;
            else return false;
        }
        public static int GetDeviceId(Element ele, Document doc)
        //This method searches for both an Instance and Type Shared parameter'
        //A Type parameter is returned first if found 
        {
            Guid guid = TCCElecSettings.DeviceIdGuid;
            
            ElementType etype = doc.GetElement(ele.GetTypeId()) as ElementType;
            Parameter typeDeviceId = etype.get_Parameter(guid);
            Parameter DeviceId = ele.get_Parameter(guid);

            if (typeDeviceId.HasValue) return typeDeviceId.AsInteger();
            else return DeviceId.AsInteger();
        }
    }
}
