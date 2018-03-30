// (C) Copyright 2018 by  
//
using System;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using Autodesk.Aec.PropertyData.DatabaseServices;

using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.ApplicationServices;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ALC_ParameterSetSample.MyCommands))]

namespace ALC_ParameterSetSample
{
    public class MyCommands
    {
        // Modal Command 

        [CommandMethod("MyGroup", "ALC-CheckStairParameter", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void IsStairWithPArameter()
        {
            CivilDocument civdoc = CivilApplication.ActiveDocument;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            double tread = 0;
            double riser = 0;
            double landing = 0;
            double width = 0;
            int steps = 0;

            // Check if drawing has alignments
            ObjectIdCollection alignIds = civdoc.GetAlignmentIds();
            if (alignIds.Count == 0)
            {
                ed.WriteMessage("\n" + "Error: No alignments in this drawing");
                return;
            }

            // Check drawing has PropertySet "Stair"defined
            ObjectId psdId = GetPropertySetDefinitionIdByName("Stair");
            if (psdId == ObjectId.Null)
            {
                ed.WriteMessage("\n" + "Error: No property set Stair in this drawing");
                return;
            }

            ObjectId alignId = SelectAlignment();

            if (alignId == ObjectId.Null) return;

            // Alignment is selected
            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                Alignment align = (Alignment)alignId.GetObject(OpenMode.ForRead);
                if (GetStairParametersFromPropertySetAttachedToAlignment(align, ref tread, ref riser, ref landing, ref width, ref steps))
                {
                    ed.WriteMessage("\n{0} Tread: {1} Riser: {2} Landing: {3} Width: {4} Steps: {45}", align.Name, tread, riser, landing, width, steps);
                }
                tr.Commit();
            };

        }

        public static bool GetStairParametersFromPropertySetAttachedToAlignment(Alignment align, ref double tread, ref double riser, ref double landing, ref double width, ref int steps)
        {
            using (Transaction tr = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
            {
                ObjectIdCollection setIds = PropertyDataServices.GetPropertySets(align);
                if (setIds.Count == 0) return false;

                foreach (ObjectId id in setIds)
                {
                    PropertySet pset = (PropertySet)id.GetObject(OpenMode.ForRead);
                    if (pset.PropertySetDefinitionName == "Stair")
                    {
                        PropertySetDefinition psetDef = tr.GetObject(pset.PropertySetDefinition, OpenMode.ForRead) as PropertySetDefinition;
                        PropertyDefinitionCollection propDefs = psetDef.Definitions;
                        {
                            // Get the ObjectID of the property set definition by name
                            // Get the value of the property definition
                            tread = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("tread")));
                            riser = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("riser")));
                            landing = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("landing")));
                            width = Convert.ToDouble(pset.GetAt(pset.PropertyNameToId("width")));
                            steps = Convert.ToInt16(pset.GetAt(pset.PropertyNameToId("steps")));
                        }
                    }
                    return true;
                }
                tr.Commit();
            }
            return false;
        }

        public static ObjectId GetPropertySetDefinitionIdByName(string psdName)
        {
            ObjectId result = ObjectId.Null;
            Database db = Application.DocumentManager.MdiActiveDocument.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DictionaryPropertySetDefinitions psdDict = new DictionaryPropertySetDefinitions(db);
                if (psdDict.Has(psdName, tr)) result = psdDict.GetAt(psdName);
                tr.Commit();
            }
            return result;
        }


        public static ObjectId SelectAlignment()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectId result = ObjectId.Null;

            PromptEntityOptions entOpts = new PromptEntityOptions("\nSelect alignment: ") ;
            entOpts.SetRejectMessage("...not an Alignment, try again!:");
            entOpts.AddAllowedClass(typeof(Alignment), true);

            PromptEntityResult entRes = ed.GetEntity(entOpts);
            if (entRes.Status == PromptStatus.OK) result = entRes.ObjectId;

            return result;
        }
        
    }
}
