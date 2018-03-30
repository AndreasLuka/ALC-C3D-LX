// (C) Copyright 2018 by Andreas Luka
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.DatabaseServices;

// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(ALC_C3DStair.MyPlugin))]

namespace ALC_C3DStair
{

    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class MyPlugin : IExtensionApplication
    {

        private static DrawableOverruleAlignment _overruleAlignment = null;


        void IExtensionApplication.Initialize()
        {
            // Add one time initialization here
            // One common scenario is to setup a callback function here that 
            // unmanaged code can call. 
            // To do this:
            // 1. Export a function from unmanaged code that takes a function
            //    pointer and stores the passed in value in a global variable.
            // 2. Call this exported function in this function passing delegate.
            // 3. When unmanaged code needs the services of this managed module
            //    you simply call acrxLoadApp() and by the time acrxLoadApp 
            //    returns  global function pointer is initialized to point to
            //    the C# delegate.
            // For more info see: 
            // http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
            // http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
            // http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
            // as well as some of the existing AutoCAD managed apps.

            // Initialize your plug-in application here

            if (_overruleAlignment == null)
            {
                _overruleAlignment = new DrawableOverruleAlignment();
                Overrule.AddOverrule(RXClass.GetClass(typeof(Alignment)), _overruleAlignment, false);

                //This is required in order for IsApplicable() being used as custom overrule filter
                _overruleAlignment.SetCustomFilter();
                Overrule.Overruling = true;
            }

         Application.DocumentManager.MdiActiveDocument.Editor.Regen();
            //
        }

        void IExtensionApplication.Terminate()
        {
            if (_overruleAlignment != null)
            { 
                Overrule.RemoveOverrule(RXClass.GetClass(typeof(Alignment)), _overruleAlignment);
                _overruleAlignment.Dispose();
                _overruleAlignment = null;
            }

        }

    }

}