// (C) Copyright 2018 by  
//
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

using ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId;


// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(LX_SolidStair.MyPlugin))]

namespace LX_SolidStair
{
    
    public class MyPlugin : IExtensionApplication
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        static public string psdName = "LXStair";

        // private static DrawableOverruleStair overruleALCSolidStair = null;

        void IExtensionApplication.Initialize()
        {
            // Check StairParameterSetDefinition exist 
            ObjectId psdId = MyFunctions.GetPropertySetDefinitionIdByName(psdName);
            if (psdId == ObjectId.Null)
            {
                MyFunctions.CreateStairPropertySetDefinition(MyPlugin.psdName);
                ed.WriteMessage("\n Property set defenition {0} created", psdName);
            }

            //if (overruleALCSolidStair == null)
            //{
            //    overruleALCSolidStair = new DrawableOverruleStair();
            //    Overrule.AddOverrule(RXClass.GetClass(typeof(Solid3d)), overruleALCSolidStair, false);

            //    //This is required in order for IsApplicable() being used as custom overrule filter
            //    overruleALCSolidStair.SetCustomFilter();
            //    Overrule.Overruling = true;
            //}
            //// Regen is required to update changes on screen
            //Application.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        void IExtensionApplication.Terminate()
        {
            // Do plug-in application clean up here
            //if (overruleALCSolidStair != null)
            //{
            //    Overrule.RemoveOverrule(RXClass.GetClass(typeof(Solid3d)), overruleALCSolidStair);
            //    overruleALCSolidStair.Dispose();
            //    overruleALCSolidStair = null;
            //}
        }

    }

}
