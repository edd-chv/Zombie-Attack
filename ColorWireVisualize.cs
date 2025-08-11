using System.Collections.Generic;
using UnityEngine;

namespace WiresMiniGame
{
    public class ColorWireVisualize : MonoBehaviour
    {
        public ColorWire[] ColorWires;

        public List<SpriteRenderer> Colors;

        public List<SpriteRenderer> FreeBool;

        public Color ColorTrue;
        public Color ColorFalse;

        private bool _init = false; 

        public void Init(ref ColorWire[] cw)
        {
            ColorWires = cw;
            OutputUI();
            _init = true;
        }

        void Update()
        {
            OutputUI();
        }

        private void OutputUI()
        {
            if (_init)
            {
                for (int i = 0; i < ColorWires.Length; i++)
                {
                    Colors[i].color = ColorWires[i].color;
                    FreeBool[i].color = Bool2Color(ColorWires[i].IsFree);
                }
            }
        }

        private Color Bool2Color(bool value)
        {
            if (value)
            {
                return ColorTrue;
            }
            else
            {
                return ColorFalse;
            }
        }
    }
}