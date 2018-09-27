// (C) Copyright 2018 by  
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;


using static LX_Anchor.MyOverrules;

// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(LX_Anchor.MyPlugin))]

namespace LX_Anchor
{

    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class MyPlugin : IExtensionApplication
    {
        private static DrawableOverruleAnchor _overruleAnchoredElelement = null;
        private static DrawableOverruleAlignment _overruleHostElement = null;

        void IExtensionApplication.Initialize()
        {


                _overruleAnchoredElelement = new DrawableOverruleAnchor();
                Overrule.AddOverrule(RXClass.GetClass(typeof(Circle)), _overruleAnchoredElelement, false);

                //Setting Filter
                _overruleAnchoredElelement.SetExtensionDictionaryEntryFilter("anchor");


                _overruleHostElement = new DrawableOverruleAlignment();
                Overrule.AddOverrule(RXClass.GetClass(typeof(Alignment)), _overruleHostElement, false);

                //Setting Filter
                _overruleHostElement.SetExtensionDictionaryEntryFilter("anchor");


            Overrule.Overruling = true;
            // Regen is required to update changes on screen
            Application.DocumentManager.MdiActiveDocument.Editor.Regen();

        }

        void IExtensionApplication.Terminate()
        {
            // Do plug-in application clean up here
        }

    }

}
