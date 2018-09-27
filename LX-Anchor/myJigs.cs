// (C) Copyright 2018 by Andreas Luka

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;

using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.Colors;

namespace LX_Anchor
{
    class StationJig : DrawJig
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

        public Point3d ptInsert;
        public double station = Double.NaN;
        public double offset = Double.NaN;
        public double easting = Double.NaN;
        public double northing = Double.NaN;
        public Matrix3d matDisplacement;

        private Autodesk.AutoCAD.DatabaseServices.Entity e;
        private Alignment a;
        private Point3d ptObject;

        Point2d ptOnAlignment = new Point2d(Double.NaN, Double.NaN);
        Point2d ptOffAlignment = new Point2d(Double.NaN, Double.NaN);

        bool inRange;

        public StationJig(Alignment _a, Autodesk.AutoCAD.DatabaseServices.Entity _e, Point3d _ptObject)
        {
            this.e = _e;
            this.a = _a;
            this.ptObject = _ptObject;
        }


        protected override bool WorldDraw(WorldDraw wd)
        {
            {
                Color acColor = Color.FromColorIndex(ColorMethod.ByAci, 1);
                wd.SubEntityTraits.Color = 1;
                easting = ptOnAlignment.X;
                northing = ptOnAlignment.Y;
                wd.Geometry.WorldLine(new Point3d(easting, northing, 0), new Point3d(ptOffAlignment.X, ptOffAlignment.Y, 0));
                WorldGeometry geo = wd.Geometry;
                if (geo != null)
                {
                    geo.PushModelTransform(matDisplacement);
                    geo.Draw(e);
                    geo.PopModelTransform();
                }
                return inRange;
            }
        }


        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify the point on alignment: ")
            {
                // UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZDirectionOrtho | UserInputControls.UseBasePointElevation
            };

            PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

            if (ppr1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

            if (ppr1.Value.DistanceTo(ptInsert) < Tolerance.Global.EqualPoint) return SamplerStatus.NoChange;
            else
            {
                ptInsert = ppr1.Value;

                ptOffAlignment = new Point2d(ppr1.Value.X, ppr1.Value.Y);

                //finding the perpendicular point on the alignment
                inRange = MyFunctions.InRangePerpendicularPoint(a, ptOffAlignment, ref ptOnAlignment, ref station);
                if (inRange)
                {
                    Vector3d disp = ptObject.GetVectorTo(new Point3d(ptOnAlignment.X, ptOnAlignment.Y, 0));
                    matDisplacement = Matrix3d.Displacement(disp);
                }
            }
            return SamplerStatus.OK;
        }

    }

    class CogoPointJig : EntityJig
    {
        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
        public Point3d ptInsert;

        public double easting = Double.NaN;
        public double northing = Double.NaN;
        private CogoPoint cp;
        private Point3d ptStart;

        public CogoPointJig(CogoPoint _cp, Point3d _ptStart) : base(_cp)
        {
            cp = _cp;
            ptStart = _ptStart;
        }

        protected override bool Update()
        {
            cp.Easting = ptInsert.X;
            cp.Northing = ptInsert.Y;
            easting = ptInsert.X;
            northing = ptInsert.Y;
            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions opts1 = new JigPromptPointOptions("\nSpecify the point on alignment: ")
            {

            // UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.NoZDirectionOrtho | UserInputControls.UseBasePointElevation
            };
            opts1.UseBasePoint = true;
            opts1.BasePoint = ptStart;
            opts1.Cursor = CursorType.RubberBand;

            PromptPointResult ppr1 = prompts.AcquirePoint(opts1);

            if (ppr1.Status == PromptStatus.Cancel) return SamplerStatus.Cancel;

            if (ppr1.Value.DistanceTo(ptInsert) < Tolerance.Global.EqualPoint) return SamplerStatus.NoChange;
            else
            {
                ptInsert = ppr1.Value;
            }
            return SamplerStatus.OK;
        }

    }
}

       
