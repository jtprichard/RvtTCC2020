using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RvtElectrical
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdRenumberBoxes: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;
            Document doc = uidoc.Document;

            if (doc.IsFamilyDocument)
            {
                TaskDialog.Show("Document Error", "Command only available in Model");
                return Result.Failed;
            }

            //GET THE CURRENT BOX NUMBER
            string rt = "TCC Technical";
            string rp = "Device Boxes";
            string riBoxNumStr = "Box_Number_Text";
            TextBox tb = null;
            int boxNumber = 0;

            if (RibbonUtils.GetRibbonItem(rt, rp, riBoxNumStr, uiapp, out RibbonItem riBoxNum))
            {
                tb = riBoxNum as TextBox;
                if (tb.Value == null)
                {
                    TaskDialog.Show("BOX NUMBER ERRROR", "Please provide a Box Number in the text box");
                    return Result.Failed;
                }

                else
                    boxNumber = int.Parse(tb.Value.ToString());
            }
            else
            {
                TaskDialog.Show("Box Number Error", "Provide a valid box number in the associated text box");
                return Result.Failed;
            }



            //PICK THE DEVICEBOX
            DeviceBox dbObject = null;

            //Define a reference Object to accept the pick result
            Reference pickedObj = null;

            //Set Ingnore Duplicates = false
            bool ignoreDuplicates = false;

            //Determine state of Duplicate Box Check parameter
            bool dupBoxCheckParam = ExternalApplication.Instance.DuplicateBoxOn;

            try
            {
                do
                {
                    try
                    {
                        //DETERMINE IF THE BOX NUMBER IS ALREADY BEING USED
                        //If so, offer to advance to next available.
                        //If not, ignore future duplicates
                        if (DeviceBox.GetDuplicateDeviceBoxes(doc, boxNumber).Count() > 0 && !ignoreDuplicates)
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
                            else
                            {
                                ignoreDuplicates = true;
                                //If Duplicate Check Toggle Button is enabled, disable it
                                if (dupBoxCheckParam)
                                {
                                    ExternalApplication.Instance.ToggleDuplicateBox(uiapp);
                                }
                            }

                        }

                        //Pick an object
                        Selection sel = uiapp.ActiveUIDocument.Selection;
                        SelectionFilterDeviceBox selFilter = new SelectionFilterDeviceBox();

                        pickedObj = sel.PickObject(ObjectType.Element, selFilter, "Select a Device Box");
                        Element elem = doc.GetElement(pickedObj);
                    }

                    //catch Escape from routine
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        //If box duplication was turned off, turn it back on
                        if (ExternalApplication.Instance.DuplicateBoxOn != dupBoxCheckParam)
                            ExternalApplication.Instance.ToggleDuplicateBox(uiapp);
                        return Result.Succeeded;
                    }

                    Element boxEle;
                    if (pickedObj != null)
                    {
                        // Retreive Element
                        ElementId eleId = pickedObj.ElementId;
                        Element ele = doc.GetElement(eleId) as Element;
                        ElementId typeId = ele.GetTypeId();
                        ElementType type = doc.GetElement(typeId) as ElementType;

                        if (DeviceBox.IsDeviceBox(ele, doc))
                        {
                            dbObject = new DeviceBox(doc, ele);
                            boxEle = ele;
                        }

                        else
                        {
                            TaskDialog.Show("Selection Error", "Please select a Device Box");
                            return Result.Failed;
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Selection Error", "Please select a Device Box");
                        return Result.Failed;
                    }

                    using (Transaction trans = new Transaction(doc, "Number Box"))
                    {
                        try
                        {
                            //ASSIGN BOX NUMBER TO DEVICEBOX
                            trans.Start();
                            Parameter boxIdParam = boxEle.get_Parameter(TCCElecSettings.BoxIdGuid);
                            boxIdParam.Set(boxNumber);

                            //ADVANCE TEXTBOX NUMBER
                            boxNumber++;
                            tb.Value = boxNumber;
                            trans.Commit();
                        }

                        catch (Exception ex)
                        {
                            trans.Dispose();
                            UIUtils.EBSGenException(ex);
                        }
                    }
                } while (1 == 1);

                return Result.Succeeded;
            }

            catch (Exception ex)
            {
                UIUtils.EBSGenException(ex);
                return Result.Failed;
            }

        }
    }
}
