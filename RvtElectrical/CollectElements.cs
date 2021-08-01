using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Electrical;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CollectElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get GUID Settings from Setting Class
            Guid BoxIdGuid = TCCElecSettings.BoxIdGuid;

            //TEMP - Set Specific BoxNumber
            //int targetBoxNumber = 106;

            //Get UIDocument & Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // get all PanelScheduleView instances in the Revit document.
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            ElementClassFilter PanelScheduleViewsAreWanted = new ElementClassFilter(typeof(PanelScheduleView));
            fec.WherePasses(PanelScheduleViewsAreWanted);
            List<Element> psViews = fec.ToElements() as List<Element>;

            bool noPanelScheduleInstance = true;

            //foreach (Element element in psViews)
            //{
            //    PanelScheduleView psView = element as PanelScheduleView;
            //    if (psView.IsPanelScheduleTemplate())
            //    {
            //        // ignore the PanelScheduleView instance which is a template.
            //        continue;
            //    }
            //    else
            //    {
            //        noPanelScheduleInstance = false;
            //    }
            //}

            //IList<PanelScheduleView> panelScheduleViews = psViews as List<PanelScheduleView>;

            PanelScheduleView psv = psViews[0] as PanelScheduleView;

            ElementId panelId = psv.GetPanel();

            string panelName = panelId.ToString();

            //Create List to capture Boxes and Box Numbers for them
            //IList<Element> box = new List<Element>();
            //List<int> boxNumbers = new List<int>();

            //foreach(Element ele in devices)
            //{
            //    //Because of LX120Connector, must check for type and instance parameters
            //    ElementType etype = doc.GetElement(ele.GetTypeId()) as ElementType;
            //    Parameter typeDeviceId = etype.get_Parameter(DeviceIdGuid);
            //    Parameter deviceId = ele.get_Parameter(DeviceIdGuid);
            //    Parameter tempParam = typeDeviceId;
            //    if (typeDeviceId.HasValue)
            //    {
            //        tempParam = typeDeviceId;
            //    }
            //    else
            //    {
            //        tempParam = deviceId;
            //    }

            //    Parameter param = ele.get_Parameter(BoxIdGuid);
            //    if (param.AsInteger() == targetBoxNumber)
            //    {
            //        boxes.Add(ele);
            //        TaskDialog.Show("Parameter Values", string.Format("param Value Type:  {0}" + Environment.NewLine + "Box ID: {1}" + Environment.NewLine + "Device ID: {2}",
            //            param.StorageType.ToString(),
            //            param.AsInteger(),
            //            tempParam.AsInteger()));
            //    }

            //IList<int> boxNumbers = new List<int>();
            //foreach(Element device in devices)
            //{
            //    boxNumbers.Add(device.get_Parameter(BoxIdGuid).AsInteger());
            //}
            //boxNumbers.Distinct();



            //using (Transaction trans = new Transaction(doc, "Update User Circuits"))
            //{
            //    trans.Start();
            //    foreach(int targetBoxNumber in boxNumbers)
            //    {
            //        IList<Element> box = ElecUtils.CollectDeviceByBoxNumber(devices, targetBoxNumber, BoxIdGuid);
            //        DeviceBox deviceBox = new DeviceBox(doc, box);

            //        deviceBox.UpdateDeviceBoxConcat();
            //    }

            //    trans.Commit();
            //}


            //TaskDialog.Show("Parameter Values", string.Format("Device ID: {0}" + Environment.NewLine +
            //    "Box ID: {1}" + Environment.NewLine +
            //    "Connector1 Device ID: {2}",
            //    deviceBox.BoxDeviceId,
            //    deviceBox.BoxId,
            //    deviceBox.Connectors.First().DeviceId));


            



            //foreach(Element ele in boxes)
            //{
            //    Parameter param = ele.get_Parameter(BoxIdGuid);
            //    TaskDialog.Show("Parameter Values", string.Format("param Value Type:  {0}" + Environment.NewLine + "Box ID: {1}",
            //        param.StorageType.ToString(),
            //        param.AsInteger()));
            //}



            ////Collect all Boxes
            //foreach (Element ele in devices)
            //{
            //    //TCC_DEVICE_ID is a Type parameter, so must get element type for each element
            //    ElementType etype = doc.GetElement(ele.GetTypeId()) as ElementType;

            //    //Lookup to see if it is aTCC Device
            //    Parameter param = etype.get_Parameter(DeviceIdGuid);

            //    //Find the Box ID to add to list
            //    Parameter param2 = ele.get_Parameter(BoxIdGuid);

            //    if (param != null && ElecUtils.IsDeviceBox(param.AsInteger()))
            //    {
            //        boxes.Add(ele);
            //        boxNumbers.Add(param2.AsInteger());

            //    //    TaskDialog.Show("Parameter Values", string.Format("param Value Type:  {0}" + Environment.NewLine + "param Value: {1}" + Environment.NewLine + "Box ID: {2}",
            //    //        param.StorageType.ToString(),
            //    //        param.AsInteger(),
            //    //        param2.AsInteger()));
            //    }
            //}

            ////Get the unique box numbers removing duplicates
            //var uniqueBoxNumbers = boxNumbers.Distinct().ToList();
            //uniqueBoxNumbers.Sort();

            //foreach (int boxNumber in uniqueBoxNumbers)
            //{

            //    List<Element> boxDevices = new List<Element>();
            //    List<int> connectorIds = new List<int>();

            //    //Get all connectors in a box number into a list
            //    foreach (Element ele in boxes)
            //    {
            //        //Parameter param = ele.LookupParameter("TCC_BOX_ID");
            //        Parameter param = ele.get_Parameter(BoxIdGuid);

            //        if (param.AsInteger() == boxNumber)
            //        {
            //            Element deviceType = doc.GetElement(ele.GetTypeId()) as ElementType;
            //            Parameter deviceParam = deviceType.get_Parameter(DeviceIdGuid);
            //            //Parameter deviceParam = ele.get_Parameter(DeviceIdGuid);

            //            if (deviceParam != null)
            //            {
            //                connectorIds.Add((deviceParam.AsInteger()));
            //                boxDevices.Add(ele);
            //            }
            //        }
            //    }

            //    var uniqueConnectorIds = connectorIds.Distinct().ToList();
            //    uniqueConnectorIds.Sort();

            //    foreach (int connectorId in uniqueConnectorIds)
            //    {
            //        List<string> circuit = new List<String>();
            //        foreach (Element boxDevice in boxDevices)
            //        {
            //            Parameter deviceParam1 = boxDevice.get_Parameter(DeviceIdGuid);
            //            Parameter deviceParam2 = boxDevice.get_Parameter(ConnectorCircuitGuid);
            //            if (connectorId == (deviceParam1.AsInteger()/10))
            //            {
            //                circuit.Add(deviceParam2.AsInteger().ToString());
            //            }
            //        }
            //        string circuits = string.Join<string>(", ", circuit);

            //        foreach (Element boxDevice in boxDevices)
            //        {
            //            Parameter deviceParam = boxDevice.get_Parameter(ConnectorCircuitGuid);

            //            if (connectorId == (deviceParam.AsInteger() / 10))
            //            {
            //                using (Transaction trans = new Transaction(doc, "Set Circuit Parameter"))
            //                {
            //                    trans.Start();

            //                    Parameter deviceParam2 = boxDevice.get_Parameter(ConnectorCircuitConcatGuid);
            //                    deviceParam2.Set(circuits);

            //                    trans.Commit();
            //                }
            //            }

            //        }
            //    }

            //}

            //TaskDialog.Show("Lighting Plates", string.Format("{0} Lighting Plates Counted" + Environment.NewLine + "{1} Box Numbers Counted",
            //    boxes.Count(),
            //    uniqueBoxNumbers.Count()));

            return Result.Succeeded;
        }
    }
}
