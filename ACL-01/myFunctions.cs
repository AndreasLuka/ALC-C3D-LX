using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Autodesk.AutoCAD.Runtime;


namespace ALCJIG2.Civil3D
{
    class myFunctions
    {
        public static List<BaseC3DObject> GetAlignments()
        {
            CivilDocument civdoc = CivilApplication.ActiveDocument;
            List<BaseC3DObject> alignList = new List<BaseC3DObject>();
            BaseC3DObject bcObject;

            ObjectIdCollection alignIds = civdoc.GetAlignmentIds();
            if (alignIds.Count == 0) return null;
            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId id in alignIds)
                    {
                        Alignment align = trans.GetObject(id, OpenMode.ForRead) as Alignment;
                        bcObject = new BaseC3DObject();
                        bcObject.Id = align.Id;
                        bcObject.Name = align.Name;
                        alignList.Add(bcObject);
                    }
                }
                catch
                {
                }
            }
            return alignList;
        }

        public static List<BaseC3DObject> GetAlignmentStyles()
        {
            CivilDocument civdoc = CivilApplication.ActiveDocument;
            List<BaseC3DObject> alignStyleList = new List<BaseC3DObject>();
            BaseC3DObject bcObject;

            AlignmentStyleCollection alignStyles = civdoc.Styles.AlignmentStyles;

            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId id in alignStyles)
                    {
                        AlignmentStyle alignStyle = trans.GetObject(id, OpenMode.ForRead) as AlignmentStyle;
                        bcObject = new BaseC3DObject();
                        bcObject.Id = alignStyle.Id;
                        bcObject.Name = alignStyle.Name;
                        alignStyleList.Add(bcObject);
                    }
                }
                catch
                {
                }
            }
            return alignStyleList;
        }



        public static List<BaseC3DObject> GetLayers(Database db)
        {
            List<BaseC3DObject> layerList = new List<BaseC3DObject>();
            BaseC3DObject bcObject;
            LayerTableRecord layer;
            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    foreach (ObjectId layerId in lt)
                    {
                        layer = tr.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                        bcObject = new BaseC3DObject();
                        bcObject.Id = layer.Id;
                        bcObject.Name = layer.Name;
                        layerList.Add(bcObject);
                    }
            }
            return layerList;
        }
        
         
    }
}
