

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using System;


namespace LX_SolidStair
{

    public class GripOverruleStair : GripOverrule
    {


        public override bool IsApplicable(RXObject overruledSubject)
        {
            Solid3d sol = overruledSubject as Solid3d;
            if (sol == null) return false;
            else return (MyFunctions.IsStairPropertySetOnSolid(sol));
         }  

    }
}
