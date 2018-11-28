using System;
using System.Collections.Generic;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using Autodesk.Aec.PropertyData.DatabaseServices;



namespace LX_SolidStair
{
    class MyFunctions
    {
        // Property Set Definitions 
        public static ObjectId GetPropertySetDefinitionIdByName(string psdName)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ObjectId psdId = ObjectId.Null;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;

            using (Transaction tr = tm.StartTransaction())
            {
                try
                {
                    DictionaryPropertySetDefinitions psdDict = new DictionaryPropertySetDefinitions(db);
                    if (psdDict.Has(psdName, tr))
                    {
                        psdId = psdDict.GetAt(psdName);
                    }
                }
                catch
                {
                    ed.WriteMessage("\n GetPropertySetDefinitionIdByName failed");
                }
                tr.Commit();
                return psdId;
            }
        }


        public static ObjectId CreateStairPropertySetDefinition(string psdName)
        {
            ObjectId psdId;

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;

            DictionaryPropertySetDefinitions dict = new DictionaryPropertySetDefinitions(db);


            // Check for existing propert set definition ... If so just return its ObjectId.
            psdId = GetPropertySetDefinitionIdByName(psdName);

            if (psdId != ObjectId.Null)
            {
                return psdId;
                // check version and correctness not implemented
            }
            else
            {
                // Create the new property set definition;
                PropertySetDefinition psd = new PropertySetDefinition();
                psd.SetToStandard(db);
                psd.SubSetDatabaseDefaults(db);
                psd.AlternateName = psdName;
                psd.IsLocked = true;
                psd.IsVisible = false;
                psd.IsWriteable = true;

                // Setup an array of objects that this property set definition will apply to
                System.Collections.Specialized.StringCollection appliesto = new System.Collections.Specialized.StringCollection
                {
                    "AcDb3dSolid"
                };
                psd.SetAppliesToFilter(appliesto, false);

                // Add the property set definition to the dictionary to make formula property work correctly
                using (Transaction tr = tm.StartTransaction())
                {
                    dict.AddNewRecord(psdName, psd);
                    tr.AddNewlyCreatedDBObject(psd, true);

                    // Invisible Properties (managed by app)
                    PropertyDefinition def;

                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "_tread";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Real;
                    def.DefaultData = 0.32;
                    def.IsVisible = false;
                    psd.Definitions.Add(def);

                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "_riser";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Real;
                    def.DefaultData = 0.15;
                    def.IsVisible = false;
                    psd.Definitions.Add(def);

                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "_landing";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Real;
                    def.DefaultData = 1.1;
                    def.IsVisible = false;
                    psd.Definitions.Add(def);

                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "_width";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Real;
                    def.DefaultData = 2.00;
                    def.IsVisible = false;
                    psd.Definitions.Add(def);


                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "_slope";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Real;
                    def.DefaultData = 0.02;
                    def.IsVisible = false;
                    psd.Definitions.Add(def);

                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "_steps";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Integer;
                    def.DefaultData = 5;
                    def.IsVisible = false;
                    psd.Definitions.Add(def);

                    // Visable properties (exposed to user)

                    def = new PropertyDefinition();
                    def.SetToStandard(db);
                    def.SubSetDatabaseDefaults(db);
                    def.Name = "name";
                    def.DataType = Autodesk.Aec.PropertyData.DataType.Text;
                    def.DefaultData = "Stair - ";
                    def.IsVisible = true;
                    psd.Definitions.Add(def);

                    // Visable read only properties (exposed to user)

                    PropertyDefinitionFormula formula;

                    // Property definition need to be added to the property set definition before setting formula string to the formula property

                    // steps
                    formula = new PropertyDefinitionFormula();
                    formula.SetToStandard(db);
                    formula.SubSetDatabaseDefaults(db);
                    formula.Name = "steps";
                    psd.Definitions.Add(formula);
                    formula.SetFormulaString("[_steps]");

                    // riser
                    formula = new PropertyDefinitionFormula();
                    formula.SetToStandard(db);
                    formula.SubSetDatabaseDefaults(db);
                    formula.Name = "riser";
                    psd.Definitions.Add(formula);
                    formula.SetFormulaString("[_riser]");

                    // tread
                    formula = new PropertyDefinitionFormula();
                    formula.SetToStandard(db);
                    formula.SubSetDatabaseDefaults(db);
                    formula.Name = "tread";
                    psd.Definitions.Add(formula);
                    formula.SetFormulaString("[_tread]");

                    // landing
                    formula = new PropertyDefinitionFormula();
                    formula.SetToStandard(db);
                    formula.SubSetDatabaseDefaults(db);
                    formula.Name = "landing";
                    psd.Definitions.Add(formula);
                    formula.SetFormulaString("[_landing]");

                    // width
                    formula = new PropertyDefinitionFormula();
                    formula.SetToStandard(db);
                    formula.SubSetDatabaseDefaults(db);
                    formula.Name = "width";
                    psd.Definitions.Add(formula);
                    formula.SetFormulaString("[_width]");

                    // slope
                    formula = new PropertyDefinitionFormula();
                    formula.SetToStandard(db);
                    formula.SubSetDatabaseDefaults(db);
                    formula.Name = "slope";
                    psd.Definitions.Add(formula);
                    formula.SetFormulaString("[_slope]");

                    tr.Commit();

                    psdId = psd.ObjectId;
                    return psdId;
                }
            }
        }


        public static BaseStairObject GetPropertySetDefinitionStairStandardValues()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            BaseStairObject retBso = new BaseStairObject();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DictionaryPropertySetDefinitions psdDict = new DictionaryPropertySetDefinitions(db);

                    // Create property set definition if not existing
                    if (!psdDict.Has(MyPlugin.psdName, tr))
                    {
                        CreateStairPropertySetDefinition(MyPlugin.psdName);
                        ed.WriteMessage("\n Property set defenition {0} created", MyPlugin.psdName);
                    }

                    PropertySetDefinition psd = (PropertySetDefinition)tr.GetObject(psdDict.GetAt(MyPlugin.psdName), OpenMode.ForRead);

                    // Get the standard value from the properties in the property set defenition
                    BaseStairObject bso = new BaseStairObject
                        {
                            Id = ObjectId.Null,
                            Name = Convert.ToString(psd.Definitions[psd.Definitions.IndexOf("name")].DefaultData),
                            Steps = Convert.ToInt32(psd.Definitions[psd.Definitions.IndexOf("_steps")].DefaultData),
                            Tread = Convert.ToDouble(psd.Definitions[psd.Definitions.IndexOf("_tread")].DefaultData),
                            Riser = Convert.ToDouble(psd.Definitions[psd.Definitions.IndexOf("_riser")].DefaultData),
                            Landing = Convert.ToDouble(psd.Definitions[psd.Definitions.IndexOf("_landing")].DefaultData),
                            Width = Convert.ToDouble(psd.Definitions[psd.Definitions.IndexOf("_width")].DefaultData),
                            Slope = Convert.ToDouble(psd.Definitions[psd.Definitions.IndexOf("_slope")].DefaultData)
                        };
                        retBso = bso;

                }
                catch
                {
                    ed.WriteMessage("\n GetPropertySetDefinitionIdByName failed");
                }
                tr.Commit();
            }
            return retBso;
        }


        public static bool AddStairPropertySetToSolid(Solid3d sol)
        {
            bool result = false;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DictionaryPropertySetDefinitions psdDict = new DictionaryPropertySetDefinitions(db);

                    // Create property set definition if not existing
                    if (!psdDict.Has(MyPlugin.psdName, tr))
                    {
                        CreateStairPropertySetDefinition(MyPlugin.psdName);
                        ed.WriteMessage("\n Property set defenition {0} created", MyPlugin.psdName);
                    }

                    ObjectId psdId = GetPropertySetDefinitionIdByName(MyPlugin.psdName);
                    DBObject dbobj = tr.GetObject(sol.Id, OpenMode.ForWrite);
                    PropertyDataServices.AddPropertySet(dbobj, psdId);
                    result = true;
                }
                catch
                {
                    result = false;
                    ed.WriteMessage("\n AddStairPropertySetToSolid function failed");
                }
                tr.Commit();
                return result;
            }
        }


        public static bool IsStairPropertySetOnSolid(Solid3d sol)
        {
            bool result = false;

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(sol);
                if (setIds.Count > 0)
                {
                    foreach (ObjectId id in setIds)
                    {
                        PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForRead);
                        if (pset.PropertySetDefinitionName == MyPlugin.psdName)
                        {
                            result = true;
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }


        public static BaseStairObject GetStairPropertiesFromSolid (Solid3d sol)
        {
            BaseStairObject retBso = null;

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(sol);
                if (setIds.Count > 0)
                {
                    foreach (ObjectId id in setIds)
                    {
                        PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForRead);

                        if (pset.PropertySetDefinitionName == MyPlugin.psdName)
                        {
                            // Get the ObjectID of the property set definition by name
                            // Get the value of the property definition
                            BaseStairObject bso = new BaseStairObject
                            {
                                Id = sol.Id,
                                Name = Convert.ToString(pset.GetAt(pset.PropertyNameToId("name"))),
                                Steps = Convert.ToInt32(pset.GetAt(pset.PropertyNameToId("_steps"))),
                                Tread = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("_tread"))),
                                Riser = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("_riser"))),
                                Landing = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("_landing"))),
                                Width = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("_width"))),
                                Slope = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("_slope")))
                            };
                            retBso = bso;
                        }
                    }
                }
                tr.Commit();
            }
            return retBso;
        }


        public static bool SetStairPropertiesToSolid(Solid3d sol, BaseStairObject bso)
        {
            bool result = false;

            if (!IsStairPropertySetOnSolid(sol)) AddStairPropertySetToSolid(sol);

            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(sol);
                if (setIds.Count > 0)
                {
                    foreach (ObjectId id in setIds)
                    {
                        PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForWrite);
                        if (pset.PropertySetDefinitionName == MyPlugin.psdName && pset.IsWriteEnabled)
                        {
                            // Get the ObjectID of the property set definition by name
                            // Get the value of the property definition
                            pset.SetAt(pset.PropertyNameToId("name"), bso.Name);
                            pset.SetAt(pset.PropertyNameToId("_tread"), bso.Tread);
                            pset.SetAt(pset.PropertyNameToId("_riser"), bso.Riser);
                            pset.SetAt(pset.PropertyNameToId("_landing"), bso.Landing);
                            pset.SetAt(pset.PropertyNameToId("_width"), bso.Width);
                            pset.SetAt(pset.PropertyNameToId("_slope"), bso.Slope);
                            pset.SetAt(pset.PropertyNameToId("_steps"), bso.Steps);
                            result = true;
                            break;
                        }
                    }
                }
                tr.Commit();
            }
            return result;
        }


        //Solid Stair 
        public static Polyline CreateStairPolyline(int steps, double riser, double tread, double landing, double slope)
        {
            Polyline pSection = new Polyline();

            pSection.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);

            int iVertex = 1;
            for (int i = 1; i <= steps; i++)
            {
                pSection.AddVertexAt(iVertex++, new Point2d((i - 1) * tread, i * riser), 0, 0, 0);
                pSection.AddVertexAt(iVertex++, new Point2d(i * tread, i * riser), 0, 0, 0);
            };

            pSection.AddVertexAt(iVertex++, new Point2d(steps * tread, steps * riser), 0, 0, 0);

            if (landing > 0)
            {
                pSection.AddVertexAt(iVertex++, new Point2d(steps * tread + landing, steps * riser), 0, 0, 0);
                pSection.AddVertexAt(iVertex++, new Point2d(steps * tread + landing, (steps - 1) * riser), 0, 0, 0);
            }

            pSection.AddVertexAt(iVertex++, new Point2d((steps * tread), (steps - 1) * riser), 0, 0, 0);

            pSection.AddVertexAt(iVertex++, new Point2d(tread, 0), 0, 0, 0);
            pSection.Closed = true;

            // rotate polyline around x-Axis and z-Axis for slope
            Matrix3d matrix = Matrix3d.Rotation(Math.PI / 2, Vector3d.XAxis, Point3d.Origin) * Matrix3d.Rotation(slope, Vector3d.ZAxis, Point3d.Origin);
            pSection.TransformBy(matrix);

            return pSection;
        }


        public static Polyline CreateStairSweepPath(double width)
        {
            Polyline pline = new Polyline();
            pline.AddVertexAt(0, new Point2d(0, -width / 2), 0, 0, 0);
            pline.AddVertexAt(1, new Point2d(0, width / 2), 0, 0, 0);
            return pline;
        }


        public static SweepOptionsBuilder CreateSweepOptions(Polyline pline)
        {
            SweepOptionsBuilder sob = new SweepOptionsBuilder
            {
                Align = SweepOptionsAlignOption.AlignSweepEntityToPath,
                BasePoint = pline.StartPoint,
                Bank = true
            };
            return sob;
        }


        public static Solid3d CreateStair3D(int steps, double riser, double tread, double landing, double width, double slope)
        {
            Solid3d stair3d = new Solid3d
            {
                RecordHistory = true
            };

            Polyline pline = CreateStairPolyline(steps, riser, tread, landing, slope);
            Polyline path = MyFunctions.CreateStairSweepPath(width);

            stair3d.CreateSweptSolid(pline, path, MyFunctions.CreateSweepOptions(path).ToSweepOptions());

            return stair3d;
        }


        public static Matrix3d GetStairDisplacment(Solid3d stair3d)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            GripDataCollection grips = new GripDataCollection();
            GetGripPointsFlags bitFlags = GetGripPointsFlags.GripPointsOnly;

            stair3d.GetGripPoints(grips, 0, 0, ed.GetCurrentView().ViewDirection, bitFlags);

            // Grip point [0] is the start point of the polyline defining the stair 

            Vector3d disp = Point3d.Origin.GetVectorTo(grips[0].GripPoint);

            grips.Dispose();

            return Matrix3d.Displacement(disp);


        }


        public static Matrix3d GetStairRotation(Solid3d stair3d)
        {
            GripDataCollection grips = new GripDataCollection();
            GetGripPointsFlags bitFlags = GetGripPointsFlags.GripPointsOnly;

            stair3d.GetGripPoints(grips, 0, 0, Application.DocumentManager.MdiActiveDocument.Editor.GetCurrentView().ViewDirection, bitFlags);

            // Grip point [0] is the start point of the polyline defining the stair 
            // Grip point [0,1,2] are the points defining the vertical line and grip point [2,3,4] defining the horizontal line of the first step 

            Point2d start = new Point2d(grips[2].GripPoint.X, grips[2].GripPoint.Y);
            Point2d end = new Point2d(grips[4].GripPoint.X, grips[4].GripPoint.Y);
            Point3d pInsert = grips[0].GripPoint;

            double angle = start.GetVectorTo(end).Angle;

            grips.Dispose();

            return Matrix3d.Rotation(angle, Vector3d.ZAxis, pInsert);

        }

        public static Point3d GetStairInsertPoint (Solid3d stair3d)
        {
            GripDataCollection grips = new GripDataCollection();
            GetGripPointsFlags bitFlags = GetGripPointsFlags.GripPointsOnly;

            stair3d.GetGripPoints(grips, 0, 0, Application.DocumentManager.MdiActiveDocument.Editor.GetCurrentView().ViewDirection, bitFlags);

            // Grip point [0] is the start point of the polyline defining the stair 
            // Grip point [0,1,2] are the points defining the vertical line and grip point [2,3,4] defining the horizontal line of the first step 
            Point3d pInsert = grips[0].GripPoint;
            grips.Dispose();

            return pInsert;
        }

        public static double GetStairRotationAngle(Solid3d stair3d)
        {
            GripDataCollection grips = new GripDataCollection();
            GetGripPointsFlags bitFlags = GetGripPointsFlags.GripPointsOnly;

            stair3d.GetGripPoints(grips, 0, 0, Application.DocumentManager.MdiActiveDocument.Editor.GetCurrentView().ViewDirection, bitFlags);

            // Grip point [0] is the start point of the polyline defining the stair 
            // Grip point [0,1,2] are the points defining the vertical line and grip point [2,3,4] defining the horizontal line of the first step 

            Point2d start = new Point2d(grips[2].GripPoint.X, grips[2].GripPoint.Y);
            Point2d end = new Point2d(grips[4].GripPoint.X, grips[4].GripPoint.Y);
            Point3d pInsert = grips[0].GripPoint;
            grips.Dispose();

            return start.GetVectorTo(end).Angle;
          
        }

        public static ObjectId SelectStair()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect Stair: ");
            PromptEntityResult entRes;

            ObjectId result = ObjectId.Null;


            do
            {
                entRes = ed.GetEntity(entOpts);
                if (entRes.Status == PromptStatus.OK)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            Solid3d sol = tr.GetObject(entRes.ObjectId, OpenMode.ForRead) as Solid3d;
                            if (sol != null)
                            {
                                if (IsStairPropertySetOnSolid(sol)) result = entRes.ObjectId;
                            }
                            tr.Commit();
                        }
                        catch { }
                    }

                    if (result == ObjectId.Null) ed.WriteMessage("...not a stair, try again!");
                }
                else
                {
                    ed.WriteMessage("...no object selected.");
                }
            }
            while (result == ObjectId.Null && entRes.Status != PromptStatus.Cancel);

            return result;

        }


        public static List<BaseStairObject> GetStairList()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            List<BaseStairObject> listStairs = new List<BaseStairObject>();
            RXClass rxClass = RXClass.GetClass(typeof(Solid3d));
            BaseStairObject bso;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btRecord = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                foreach (ObjectId id in btRecord)
                {
                    if (id.ObjectClass.IsDerivedFrom(rxClass))
                    {
                        Solid3d sol = (Solid3d)tr.GetObject(id, OpenMode.ForRead);
                        bso = GetStairPropertiesFromSolid(sol);
                        if (bso != null) listStairs.Add(bso);

                    };
                }
                tr.Commit();
            }
            return listStairs;
        }

        //Layers
        public static List<LayerObject> GetLayerList()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            List<LayerObject> layerList = new List<LayerObject>();

            LayerObject bcObject;
            LayerTableRecord layer;
            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId layerId in lt)
                {
                    layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                    bcObject = new LayerObject
                    {
                        Id = layer.Id,
                        Name = layer.Name
                    };
                    layerList.Add(bcObject);
                }
            }
            return layerList;
        }

        //Overloaded with preselection
        public static List<LayerObject> GetLayerList(ObjectId selectedLayerId)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            List<LayerObject> layerList = new List<LayerObject>();

            LayerObject bcObject;
            LayerTableRecord layer;
            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId layerId in lt)
                {
                    layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                    bcObject = new LayerObject
                    {
                        Id = layer.Id,
                        Name = layer.Name,
                        IsSelected = (layer.Id == selectedLayerId)
                    };
                    layerList.Add(bcObject);
                    // if (bcObject.IsSelected) ed.WriteMessage("\n Selected layer: {0} {1}", bcObject.Id, bcObject.Name);
                }
            }
            return layerList;
        }
    }


}
