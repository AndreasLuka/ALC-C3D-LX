// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_PolyLine.MyCommands))]

namespace ALC_PolyLine
{


    public class MyCommands
    {


        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "ALC-Polyline", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            CivilDocument civdoc = CivilApplication.ActiveDocument;

            ObjectId alignId = SelectAlignment();

            if (alignId == ObjectId.Null) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    // get the alignment from alignId
                    Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);
                    ObjectId plId = align.GetPolyline();
                    // Polyline pl = tr.GetObject(plId, OpenMode.ForRead) as Polyline;
                    // ed.WriteMessage(pl.ToString());
                }
                catch { }
                tr.Commit();
            }
        }



        public static ObjectId SelectAlignment()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId result = ObjectId.Null;

            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect alignment: ");
            entOpts.SetRejectMessage("...not an Alignment, try again!:");
            entOpts.AddAllowedClass(typeof(Alignment), true);

            PromptEntityResult entRes = ed.GetEntity(entOpts);

            if (entRes.Status == PromptStatus.OK) result = entRes.ObjectId;

            return result;
        }

    }

}
