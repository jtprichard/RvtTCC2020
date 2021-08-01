using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;

namespace RvtElectrical
{

    public class IUCircuitLabel : IUpdater
    //UPdater that updates the concatenated label of device box circuits
    {

        static IUCircuitLabel _updater = null;
        
        // DMU updater to call class to update device box concat label to new entry

        static AddInId _appId;
        static UpdaterId _updaterId;

        public IUCircuitLabel(AddInId id)
        {
            _appId = id;
            _updaterId = new UpdaterId(_appId, new Guid(
                "77128A86-8102-4605-B2F9-A1939D193A00"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Application app = doc.Application;

            foreach (ElementId eid in data.GetModifiedElementIds())
            {
                try
                {
                    Element ele = doc.GetElement(eid);
                    //Get box number for changed element
                    int boxNumber = ele.get_Parameter(TCCElecSettings.BoxIdGuid).AsInteger();

                    //Get Devicebox Associated with Connector
                    FamilyInstance fi = ele as FamilyInstance;
                    //Test once for connectors
                    if (fi.SuperComponent != null)
                    {
                        ele = fi.SuperComponent;
                        fi = ele as FamilyInstance;
                    }

                    //Test again in case subconnectors
                    if (fi.SuperComponent != null)
                    {
                        ele = fi.SuperComponent;
                    }

                    //Create Device Box
                    if (DeviceBox.IsDeviceBox(ele, doc))
                    {
                        DeviceBox db = new DeviceBox(doc, ele);
                        db.UpdateDeviceBoxConcat();
                    }


                    ////Get devicebox elements that match the box number (includes connectors)
                    //IList<Element> deviceBoxElements = ElecUtils.CollectDeviceByBoxNumber(doc, boxNumber);
                    //if (deviceBoxElements.Count > 0)
                    //{
                    //    //Create a DeviceBox
                    //    DeviceBox deviceBox = new DeviceBox(doc, deviceBoxElements);
                    //    //Update the circuits
                    //    deviceBox.UpdateDeviceBoxConcat();
                    //}
                }

                catch(Exception ex)
                {
                    UIUtils.EBSGenException(ex);
                }
            }
        }
        public string GetAdditionalInformation()
        {
            return "Entertainment BIM Solutions";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPFixtures;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "CircuitLabelUpdater";
        }
    }
}
