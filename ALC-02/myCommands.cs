// (C) Copyright 2017 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace ALC_02.ACAD
{

    public class MyCommands
    {

        [CommandMethod("alcFlatten")]
        public void alcFlatten()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            Entity dbEntity;
 
            // reading all elements from model-spae
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                BlockTableRecord btRecord = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);

                foreach (ObjectId id in btRecord)
                {
                    dbEntity = (Entity)tr.GetObject(id, OpenMode.ForRead);

                    //Access to the entity
 
                    var property = dbEntity.GetType().GetProperty("Thickness");
                    if (property != null)
                    { //object is a class with thickness
                        var oldThickness = Convert.ToInt64(property.GetValue(dbEntity));
                        if (oldThickness > 0)
                        {
                            dbEntity.UpgradeOpen();
                            dbEntity.GetType().GetProperty("Thickness").SetValue(dbEntity, 0);

                            if (dbEntity.Layer == "0")
                            {
                                var newLayerName = oldThickness.ToString();
                                
                                using (var trLayer = db.TransactionManager.StartTransaction())
                                {
                                    LayerTable lt = (LayerTable)trLayer.GetObject(db.LayerTableId, OpenMode.ForRead);
                                    if (!lt.Has(newLayerName))
                                    {                                    
                                        LayerTableRecord ltr = new LayerTableRecord();
                                        ltr.Name = newLayerName;
                                        lt.UpgradeOpen();
                                        ObjectId ltId = lt.Add(ltr);
                                        tr.AddNewlyCreatedDBObject(ltr, true);
                                     };

                                     dbEntity.LayerId = lt[newLayerName];
                                     trLayer.Commit();
                                }
                            }
                        }

                    }
                }
                tr.Commit();
                
            }
        }      
    }
}
