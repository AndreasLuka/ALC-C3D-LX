using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALCJIG2.Civil3D.MyCommands))]

namespace ALCJIG2.Civil3D
{
    public class MyCommands
    {

        [CommandMethod("alcRenameAlignments")]
        public void alcRenAlgn()
        {

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CivilDocument civdoc = CivilApplication.ActiveDocument;

            ObjectIdCollection alignIds = civdoc.GetAlignmentIds();
            if (alignIds.Count == 0)
            {
                ed.WriteMessage("\n" + "Error: No alignments in this drawing");
                return;
            }
            List<BaseC3DObject> alignList = myFunctions.GetAlignments();
            if (alignList == null)
            {
                ed.WriteMessage("\n" + "Error: No alignments in this drawing");
                return;
            }

            List<BaseC3DObject> alignStyleList = myFunctions.GetAlignmentStyles();
            if (alignStyleList == null)
            {
                ed.WriteMessage("\n" + "Error: No alignment styles in this drawing");
                return;
            }
            myChangeStyleWindow win = new myChangeStyleWindow(alignList, alignStyleList);
            Boolean rtn = Application.ShowModalWindow(win).Value;
            if (rtn == false) return;

            using (DocumentLock doclck = Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                try
                {
                    using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            foreach (BaseC3DObject bco in win.Alignments)
                            {
                                if (bco.IsSelected)
                                {
                                    Alignment align = tr.GetObject(bco.Id, OpenMode.ForWrite) as Alignment;
                                    BaseC3DObject selectedAlignmentStyle = win.ComboBox_Style.SelectedItem as BaseC3DObject;
                                    align.StyleId = selectedAlignmentStyle.Id;
                                }
                            }
                            tr.Commit();
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

    }
}
