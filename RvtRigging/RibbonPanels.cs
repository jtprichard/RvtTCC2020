using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Media.Imaging;
using System.Collections;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.Creation;

namespace RvtRigging
{
    public static class RibbonPanels
    //Ribbon Panel and Button Creation
    {
        //RIGGING RIBBON PANEL
        public static void RibbonPanelRigging(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Performance Rigging");

            BitmapImage genericButtonImage = new BitmapImage(new Uri("pack://application:,,,/RvtRigging;component/Resources/blank_button.png"));

            //BUTTON TO UPDATE DEVICE SCHEDULES
            PushButtonData button = new PushButtonData("Duplicate_Linesets_Button", "Duplicate\nLinesets", 
                path, "RvtRigging.CmdDuplicateLinesets")
            {
                ToolTip = "Duplicate Linesets on Specified Centers"
            };

            BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtRigging;component/Resources/StageDrape.png"));
            button.LargeImage = buttonImage;

            _ = panel.AddItem(button) as PushButton;


            //TEXT BOX DATA FOR LINESET SPACING
            TextBoxData lsSpacing = new TextBoxData("Lineset_Spacing");
            BitmapImage lsSpaceButImg = new BitmapImage(new Uri("pack://application:,,,/RvtRigging;component/Resources/Dim.png"));
            lsSpacing.Name = "Lineset_Spacing";
            lsSpacing.ToolTip = "Enter lineset spacing distance";
            lsSpacing.LongDescription = "Note that spacing must be entered in either full inches (Imperial" +
                " or in millimeteres (Metric).  Fractional inches will need to be adjusted manually in the model.";
            lsSpacing.Image = lsSpaceButImg;
            
            
            //TEXT BOX DATA FOR LINESET QTY
            TextBoxData lsQty = new TextBoxData("Lineset_Qty");
            BitmapImage lsQtyButImg = new BitmapImage(new Uri("pack://application:,,,/RvtRigging;component/Resources/Hash.png"));
            lsQty.Name = "Lineset_Qty";
            lsQty.ToolTip = "Enter lineset quantity to duplicate";
            lsQty.LongDescription = "Note that this is in addition to the one being duplicated";
            lsQty.Image = lsQtyButImg;
            
            
            //CREATE STACKED PANEL
            IList<RibbonItem> stackedItems = panel.AddStackedItems(lsSpacing, lsQty);
            TextBox tb1 = stackedItems[0] as TextBox;
            tb1.PromptText = "Spacing";
            tb1.Width = 70;
            tb1.ShowImageAsButton = true;
            tb1.EnterPressed += CallbackSpacingTextBox;

            TextBox tb2 = stackedItems[1] as TextBox;
            tb2.PromptText = "Qty";
            tb2.Width = 70;
            tb2.ShowImageAsButton = true;
            tb2.EnterPressed += CallbackQtyTextBox;

            //PIN LINESETS BUTTON
            PushButtonData pinLinesetButton = new PushButtonData("Pin_Linesets", "Pin\nLinesets",
                path, "RvtRigging.CmdPinLinesets");
            pinLinesetButton.LargeImage = genericButtonImage;

            _ = panel.AddItem(pinLinesetButton) as PushButton;

            //UNPIN LINESETS BUTTON
            PushButtonData unpinLinesetButton = new PushButtonData("Unpin_Linesets", "UnPin\nLinesets",
                path, "RvtRigging.CmdUnpinLinesets");
            unpinLinesetButton.LargeImage = genericButtonImage;

            _ = panel.AddItem(unpinLinesetButton) as PushButton;

            //RENUMBER LINESETS BUTTON
            PushButtonData renumberLinesetButton = new PushButtonData("Renumber_Linesets", "Renumber\nLinesets",
                path, "RvtRigging.CmdRenumberLinesets");
            renumberLinesetButton.LargeImage = genericButtonImage;

            _ = panel.AddItem(renumberLinesetButton) as PushButton;
        }
        public static void CallbackSpacingTextBox(object sender, TextBoxEnterPressedEventArgs args)
        {
            //Retrieve text box information
            TextBox textBox = sender as TextBox;

            Units units = args.Application.ActiveUIDocument.Document.GetUnits();
            UnitType uType = UnitType.UT_Length;

            if (UnitFormatUtils.TryParse(units, uType, textBox.Value.ToString(), out double dResult))
                textBox.Value = UnitFormatUtils.Format(units, uType, dResult, false, false);
            else
                textBox.Value = null;
        }

        public static void CallbackQtyTextBox(object sender, TextBoxEnterPressedEventArgs args)
        {
            //Retrieve text box information
            TextBox textBox = sender as TextBox;

            if (int.TryParse(textBox.Value.ToString(), out int result))
                if (result > 0)
                    textBox.Value = result.ToString();
                else
                    textBox.Value = null;
            else
                textBox.Value = null;
        }


    }

}
