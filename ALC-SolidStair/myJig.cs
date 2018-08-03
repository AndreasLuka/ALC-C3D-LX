using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace LX_SolidStair
{
    class MyJig2 : DrawJig
    {

        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;


        private Solid3d stair;
        public Point3d pInsert, pSecond;
        public Double angInsert;

        public int steps;
        public double tread;
        public double riser;
        public double landing;
        public double width;
        public double slope;

        public Matrix3d matDisplacement;
        public Matrix3d matRotation; 

        public int jigStatus = 0;  //0  = Insert :: 1 = Rotation
        public Boolean jigUpdate = false;

        public MyJig2(int steps, double riser, double tread, double landing, double width, double slope)
        {
            this.stair = MyFunctions.CreateStair3D(steps, riser, tread, landing, width, slope);
            this.steps = steps;
            this.tread = tread;
            this.riser = riser;
            this.landing = landing;
            this.width = width;
            this.slope = slope;
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (jigStatus)
            {
                case 0:
                    JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify the start point: ")
                    {
                        UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZDirectionOrtho | UserInputControls.UseBasePointElevation
                    };

                    PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

                    if (ppr1.Status == PromptStatus.Cancel)
                    {
                        return SamplerStatus.Cancel;
                    }

                    if (ppr1.Value.DistanceTo(pInsert) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        pInsert = ppr1.Value;
                        Vector3d disp = Point3d.Origin.GetVectorTo(pInsert);
                        matDisplacement = Matrix3d.Displacement(disp);
                        return SamplerStatus.OK;
                    }

                case 1:
                    JigPromptPointOptions opts2 = new JigPromptPointOptions("\nSpecify second point:")
                    {
                        UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZDirectionOrtho | UserInputControls.UseBasePointElevation,


                        BasePoint = pInsert,
                        UseBasePoint = true,
                        Cursor = CursorType.RubberBand
                    };
                    PromptPointResult ppr2 = prompts.AcquirePoint(opts2);

                    if (ppr2.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

                    if (ppr2.Value.DistanceTo(pSecond) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        pSecond = ppr2.Value;
                        Point2d start = new Point2d(pInsert.X, pInsert.Y);
                        Point2d end = new Point2d(pSecond.X, pSecond.Y);
                        angInsert = start.GetVectorTo(end).Angle;
                        matRotation = Matrix3d.Rotation(angInsert, Vector3d.ZAxis, pInsert); 
                        return SamplerStatus.OK;
                    }
                default: break;
            }
            return SamplerStatus.OK;

        }

        protected override bool WorldDraw(WorldDraw draw)
        {

            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                if (jigUpdate)
                {
                    stair = MyFunctions.CreateStair3D(steps, riser, tread, landing, width, slope);
                }
                switch (jigStatus)
                {
                    case 0:
                        geo.PushModelTransform(matDisplacement);
                        geo.Draw(stair);
                        geo.PopModelTransform();
                        break;
                    case 1:
                        geo.PushModelTransform(matDisplacement.PreMultiplyBy(matRotation));
                        geo.Draw(stair);
                        geo.PopModelTransform();
                        break; ;
                    default: return false;
                }
            }
            return true;
        }
    }

}