using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Collections;
using Autodesk.Revit.UI.Events;

namespace RvtElectrical
{
    static class RibbonPanelsOld
    //Ribbon Panel and Button Creation
    {
        




        //PANEL SCHEDULE RIBBON PANEL
        public static void RibbonPanelPanelBoard(UIControlledApplication application, string path, string tabName)
        {
            //BUTTON SERIES TO UPDATE PANEL SCHEDULES
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Specialty PanelBoards");

            //Panel schedules split buttons
            PushButtonData sbButton1 = new PushButtonData("Update_Panel_Schedule_Button", "Update Current\nPanel Schedule",
                path, "RvtElectrical.CmdPBUpdateSchedule");
            sbButton1.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/electrical_panel.png"));
            sbButton1.ToolTip = "Update the current specialty panel schedule";

            PushButtonData sbButton2 = new PushButtonData("Update_PL_Panel_Schedule_Button", "Update All\nLighting Panels",
                path, "RvtElectrical.CmdPBUpdatePLSchedule");
            sbButton2.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/electrical_panel_light.png"));
            sbButton2.ToolTip = "Update all performance lighting panel schedules";

            PushButtonData sbButton3 = new PushButtonData("Update_PS_Panel_Schedule_Button", "Update All\nSound Panels",
                path, "RvtElectrical.CmdPBUpdatePSSchedule");
            sbButton3.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/electrical_panel_sound.png"));
            sbButton3.ToolTip = "Update all performance sound panel schedules";

            PushButtonData sbButton4 = new PushButtonData("Update_All_Panel_Schedule_Button", "Update All\nPanel Schedules",
                path, "RvtElectrical.CmdPBUpdateAllSchedule");
            sbButton4.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/electrical_panel_all.png"));
            sbButton4.ToolTip = "Update all specialty panel schedules";

            SplitButtonData sb1 = new SplitButtonData("sbPanel_Schedule_Update", "Panel Schedules");
            SplitButton sb = panel.AddItem(sb1) as SplitButton;
            sb.AddPushButton(sbButton1);
            sb.AddPushButton(sbButton2);
            sb.AddPushButton(sbButton3);
            sb.AddPushButton(sbButton4);

 
            //PANEL SCHEDULES UTILTIES
            //stacked buttons
            PushButtonData button1 = new PushButtonData("Fill_Spare_Panel_Schedule_Button", "Fill\nSpares", 
                path, "RvtElectrical.CmdPBFillSpare");
            button1.Image = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spare.png"));
            button1.ToolTip = "Automatically infill unused slots with spares";

            PushButtonData button2 = new PushButtonData("Remove_Spare_Panel_Schedule_Button", "Remove\nSpares", 
                path, "RvtElectrical.CmdPBRemoveSpare");
            button2.Image = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/removespare.png"));
            button2.ToolTip = "Automatically remove spares from slots";

            PushButtonData button3 = new PushButtonData("Remove_Circuits_Button", "Remove\nCircuits",
                path, "RvtElectrical.CmdPBRemoveCircuits");
            button3.Image = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/remove_circuit.png"));
            button3.ToolTip = "Automatically remove spares from slots";

            // add 3 stacked buttons to the panel
            panel.AddStackedItems(button1, button2, button3);




            //OLD FOR REFERENCE

            //PulldownButton pullDownButton = panel.AddItem(new PulldownButtonData("Update_Panel_Schedules", "Update\nPanel Schedules")) as PulldownButton;
            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/electrical_panel.png"));
            //pullDownButton.LargeImage = buttonImage;

            //PushButton pdButton1 = pullDownButton.AddPushButton(new PushButtonData("Update_Panel_Schedule_Button", "Update\nPanel Schedule",
            //    path, "RvtElectrical.PBUpdateSchedule"));
            //pdButton1.LargeImage = buttonImage;
            //pdButton1.ToolTip = "Update the specialty panel schedule";

            //PushButton pdButton2 = pullDownButton.AddPushButton(new PushButtonData("Update_PL_Panel_Schedule_Button", "Update Lighting\nPanel Schedules",
            //    path, "RvtElectrical.PBUpdatePLSchedule"));
            //pdButton2.LargeImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/lighting_panel.png"));
            //pdButton1.ToolTip = "Update all performance lighting panel schedules";

            //PushButton pdButton3 = pullDownButton.AddPushButton(new PushButtonData("Update_PS_Panel_Schedule_Button", "Update Sound\nPanel Schedules",
            //    path, "RvtElectrical.PBUpdatePSSchedule"));
            //pdButton1.ToolTip = "Update all performance sound panel schedules";

            //PushButton pdButton4 = pullDownButton.AddPushButton(new PushButtonData("Update_All_Panel_Schedule_Button", "Update All\nPanel Schedules",
            //    path, "RvtElectrical.PBUpdateAllSchedule"));
            //pdButton1.ToolTip = "Update all specialty panel schedules";


            //PushButtonData button = new PushButtonData("Update_Panel_Schedule_Button", "Update\nPanel Schedule", path, "RvtElectrical.PBUpdateSchedule")
            //{

            //};


            //button.LargeImage = buttonImage;

            //panel.AddItem(button);

        }
        //CONDUIT
        public static void RibbonPanelConduit(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Conduit");

            //PLACE CONDUIT BUTTON
            PushButtonData button = new PushButtonData("Place_Conduit_Button", "Place\nConduit",
                path, "RvtElectrical.CmdCreateConduit")
            {
                ToolTip = "Place Specialty Conduit"
            };

            BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/conduit.png"));
            button.LargeImage = buttonImage;

            _ = panel.AddItem(button) as PushButton;

            //CONDUIT INFORMATION COMBOBOXES
            ComboBoxData cbLevel = new ComboBoxData("Level");
            //Autodesk.Revit.UI.ComboBox combo1 = panel.AddItem(cbLevel) as Autodesk.Revit.UI.ComboBox;

            ComboBoxData cbSize = new ComboBoxData("Size");
            //Autodesk.Revit.UI.ComboBox combo2 = panel.AddItem(cbSize) as Autodesk.Revit.UI.ComboBox;

            TextBoxData tbDestination = new TextBoxData("Destination");
            //BitmapImage lsSpaceButImg = new BitmapImage(new Uri("pack://application:,,,/RvtRigging;component/Resources/Dim.png"));

            //CREATE STACKED PANEL
            IList<RibbonItem> stackedItems = panel.AddStackedItems(cbLevel, cbSize, tbDestination);
            ComboBox combo1 = stackedItems[0] as ComboBox;
            ComboBox combo2 = stackedItems[1] as ComboBox;
            TextBox tb3 = stackedItems[2] as TextBox;


            ComboBoxMemberData cb1MemData1 = new ComboBoxMemberData("L", "Low");
            ComboBoxMember cb1Mem1 = combo1.AddItem(cb1MemData1);
            //cb1Mem1.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item1_16x16.bmp");

            ComboBoxMemberData cb1MemData2 = new ComboBoxMemberData("M", "Medium");
            ComboBoxMember cb1Mem2 = combo1.AddItem(cb1MemData2);
            //cb1Mem2.Image = new BitmapImage(new Uri(Path.Combine(assemPath, "Standalone_Item1_16x16.bmp"), UriKind.Absolute));

            ComboBoxMemberData cb1MemData3 = new ComboBoxMemberData("H", "High");
            ComboBoxMember cb1Mem3 = combo1.AddItem(cb1MemData3);
            //cb1Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb1MemData4 = new ComboBoxMemberData("O", "Other");
            ComboBoxMember cb1Mem4 = combo1.AddItem(cb1MemData4);
            //cb1Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb1MemData5 = new ComboBoxMemberData("X", "Line Voltage");
            ComboBoxMember cb1Mem0 = combo1.AddItem(cb1MemData5);
            //cb1Mem1.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item1_16x16.bmp");



            ComboBoxMemberData cb2MemData1 = new ComboBoxMemberData("B1", "1 @ 3/4\"");
            ComboBoxMember cb2Mem1 = combo2.AddItem(cb2MemData1);
            //cb2Mem1.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item1_16x16.bmp");

            ComboBoxMemberData cb2MemData2 = new ComboBoxMemberData("C1", "1 @ 1\"");
            ComboBoxMember cb2Mem2 = combo2.AddItem(cb2MemData2);
            //cb2Mem2.Image = new BitmapImage(new Uri(Path.Combine(assemPath, "Standalone_Item1_16x16.bmp"), UriKind.Absolute));

            ComboBoxMemberData cb2MemData3 = new ComboBoxMemberData("D1", "1 @ 1.25\"");
            ComboBoxMember cb2Mem3 = combo2.AddItem(cb2MemData3);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData4 = new ComboBoxMemberData("E1", "1 @ 1.5\"");
            ComboBoxMember cb2Mem4 = combo2.AddItem(cb2MemData4);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData5 = new ComboBoxMemberData("F1", "1 @ 2\"");
            ComboBoxMember cb2Mem5 = combo2.AddItem(cb2MemData5);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData6 = new ComboBoxMemberData("G1", "1 @ 4\"");
            ComboBoxMember cb2Mem6 = combo2.AddItem(cb2MemData6);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData11 = new ComboBoxMemberData("B2", "2 @ 3/4\"");
            ComboBoxMember cb2Mem11 = combo2.AddItem(cb2MemData11);
            //cb2Mem1.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item1_16x16.bmp");

            ComboBoxMemberData cb2MemData12 = new ComboBoxMemberData("C2", "2 @ 1\"");
            ComboBoxMember cb2Mem12 = combo2.AddItem(cb2MemData12);
            //cb2Mem2.Image = new BitmapImage(new Uri(Path.Combine(assemPath, "Standalone_Item1_16x16.bmp"), UriKind.Absolute));

            ComboBoxMemberData cb2MemData13 = new ComboBoxMemberData("D2", "2 @ 1.25\"");
            ComboBoxMember cb2Mem13 = combo2.AddItem(cb2MemData13);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData14 = new ComboBoxMemberData("E2", "2 @ 1.5\"");
            ComboBoxMember cb2Mem14 = combo2.AddItem(cb2MemData14);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData15 = new ComboBoxMemberData("F2", "2 @ 2\"");
            ComboBoxMember cb2Mem15 = combo2.AddItem(cb2MemData15);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData16 = new ComboBoxMemberData("G2", "2 @ 4\"");
            ComboBoxMember cb2Mem16 = combo2.AddItem(cb2MemData16);
            //cb2Mem3.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item2_16x16.bmp");

            ComboBoxMemberData cb2MemData51 = new ComboBoxMemberData("AR", "As Required");
            ComboBoxMember cb2Mem0 = combo2.AddItem(cb2MemData51);
            //cb2Mem1.Image = BmpImageSource("RevitAddinCSProject.Resources.Embedded_Item1_16x16.bmp");


            tbDestination.Name = "Destination";
            tbDestination.ToolTip = "Enter default conduit destination";
            tbDestination.LongDescription = "Destination should be noted as a Box Number or Rack Location";
            //lsSpacing.Image = lsSpaceButImg;
            tb3.PromptText = "Destination";
            //tb3.Width = 70;
            //tb3.ShowImageAsButton = true;

        }




        //FACEPLATES
        public static void RibbonPanelFaceplates(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Faceplates");

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/spreadsheet.png"));
            //button.LargeImage = buttonImage;

            //_ = panel.AddItem(button) as PushButton;
        }
        
        //SIGNAL RISER
        public static void RibbonPanelSignalRiser(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Signal Riser");

            ////BUTTON
            //PushButtonData button = new PushButtonData("", "",
            //    path, "")
            //{
            //    ToolTip = ""
            //};

            //BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/IMAGE.png"));
            //button.LargeImage = buttonImage;

            //_ = panel.AddItem(button) as PushButton;
        }

        //FACEPLATES
        public static void RibbonPanelFamilyEditor(UIControlledApplication application, string path, string tabName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Family Editor");

            PushButtonData button = new PushButtonData("Associate_FP_Parameter_Button", "Finalize\nDevice",
                path, "RvtElectrical.CmdFPParamAssoc")
            {
                ToolTip = "Associate parameters of connectors to faceplate family and provide plate & family codes"
            };

            BitmapImage buttonImage = new BitmapImage(new Uri("pack://application:,,,/RvtElectrical;component/Resources/Faceplate New.png"));
            button.LargeImage = buttonImage;

            _ = panel.AddItem(button) as PushButton;
        }
    }

}
