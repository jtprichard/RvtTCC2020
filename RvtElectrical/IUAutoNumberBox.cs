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

    public class IUAutoNumberBox : IUpdater
    //UPdater that updates the concatenated label of device box circuits
    {

        static IUDuplicateBox _updater = null;
        
        // DMU updater to call class to update device box concat label to new entry

        static AddInId _appId;
        static UpdaterId _updaterId;

        public IUAutoNumberBox(AddInId id)
        {
            _appId = id;
            _updaterId = new UpdaterId(_appId, new Guid(
                "6B150DD6-2A32-4C91-853B-8CCECC6DAD36"));
        }

        public void Execute(UpdaterData data)
        {

            try
            {
                Document doc = data.GetDocument();
                Application app = doc.Application;
                UIApplication uiapp = ExternalApplication._uiApplicationAutoUpdate;

                IList<ElementId> eids = (IList<ElementId>)data.GetAddedElementIds();

                IList<Element> eles = new List<Element>();
                foreach (ElementId eid in eids)
                {
                    Element e = doc.GetElement(eid);
                    if (DeviceBox.IsDeviceBox(e, doc))
                        eles.Add(e);
                }

                Element boxEle = eles[0];
                DeviceBox deviceBox = new DeviceBox(doc, boxEle);
                int boxNumber = deviceBox.BoxId;

                //GET THE CURRENT BOX NUMBER
                string rt = "TCC Technical";
                string rp = "Device Boxes";
                string riBoxNumStr = "Box_Number_Text";
                TextBox tb = null;

                if (RibbonUtils.GetRibbonItem(rt, rp, riBoxNumStr, uiapp, out RibbonItem riBoxNum))
                {
                    tb = riBoxNum as TextBox;
                    if (tb.Value == null)
                    {
                        TaskDialog.Show("BOX NUMBER ERRROR", "Please provide a Box Number in the text box");
                        throw new Exception();
                    }

                    else
                        boxNumber = int.Parse(tb.Value.ToString());
                }
                else
                {
                    TaskDialog.Show("Box Number Error", "Provide a valid box number in the associated text box");
                    throw new Exception();
                }


                //DETERMINE IF THE BOX NUMBER IS ALREADY BEING USED
                //If so, offer to advance to next available.
                if (DeviceBox.GetDuplicateDeviceBoxes(doc, boxNumber).Count() > 0)
                {
                    string strMessage = string.Format("Box Number {0} is already in use.  Use next available number?" + Environment.NewLine +
                        "Clicking [No] will ignore future duplicates", boxNumber);
                    TaskDialogResult result = TaskDialog.Show("DUPLICATE DEVICE BOX NUMBER", strMessage,
                        TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

                    if (result == TaskDialogResult.Yes)
                    {
                        boxNumber = DeviceBox.NextDeviceBoxNumber(boxNumber, doc);
                        tb.Value = boxNumber;
                    }
                }

                //UPDATE THE BOX NUMBER
                try
                {
                    //ASSIGN BOX NUMBER TO DEVICEBOX
                    Parameter boxIdParam = boxEle.get_Parameter(TCCElecSettings.BoxIdGuid);
                    boxIdParam.Set(boxNumber);

                    //ADVANCE TEXTBOX NUMBER
                    boxNumber++;
                    tb.Value = boxNumber;
                }

                catch (Exception ex)
                {
                    //UIUtils.EBSGenException(ex);
                }
            }
            catch (Exception ex)
            {
                //UIUtils.EBSGenException(ex);
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
            return "AutoNumberBoxUpdater";
        }
    }
}
