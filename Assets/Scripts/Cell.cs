using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class Cell
    {
        public static int CellPointer = 0;
        public static float Threshold = 128f;
        private float lastInput = 0;
        public bool Active = false;
        public int x;
        public int y;
        public float diff;

        public Cell(float x, float y)
        {
            this.x = (int)x;
            this.y = (int)y;
        }

        public bool CheckCell(float input)
        {

            diff = Math.Abs(lastInput - input);  

            lastInput = input;
            if (diff > Threshold)
            {
                Active = true;
                return true;
            }
            else
            {
                Active = false;
                return false;
            }
        }
    }
}
