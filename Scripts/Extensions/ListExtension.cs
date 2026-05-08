using UnityEngine;
using System.Collections.Generic;


namespace Local.Scripts.Extensions
{
    public static class ListExtension
    {
        public static List<Color> ToColors(this List<int> list)
        {
            List<Color> colors = new List<Color>();
            
            if (list.Count % 3 != 0)
            {
                return colors;
            }

            for (int i = 0; i < list.Count / 3; i++)
            {
                colors.Add(new Color(list[3*i]/255.0f, list[3*i+1]/255.0F, list[3*i+2]/255.0f));
            }

            return colors;
        }
    }
}