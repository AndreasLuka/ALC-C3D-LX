// (C) Copyright 2018 by  
//
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;



// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(LX_SolidStair.MyCommands))]

namespace LX_SolidStair
{
    
    public class MyCommands
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = Application.DocumentManager.MdiActiveDocument.Database;
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        [CommandMethod("LXStairSolidAdd", CommandFlags.Modal)]
        public void CreateSolidStair()
        {

            PromptResult pr;

            // Get default values
            BaseStairObject bsoStandard = MyFunctions.GetPropertySetDefinitionStairStandardValues();

            List<LayerObject> lstLayers = MyFunctions.GetLayerList();

            winCreateStair win = new winCreateStair (lstLayers, bsoStandard);
            Boolean rtn = Application.ShowModalWindow(win).Value;
            if (rtn == false) return;

            LayerObject selectedLayer = win.CB_Layers.SelectedItem as LayerObject;

            try
            {
                BaseStairObject objStair = new BaseStairObject
                {
                    Id = ObjectId.Null,
                    Name = win.TB_Name.Text,
                    Steps = Convert.ToInt32(win.TB_Steps.Text),
                    Tread = Convert.ToDouble(win.TB_Tread.Text),
                    Riser = Convert.ToDouble(win.TB_Riser.Text),
                    Landing = Convert.ToDouble(win.TB_Landing.Text),
                    Width = Convert.ToDouble(win.TB_Width.Text),
                    Slope = Convert.ToDouble(win.TB_Slope.Text)
                };
                MyJig2 jig = new MyJig2(objStair.Steps, objStair.Riser, objStair.Tread, objStair.Landing, objStair.Width, objStair.Slope);

                do
                {
                    // Get the value entered by the user
                    pr = ed.Drag(jig);

                    // Exit if the user presses ESC or cancels the command
                    if (pr.Status == PromptStatus.Cancel) return;

                    if (pr.Status == PromptStatus.OK)
                    {
                        // Go to next phase
                        jig.jigStatus++;
                    }
                }
                while (jig.jigStatus < 2);

                if (pr.Status == PromptStatus.OK)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                            Solid3d stair3d = MyFunctions.CreateStair3D(jig.steps, jig.riser, jig.tread, jig.landing, jig.width, jig.slope);
                            if (stair3d != null)
                            {
                                // Moving and adding to staircase drawing database
                                stair3d.TransformBy(jig.matDisplacement);
                                stair3d.TransformBy(jig.matRotation);
                                //update layer
                                stair3d.LayerId = selectedLayer.Id;

                                BlockTableRecord curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                                curSpace.AppendEntity(stair3d);
                                tr.AddNewlyCreatedDBObject(stair3d, true);

                                // Adding ParameterSet to Staircase
                                objStair.Id = stair3d.Id;
                                bool isCreated = MyFunctions.SetStairPropertiesToSolid(stair3d, objStair);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ed.WriteMessage(ex.ToString());
                        }
                        tr.Commit();
                    }
                }
            }
            catch
            {
                ed.WriteMessage("\n Something wrong");
            }
        }


        [CommandMethod("LXStairSolidModify", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void ModifySolidStair()
        {
            ObjectId entId = MyFunctions.SelectStair();
            Matrix3d matDisplacement, matRotation;

            Solid3d stair3d;
            if (entId != ObjectId.Null)
            {
                // we have a valid id for a stair with attached properties
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // getting stair geometry and parameters
                    stair3d = tr.GetObject(entId, OpenMode.ForRead) as Solid3d;

                    // getting stair position and rotation
                    matDisplacement = MyFunctions.GetStairDisplacment(stair3d);
                    matRotation = MyFunctions.GetStairRotation(stair3d);

                    tr.Commit();
                };

                List<LayerObject> lstLayers = MyFunctions.GetLayerList(stair3d.LayerId);

                BaseStairObject bso = MyFunctions.GetStairPropertiesFromSolid(stair3d);

                Point3d pInsert = MyFunctions.GetStairInsertPoint(stair3d);
                Double angle = MyFunctions.GetStairRotationAngle(stair3d);


                SolidStairObject sso = new SolidStairObject
                {
                    Id = bso.Id,
                    Name = bso.Name,
                    Steps = bso.Steps,
                    Riser = bso.Riser,
                    Tread = bso.Tread,
                    Landing = bso.Landing,
                    Width = bso.Width,
                    Slope = bso.Slope,
                    X = pInsert.X,
                    Y = pInsert.Y,
                    Elevation = pInsert.Z,
                    Rotation = angle/Math.PI*180
                };

                winModifyStairSolid win = new winModifyStairSolid(lstLayers, sso);
                Boolean rtn = Application.ShowModalWindow(win).Value;
                if (rtn == false) return;

                LayerObject selectedLayer = win.CB_Layers.SelectedItem as LayerObject;


                try
                {
                    SolidStairObject retObjStair = new SolidStairObject
                    {
                        Id = ObjectId.Null,
                        Name = win.TB_Name.Text,
                        Steps = Convert.ToInt32(win.TB_Steps.Text),
                        Tread = Convert.ToDouble(win.TB_Tread.Text),
                        Riser = Convert.ToDouble(win.TB_Riser.Text),
                        Landing = Convert.ToDouble(win.TB_Landing.Text),
                        Width = Convert.ToDouble(win.TB_Width.Text),
                        Slope = Convert.ToDouble(win.TB_Slope.Text),
                        X = Convert.ToDouble(win.TB_X.Text),
                        Y = Convert.ToDouble(win.TB_Y.Text),
                        Elevation = Convert.ToDouble(win.TB_E.Text),
                        Rotation = Convert.ToDouble(win.TB_R.Text) / 180 * Math.PI
                    };


                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            stair3d = tr.GetObject(entId, OpenMode.ForWrite) as Solid3d;

                            stair3d.RecordHistory = true;

                            Polyline pline = MyFunctions.CreateStairPolyline(retObjStair.Steps, retObjStair.Riser, retObjStair.Tread, retObjStair.Landing, retObjStair.Slope);
                            Polyline path = MyFunctions.CreateStairSweepPath(retObjStair.Width);

                            stair3d.CreateSweptSolid(pline, path, MyFunctions.CreateSweepOptions(path).ToSweepOptions());

                            //update matrix
                            Point3d pNewInsert = new Point3d(retObjStair.X, retObjStair.Y, retObjStair.Elevation);

                            Vector3d disp = Point3d.Origin.GetVectorTo(pNewInsert);
                            matDisplacement = Matrix3d.Displacement(disp);
                            stair3d.TransformBy(matDisplacement);

                            matRotation = Matrix3d.Rotation(retObjStair.Rotation, Vector3d.ZAxis, pNewInsert);
                            stair3d.TransformBy(matRotation);

                            retObjStair.Id = stair3d.Id;

                            //update layer
                            stair3d.LayerId = selectedLayer.Id;

                            // set property data
                            Boolean isCreated = MyFunctions.SetStairPropertiesToSolid(stair3d, retObjStair);
                        }
                        catch (System.Exception ex)
                        {
                            ed.WriteMessage(ex.ToString());
                        }
                        tr.Commit();
                    }
                }
                catch 
                {
                    ed.WriteMessage("\n Something wrong");
                }
            };
        }


        [CommandMethod("LXStairList")]
        public void ListStair()
        {
            List<BaseStairObject> stairList = MyFunctions.GetStairList();

            if (stairList == null)
            {
                ed.WriteMessage("\n" + "Error: No stairs in this drawing");
                return;
            }
            else
            {
                winListStairs win = new winListStairs(stairList);
                Boolean rtn = Application.ShowModalWindow(win).Value;
                return;
            }
        }

        //[CommandMethod("LXStairStandardList")]
        //public void ListStandardStair()
        //{
        //    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        //    BaseStairObject bso = MyFunctions.GetPropertySetDefinitionStairStandardValues();

        //    ed.WriteMessage("\nTread: {0}", bso.Tread);
        //    ed.WriteMessage("\nRiser: {0}", bso.Riser);
        //    ed.WriteMessage("\nLanding: {0}", bso.Landing);
        //    ed.WriteMessage("\nWidth: {0}", bso.Width);
        //    ed.WriteMessage("\nSteps: {0}", bso.Steps);
        //    ed.WriteMessage("\nNames: {0}", bso.Name); 
        //}

        

        //[CommandMethod("LXStairPropertSetAdd")]
        //public void CreateStairPropertySetDefinition()
        //{
        //    ObjectId psdId = MyFunctions.GetPropertySetDefinitionIdByName(MyPlugin.psdName);
        //    if (psdId == ObjectId.Null)
        //    {
        //        MyFunctions.CreateStairPropertySetDefinition(MyPlugin.psdName);
        //        ed.WriteMessage("\n Property set defenition {0} created", MyPlugin.psdName);
        //    }
        //    else
        //    {
        //        ed.WriteMessage("\n Property set defenition {0} alreday exist, no property set definition created", MyPlugin.psdName);
        //    }
        //}




    }
}
