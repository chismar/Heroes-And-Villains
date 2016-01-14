﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MapRoot;
using Demiurg.Core.Extensions;

namespace CoreMod
{
    public class GraphicsObjectPresenter : ObjectPresenter<GraphicsTile>
    {
        Image image;
        Text hoverText;
        Text selectText;
        GameObject hoverPanelGO;
        GameObject selectPanelGO;

        GraphicsTile hoverObj;
        GraphicsTile selectObj;

        public override void Setup (ITable definesTable)
        {
            GameObject selectionGO = GameObject.Find ("SelectionPanel");
            GameObject hoverGO = GameObject.Find ("HoverPanel");
            hoverPanelGO = Object.Instantiate (Resources.Load ("UI/Panel"), Vector3.zero, Quaternion.identity) as GameObject; 
            hoverPanelGO.name = "Hover panel";
            hoverPanelGO.transform.SetParent (hoverGO.transform, false);
            selectPanelGO = Object.Instantiate (Resources.Load ("UI/VerticalLayoutPanel")) as GameObject; 
            hoverPanelGO.name = "Selection panel";
            selectPanelGO.transform.SetParent (selectionGO.transform, false);

            RectTransform hoverTransform = hoverPanelGO.GetComponent<RectTransform> ();
            hoverTransform.sizeDelta = Vector2.zero;
            RectTransform selectionTransform = selectPanelGO.GetComponent<RectTransform> ();
            selectionTransform.sizeDelta = Vector2.zero;
            GameObject imageGO = Object.Instantiate (Resources.Load ("UI/Image"), Vector3.zero, Quaternion.identity) as GameObject; 
            imageGO.name = "Image";
            image = imageGO.GetComponent<Image> ();
            imageGO.transform.SetParent (selectionTransform, false);
            

            GameObject hoverTextGO = Object.Instantiate (Resources.Load ("UI/Text"), Vector3.zero, Quaternion.identity) as GameObject; 
            hoverTextGO.transform.SetParent (hoverTransform, false);
            hoverText = hoverTextGO.GetComponent<Text> ();

            GameObject selectionTextGO = Object.Instantiate (Resources.Load ("UI/Text"), Vector3.zero, Quaternion.identity) as GameObject; 
            selectionTextGO.transform.SetParent (selectionTransform, false);
            selectText = selectionTextGO.GetComponent<Text> ();

            hoverPanelGO.SetActive (false);
            selectPanelGO.SetActive (false);
        }


        public override void ShowObjectDesc (GraphicsTile obj)
        {
            selectPanelGO.SetActive (true);
            image.sprite = obj.Sprite;
            selectText.text = obj.Name;
            selectObj = obj;
        }

        public override void HideObjectDesc (GraphicsTile obj)
        {
            selectPanelGO.SetActive (false);
            selectObj = null;
        }

        public override void ShowObjectShortDesc (GraphicsTile obj)
        {
            hoverPanelGO.SetActive (true);
            hoverText.text = obj.Name;
            hoverObj = obj;
        }

        public override void HideObjectShortDesc (GraphicsTile obj)
        {
            hoverPanelGO.SetActive (false);
            hoverObj = null;
        }

        public override void Update ()
        {
            if (selectObj != null)
                ShowObjectDesc (selectObj);
            if (hoverObj != null)
                ShowObjectShortDesc (hoverObj);
        }

    }
}

