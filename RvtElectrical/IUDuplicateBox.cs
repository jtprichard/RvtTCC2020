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

    public class IUDuplicateBox : IUpdater
    //UPdater that updates the concatenated label of device box circuits
    {

        static IUDuplicateBox _updater = null;
        
        // DMU updater to call class to update device box concat label to new entry

        static AddInId _appId;
        static UpdaterId _updaterId;

        public IUDuplicateBox(AddInId id)
        {
            _appId = id;
            _updaterId = new UpdaterId(_appId, new Guid(
                "7C4C85FD-8263-4C60-8D32-79BCD7D41634"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Application app = doc.Application;
            IList<ElementId> eids = (IList<ElementId>)data.GetModifiedElementIds();

            IList<Element> eles = new List<Element>();
            foreach (ElementId eid in eids)
            {
                eles.Add(doc.GetElement(eid));
            }

            foreach(Element ele in eles)
            { 
                if(DeviceBox.IsDeviceBox(ele, doc))
                {
                    int boxNumber = ele.get_Parameter(TCCElecSettings.BoxIdGuid).AsInteger();
                    IList<DeviceBox> deviceBoxes = DeviceBox.GetDuplicateDeviceBoxes(doc, boxNumber);
                    if (deviceBoxes.Count() > 1)
                    {
                        TaskDialog.Show("BOX DUPLICATION", "Box Number " + boxNumber + " is duplicated in the project");
                    }
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
            return "DuplicateBoxUpdater";
        }
    }
}
