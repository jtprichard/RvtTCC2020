using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RvtElectrical
{
    public class DeviceConnector
    {
        public Element Connector { get; private set; }                                                  //Connector Element
        public int BoxId { get; private set; }                                                          //Box ID Value
        public DeviceId DeviceId { get; private set; }                                                  //Device ID Value
        public System System                                                                            //Device Connector System
        {
            get
            {
                return DeviceId.System;
            }
            private set { }
        }
        public int Poles                                                                                //Number of circuit poles
        {
            get
            {
                switch (DeviceId.Voltage)
                {
                    case Voltage.LowVoltage:
                        return 1;
                    case Voltage.High120V1Ph:
                        return 1;
                    case Voltage.High208V1Ph:
                        return 2;
                    case Voltage.High208V3Ph:
                        return 3;
                    case Voltage.High277V1Ph:
                        return 1;
                    case Voltage.High480V3Ph:
                        return 2;
                    default:
                        return 0;
                }
            }
            private set { }
        }
        public string Venue { get; private set; }                                                       //Venue
        public int ConnectorCircuit                                                                     //Connector Circuit
        {
            get
            {
                return Connector.get_Parameter(TCCElecSettings.ConnectorCircuitGuid).AsInteger();       
            }
            private set { }
        }
        public string ConnectorCircuitConcat                                                            //Connector Concatenated Circuit
        {
            get
            {
                return Connector.get_Parameter(TCCElecSettings.ConnectorCircuitConcatGuid).AsString(); 
            }
            private set { }
        }
        public string ConnectorLabelPrefix                                                              //Connector Label Prefix
        {
            get
            {
                if (null == Connector.get_Parameter(TCCElecSettings.ConnectorLabelPrefixGuid).AsString())
                    return "";
                else
                    return Connector.get_Parameter(TCCElecSettings.ConnectorLabelPrefixGuid).AsString();
            }
            private set { }
        }
        public string ConnectorLabelOther                                                              //Connector Alternate Label
        {
            get
            {
                return Connector.get_Parameter(TCCElecSettings.ConnectorLabelOtherGuid).AsString();
            }
            private set { }
        }
        public int ConnectorPosition                                                                    //Connector Position Value
        {
            get
            {
                return Connector.get_Parameter(TCCElecSettings.ConnectorPositionGuid).AsInteger();      
            }
            private set { }
        }
        public int ConnectorSubPosition                                                                 //Connector Sub Position Value
        {
            get
            {
                return Connector.get_Parameter(TCCElecSettings.ConnectorSubPositionGuid).AsInteger();   
            }
            private set { }
        }
        public string ConnectorGroupCode { get; private set; }    

        public ElectricalCircuitData CircuitData { get; private set; }
        
        public DeviceConnector(Element ele, string venue, Document doc)
        {
            //Store Connector Element
            Connector = ele;

            try
            {
                //Store required parameter information
                DeviceId = new DeviceId(DeviceId.GetDeviceId(ele, doc));
                BoxId = ele.get_Parameter(TCCElecSettings.BoxIdGuid).AsInteger();
                CircuitData = new ElectricalCircuitData(ele, doc);
                Venue = venue;

                //Get Connector Group Code
                ElementId eId = ele.GetTypeId();
                ElementType eType = doc.GetElement(eId) as ElementType;
                if (eType.get_Parameter(TCCElecSettings.ConnectorGroupCodeGuid) != null)
                    ConnectorGroupCode = eType.get_Parameter(TCCElecSettings.ConnectorGroupCodeGuid).AsString();
                else
                    ConnectorGroupCode = null;
            }
            catch (NullReferenceException ex)
            {
                TaskDialog.Show("Connector Family Error", "Device Connector is missing required parameters.  Verify parameter");
                throw new Exception(ex.Message);
            }

        }
        public void UpdateConnectorConcat(string connectorCircuitConcat)
        {
            Parameter param = Connector.get_Parameter(TCCElecSettings.ConnectorCircuitConcatGuid);
            param.Set(connectorCircuitConcat);
            return;
        }
        public void ChangeConnectorId(DeviceId Id)
        //Changes the ConnectorId
        //This is used to change Mult Ids to match their connector types
        {
            DeviceId = Id;
            return;
        }
        static public DeviceId GetMultConnectorSubId(IList<DeviceConnector> connectors, int connectorPosition)
        //Get Multiconnector connectors
        //If they are identical, return the first sub connector DeviceId
        //If there is more than one service type, return the multiconnector DeviceId
        {
            List<DeviceConnector> multConnectors = new List<DeviceConnector>();
            DeviceId multId= new DeviceId(0);
            //A list of Device Ids to see if more than one service exists in the multi connector
            List<DeviceId> deviceIds = new List<DeviceId>();

            foreach (DeviceConnector deviceConnector in connectors)
            {
                //Iterate through all connectors passed and determine if the connections are part of the multiconnector
                if (deviceConnector.ConnectorPosition == connectorPosition)
                {
                    if (!deviceConnector.DeviceId.IsMultConnector())
                    {
                        multConnectors.Add(deviceConnector);
                        deviceIds.Add(deviceConnector.DeviceId);
                    }
                    else
                    {
                        multId = deviceConnector.DeviceId;
                    }
                }
            }

            //Test for multiple DeviceIds in the connectors
            //If only 1, return the connector's device Id
            //If more than 1, return the multi's device Id
            List<int> deviceInt = new List<int>();
            foreach (DeviceId DeviceId in deviceIds)
            {
                deviceInt.Add(DeviceId.Value);
            }

            if (deviceInt.Distinct().Count() > 1)
            {
                return multId;
            }
            else
            {
                return multConnectors.First().DeviceId;
            }
        }

        static public List<DeviceConnector> GetConnectorsByBox(IList<DeviceBox> boxes, int connectorGroup)
        //Return any connector that matches the connector group type
        {
            List<DeviceConnector> connectors = new List<DeviceConnector>();
            foreach (DeviceBox box in boxes)
            {
                foreach (DeviceConnector connector in box.Connectors)
                {
                    if (connector.DeviceId.ConnectorGroup == connectorGroup)
                    {
                        connectors.Add(connector);
                    }
                }
            }

            return connectors;
        }
        static public List<DeviceConnector> GetConnectorsByBox(IList<DeviceBox> boxes)
        //Return any connectors in a list of DeviceBoxes
        {
            List<DeviceConnector> connectors = new List<DeviceConnector>();
            foreach(DeviceBox box in boxes)
            {
                foreach(DeviceConnector connector in box.Connectors)
                {
                       connectors.Add(connector);
                }
            }

            return connectors;
        }
    }
}
