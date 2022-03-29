using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ExtendedGraphicData
    {
        public bool alignToBottom = false;
        public bool rotateDrawSize = true;
        public bool? drawRotatedOverride = null;
        public Vector3 drawOffset = Vector3.zero;
        public List<string> linkStrings;
        public List<DynamicTextureParameter> textureParams;
    }

    public class DynamicTextureParameter
    {
        [NoTranslate]
        private string name;
        [NoTranslate]
        private string path;

        private Texture2D[] mats;

        public void ApplyOn(Graphic graphic)
        {
            switch (graphic)
            {
                case Graphic_Single s:
                    mats = new Texture2D[1];
                    mats[0] = ContentFinder<Texture2D>.Get($"{path}", false);
                    if (mats[0] != null)
                        s.MatSingle.SetTexture(name, mats[0]);
                    return;
                case Graphic_Multi m:
                    mats = new Texture2D[4];
                    mats[0] = ContentFinder<Texture2D>.Get($"{path}_north", false);
                    mats[1] = ContentFinder<Texture2D>.Get($"{path}_east", false);
                    mats[2] = ContentFinder<Texture2D>.Get($"{path}_south", false);
                    mats[3] = ContentFinder<Texture2D>.Get($"{path}_west", false);

                    if (mats[0] != null)
                        m.MatNorth.SetTexture(name, mats[0]);
                    if (mats[1] != null)
                        m.MatEast.SetTexture(name, mats[1]);
                    if (mats[2] != null)
                        m.MatSouth.SetTexture(name, mats[2]);
                    if (mats[3] != null)
                        m.MatWest.SetTexture(name, mats[3]);
                    if (m.EastFlipped)
                    {
                        m.MatEast.SetTexture(name, mats[3]);
                    }

                    if (m.WestFlipped)
                    {
                        m.MatWest.SetTexture(name, mats[1]);
                    }

                    return;
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                TLog.Error($"Misconfigured DynamicTextureParameter: {xmlRoot.OuterXml}");
                return;
            }
            this.name = xmlRoot.Name;
            this.path = xmlRoot.FirstChild.Value;
        }
    }
}
