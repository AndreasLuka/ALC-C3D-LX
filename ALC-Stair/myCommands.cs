using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;




// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_Stair.C3D.MyCommands))]

namespace ALC_Stair.C3D
{

    public class MyCommands
    {

        [CommandMethod("ALCGroup", "ALCCreateStair", "ALCCreateStairLocal", CommandFlags.Modal)]
        public void ALCStairCreate()
        {
            // Get the current document and database, and start a transaction
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            int    steps = 5;
            Double tread = 0.31;
            Double riser = 0.16;
            Double landing = 1.10;
            Double width = 2.0;
            Point3d ptEnd = new Point3d(0, 0, 0);
            Point3d ptStart = new Point3d(0,0,0);

            PromptPointResult pPointRes;

            PromptPointOptions pPointOpts = new PromptPointOptions("");
            pPointOpts.Message = "\nSpecify start point or ";

            // Define the valid keywords and allow Enter
            pPointOpts.Keywords.Add("Tread");
            pPointOpts.Keywords.Add("Riser");
            pPointOpts.Keywords.Add("Landing");
            pPointOpts.Keywords.Add("Width");
            pPointOpts.Keywords.Add("Steps");
            pPointOpts.AllowNone = true;

            // prompt for Steps
            PromptIntegerResult pIntRes;
            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
            // Restrict input to positive and non-negative values
            pIntOpts.AllowZero = false;
            pIntOpts.AllowNegative = false;

            //prompt for Tread, Riser, Landing, Width 
            PromptDoubleResult pDoubleRes;
            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("");

            int iPoint = 0; //no Startpoint , no Endpoint 

            do
            {
                // Get the value entered by the user
                pPointRes = doc.Editor.GetPoint(pPointOpts);

                // Exit if the user presses ESC or cancels the command
                if (pPointRes.Status == PromptStatus.Cancel) return;

                if (pPointRes.Status == PromptStatus.Keyword)
                {
                    // Handling keywords
                    switch (pPointRes.StringResult)
                    {
                        case "Tread":
                            pDoubleOpts.Message = "\nSpecify tread lenght [" + tread.ToString() + "]: ";
                            pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                            tread = pDoubleRes.Value;
                            break;
                        case "Riser":
                            pDoubleOpts.Message = "\nSpecify riser height [" + riser.ToString() + "]: ";
                            pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                            riser = pDoubleRes.Value;
                            break;
                        case "Landing":
                            pDoubleOpts.Message = "\nSpecify landing lenght [" + landing.ToString() + "]: ";
                            pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                            landing = pDoubleRes.Value;
                            break;
                        case "Width":
                            pDoubleOpts.Message = "\nSpecify width [" + width.ToString() + "]: ";
                            pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                            width = pDoubleRes.Value;
                            break;
                        case "Steps":
                            pIntOpts.Message = "\nSpecify number of steps [" + steps.ToString() + "]: ";
                            pIntRes = doc.Editor.GetInteger(pIntOpts);
                            steps = pIntRes.Value;
                            break;
                    }

                }
                else
                {
                    // Handling Points
                    switch (iPoint)
                    {
                        case 0:
                            // Prompt is for StartPoint

                            ptStart = pPointRes.Value;
                            iPoint++;

                            pPointOpts.Message = "\nSpecify end point: ";
                            pPointOpts.UseBasePoint = true;
                            pPointOpts.BasePoint = ptStart;

                            break;
                        case 1:
                            // Prompt is for EndPoint
                            ptEnd = pPointRes.Value;
                            iPoint++;
                            break;
                    }

                }
            }
            while (iPoint < 2);
            
            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // Open the Block table record for read
                    BlockTable blkTable;
                    blkTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord blkTblRec;
                    blkTblRec = tr.GetObject(blkTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Solid3d stair3d = myFunctions.CreateStair3D(steps, riser, tread, landing, width);
                    if (stair3d != null)
                    {

                        CoordinateSystem3d curUCS = doc.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;
                        Matrix3d matUCS =ed.CurrentUserCoordinateSystem;
                        Point3d pOrign = new Point3d(0, 0, 0);
                        Vector3d vStart = pOrign.GetVectorTo(ptStart);

                        Point2d start = new Point2d(ptStart.X, ptStart.Y);
                        Point2d end = new Point2d(ptEnd.X, ptEnd.Y);
 
                        stair3d.TransformBy(Matrix3d.Displacement(vStart));
                        stair3d.TransformBy(Matrix3d.Rotation(start.GetVectorTo(end).Angle, curUCS.Zaxis, ptStart));
                        stair3d.TransformBy(matUCS);

                        blkTblRec.AppendEntity(stair3d);
                        tr.AddNewlyCreatedDBObject(stair3d, true);

                        // Extension dictionary
                        DBObject dbObj = stair3d;
                        ObjectId extId = dbObj.ExtensionDictionary;

                        dbObj.UpgradeOpen();
                        dbObj.CreateExtensionDictionary();
                        extId = dbObj.ExtensionDictionary;

                        DBDictionary dbExt = (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);

                        dbExt.UpgradeOpen();

                        Xrecord xr = new Xrecord();
                        ResultBuffer rb = new ResultBuffer();
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataInteger32, steps));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, tread));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, riser));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, landing));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataReal, width));

                        xr.Data = rb;
                        dbExt.SetAt("StairCase", xr);
                        tr.AddNewlyCreatedDBObject(xr, true);

                        tr.Commit();
                    }
                    else
                    {
                        ed.WriteMessage("Error: Staircase creation failed");
                    }

                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.ToString());
            }

        }

         /*
                [CommandMethod("ALCStairJig")]
                public static void StairJig()
                {
                    Database db = HostApplicationServices.WorkingDatabase;

                    Solid3d stair = myFunctions.CreateStair3D (5, 0.15, 0.30, 1.1, 2.1);

                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        if (StairInsertRotating.Jig(stair))
                            tr.Commit();
                        else
                            tr.Abort();
                     }
                }

         */
        [CommandMethod("ALCGroup", "ALCGetStair", "ALCGetStairLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void GetStairValues()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityOptions peo = new PromptEntityOptions("Pick a 3DSOLID: ");
            peo.SetRejectMessage("\nA 3d solid must be selected.");
            peo.AddAllowedClass(typeof(Solid3d), true);

            PromptEntityResult per = ed.GetEntity(peo);

            if (per.Status != PromptStatus.OK) return;

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                ObjectId id = per.ObjectId;
 
                Solid3d stair = tr.GetObject(per.ObjectId, OpenMode.ForRead, false) as Solid3d;
                Boolean correct = myFunctions.IsStair(stair);
                if (correct)
                {
                    Point3d ptStart = myFunctions.GetStairStartPoint(stair);
                    double angle = myFunctions.GetStairAngle(stair);

                    ed.WriteMessage("\n Startpoint: {0} Angle {1}", ptStart.ToString(), angle.ToString());

                    int numstairs = myFunctions.GetStairNumber(stair);
                    double tread = myFunctions.GetStairTread(stair);
                    ed.WriteMessage("\n Numbers of Stairs: {0} Tread: {1}", numstairs.ToString(), tread.ToString());

                }

                tr.Commit();
            }
        }


 

    }
}




