// (C) Copyright 2018 by  
//
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;



// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_SolidStair.MyCommands))]

namespace ALC_SolidStair
{

    public class MyCommands
    {
        [CommandMethod("ALCSolidStairCreate")]
        public void InsertRotateStair()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // prompt for Steps
            PromptIntegerResult pIntRes;
            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("")
            {
                // Restrict input to positive and non-negative values
                AllowZero = false,
                AllowNegative = false
            };

            //prompt for Tread, Riser, Landing, Width 
            PromptDoubleResult pDoubleRes;
            PromptDoubleOptions pDoubleOpts = new PromptDoubleOptions("")
            {
                AllowNegative = false
            };

            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {

                    // Create basic stair
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                    // Create the Jig and ask the user to place the Stair
                    StairJig jig = new StairJig(5, 0.16, 0.32, 1.1, 2.0);
                    PromptResult pr;

                    do
                    {
                        // Get the value entered by the user
                        pr = ed.Drag(jig);

                        // Exit if the user presses ESC or cancels the command
                        if (pr.Status == PromptStatus.Cancel) return;
                        if (pr.Status == PromptStatus.Keyword)
                        {
                            // Handling keywords
                            switch (pr.StringResult)
                            {
                                case "Steps":
                                    pIntOpts.Message = "\nSpecify number of steps [" + jig.steps.ToString() + "]: ";
                                    pIntRes = doc.Editor.GetInteger(pIntOpts);
                                    jig.steps = pIntRes.Value;
                                    break;
                                case "Riser":
                                    pDoubleOpts.Message = "\nSpecify riser height [" + jig.riser.ToString() + "]: ";
                                    pDoubleOpts.AllowZero = false;
                                    pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                                    jig.riser = pDoubleRes.Value;
                                    break;
                                case "Tread":
                                    pDoubleOpts.Message = "\nSpecify tread lenght [" + jig.tread.ToString() + "]: ";
                                    pDoubleOpts.AllowZero = false;
                                    pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                                    jig.tread = pDoubleRes.Value;
                                    break;
                                case "Landing":
                                    pDoubleOpts.Message = "\nSpecify landing lenght [" + jig.landing.ToString() + "]: ";
                                    pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                                    jig.landing = pDoubleRes.Value;
                                    break;
                                case "Width":
                                    pDoubleOpts.Message = "\nSpecify width [" + jig.width.ToString() + "]: ";
                                    pDoubleOpts.AllowZero = false;
                                    pDoubleRes = doc.Editor.GetDouble(pDoubleOpts);
                                    jig.width = pDoubleRes.Value;
                                    break;
                            }
                            jig.jigUpdate = true;
                        }
                        else if (pr.Status == PromptStatus.OK)
                        {
                            // Go to next phase
                            jig.jigStatus++;
                        }
                    }
                    while (jig.jigStatus < 2);


                    if (pr.Status == PromptStatus.OK)
                    {
                        Solid3d stair3d = myFunctions.CreateStair3D(jig.steps, jig.riser, jig.tread, jig.landing, jig.width);
                        if (stair3d != null)
                        {
                            // Moving and adding to staircase drawing database
                            stair3d.TransformBy(jig.Displacement);
                            stair3d.TransformBy(jig.Rotation);
                            BlockTableRecord curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                            curSpace.AppendEntity(stair3d);
                            tr.AddNewlyCreatedDBObject(stair3d, true);

                            // Extension dictionary
                            Autodesk.AutoCAD.DatabaseServices.DBObject dbObj = stair3d;
                            ObjectId extId = dbObj.ExtensionDictionary;

                            dbObj.UpgradeOpen();
                            dbObj.CreateExtensionDictionary();
                            extId = dbObj.ExtensionDictionary;

                            DBDictionary dbExt = (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);

                            dbExt.UpgradeOpen();

                            Xrecord xr = new Xrecord();
                            ResultBuffer rb = new ResultBuffer()
                            {
                                new TypedValue((int)DxfCode.ExtendedDataInteger32, jig.steps),
                                new TypedValue((int)DxfCode.ExtendedDataReal, jig.tread),
                                new TypedValue((int)DxfCode.ExtendedDataReal, jig.riser),
                                new TypedValue((int)DxfCode.ExtendedDataReal, jig.landing),
                                new TypedValue((int)DxfCode.ExtendedDataReal, jig.width)
                            };

                            xr.Data = rb;
                            dbExt.SetAt("StairCase", xr);
                            tr.AddNewlyCreatedDBObject(xr, true);

                        }
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


        [CommandMethod("ALCGroup", "ALCGetStair", CommandFlags.UsePickSet)]
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

                    int numstairs = myFunctions.GetStairSteps(stair);
                    double tread = myFunctions.GetStairTread(stair);
                    ed.WriteMessage("\n Numbers of Stairs: {0} Tread: {1}", numstairs.ToString(), tread.ToString());

                }
                tr.Commit();
            }
        }

    }

}
