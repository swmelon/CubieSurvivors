using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public class Locator
    {
        private static readonly Dictionary<Vector3, (Vector3 vertical, Vector3 horizontal)> axisMap = new Dictionary<Vector3, (Vector3 vertical, Vector3 horizontal)>()
        {
            {Vector3.right, (vertical: Vector3.forward, horizontal:Vector3.right)},
            {Vector3.forward,(vertical: Vector3.right, horizontal: Vector3.forward)},
            {-Vector3.right, (vertical: -Vector3.forward, horizontal: Vector3.right)},
            {-Vector3.forward, (vertical: -Vector3.right, horizontal: Vector3.forward)},
        };

        private static readonly Dictionary<Vector3, Quaternion> rotationMap = new Dictionary<Vector3, Quaternion>()
        {
            { Vector3.right, Quaternion.Euler(0, 0, 0) },
            { Vector3.forward, Quaternion.Euler(0, 90, 0) },
            { -Vector3.right, Quaternion.Euler(0, 180, 0) },
            { -Vector3.forward, Quaternion.Euler(0, 270, 0) },
        };

        private Dictionary<Vector3, bool[]> axisMarker;
        private Dictionary<Type, int> hMap;
        private Dictionary<Type, Dictionary<Vector3, Vector3[]>> allLocations = new Dictionary<Type, Dictionary<Vector3, Vector3[]>>();
        private int n;

        public Locator(int n, Dictionary<Type, int> hMap)
        {
            axisMarker = new Dictionary<Vector3, bool[]>();
            
            foreach (var item in hMap)
            {
                allLocations[item.Key] = new Dictionary<Vector3, Vector3[]>();    
                allLocations[item.Key] = InitLocations(n, item.Value);
            }
            
            foreach (var key in axisMap.Keys)
            {
                axisMarker[key] = Enumerable.Repeat(false, n).ToArray();
            }
            
            this.n = n;
            this.hMap = hMap;
        }

        public void Clear()
        {
            // Clear axisMarker
            foreach(var key in axisMarker.Keys)
            {
                for (int i = 0; i < axisMarker[key].Length; i++)
                {
                    axisMarker[key][i] = false;
                }
            }
            
            // Clear allLocations
            // 각 타입에 대하여
            foreach (var kvp in allLocations)
            {
                var type = kvp.Key;
                var locations = kvp.Value;
                var h = hMap[type];
                
                int space = 0;
            
                // 플로어 안쪽의 배치물이 튀어나오지 않게 함
                if (n > h * 2 + 2)
                {
                    space = (n - (h * 2 + 2))/2;
                }
            
                int offset = n / 2 - 1;
                
                // 각 방향에 대하여 (전후좌우)
                foreach (var key in locations.Keys)
                {
                    for (int i = 0; i < n - 1; i++)
                    {
                        locations[key][i] = h * axisMap[key].vertical + (i - offset) * axisMap[key].horizontal;
                    }
                    
                    for (int i = 0; i < space; i++)
                    {
                        locations[key][i] = Vector3.zero;
                        locations[key][n - 2 - i] = Vector3.zero;
                    }
                }
            }
        }

        public static (Vector3 vertical, Vector3 horizontal) GetKeyAxis(Vector3 position)
        {
            Vector3 axis;
            bool x = position.x != 0;
            bool z = position.z != 0;

            Debug.Assert(!((x && z) || (!x && !z)), "invalid position. x ^ z must be true");

            if (z)
            {
                if (position.z > 0)
                {
                    axis = Vector3.right;
                }
                else
                {
                    axis = -Vector3.right;
                }
            }
            else
            {
                if (position.x > 0)
                {
                    axis = Vector3.forward;
                }
                else
                {
                    axis = -Vector3.forward;
                }
            }
            
            return axisMap[axis];
        }
        
        private Dictionary<Vector3, Vector3[]> InitLocations(int n, int h)
        {
            Dictionary<Vector3, Vector3[]> locations = new Dictionary<Vector3, Vector3[]>();

            int space = 0;
            
            // 플로어 안쪽의 배치물이 튀어나오지 않게 함
            if (n > h * 2 + 2)
            {
                space = (n - (h * 2 + 2))/2;
            }
            
            int offset = n / 2 - 1;
            
            foreach (var key in axisMap.Keys)
            {
                locations[key] = new Vector3[n - 1];
                axisMarker[key] = new bool[n - 1];
                
                for (int i = 0; i < n - 1; i++)
                {
                    locations[key][i] = h * axisMap[key].vertical + (i - offset) * axisMap[key].horizontal;
                }
                
                //space 만큼 사용할 수 없게 함 (Floor보다 바깥쪽 자리의 양 끝을 자름)
                for (int i = 0; i < space; i++)
                {
                    locations[key][i] = Vector3.zero;
                    locations[key][n - 2 - i] = Vector3.zero;
                }
            }

            return locations;
        }

        private Dictionary<Vector3, Vector3[]> GetLocationsCorresponding(ILocatable locatable)
        {
            Type type = locatable.GetType();

            foreach(Type key in allLocations.Keys)
            {
                if (key.IsAssignableFrom(type))
                {
                    return allLocations[key];
                }
            }

            return new Dictionary<Vector3, Vector3[]>();
        }

        private void MarkAxis(Vector3 axis, int index)
        {
            if (axisMarker[axis][index])
            {
                Debug.LogError("axisMarker is already marked.");
            }
            
            axisMarker[axis][index] = false;

            foreach (var locations in allLocations.Values)
            {
                locations[axis][index] = Vector3.zero;
            }
        }

        public bool Locate(ILocatable locatable, out Vector3 location, out Quaternion rotation)
        {
            
            if (!locatable.SelectLocation(GetLocationsCorresponding(locatable), out List<Vector3> selected))
            {
                location = Vector3.zero;
                rotation = Quaternion.identity;
                return false;
            }
            
            if (selected.Count == 1)
            {
                location = selected.First();
            }
            else
            {
                Vector3 first = selected.First();
                Vector3 last = selected.Last();
                location = 0.5f * (first + last);
            }


            int sign = 0;
            
            float x = MathF.Abs(location.x);
            float z = MathF.Abs(location.z);
           

            if (x == z)
            {
                Debug.Assert(selected.Count == 1, "selected coordinate size must be one if x == y");
                rotation = Quaternion.identity;
                Vector3 zAxis, xAxis;
                
                // x가 양수라면, 양의 vertical x를 지닌 zAxis
                if (location.x > 0)
                {
                    zAxis = Vector3.forward;
                }
                else
                {
                    zAxis = -Vector3.forward;
                }

                if (location.z > 0)
                {
                    xAxis = Vector3.right;
                }
                else
                {
                    xAxis = -Vector3.right;
                }
                
                int index = Mathf.RoundToInt(Vector3.Dot(Vector3.right, location)) +
                            ((axisMarker[zAxis].Length + 1) / 2 - 1);
                MarkAxis(zAxis, index);
                
                index = Mathf.RoundToInt(Vector3.Dot(Vector3.forward, location)) +
                        ((axisMarker[xAxis].Length + 1) / 2 - 1);
                
                MarkAxis(xAxis, index);
                return true;
            }
            
            Vector3 axis = Vector3.zero;
            
            if (x > z)
            {
                axis = Vector3.forward;
                
                if (location.x > 0)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
            }
            else if (x < z)
            {
                axis = Vector3.right;
                
                if (location.z > 0)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
            }
            
            axis *= sign;
            rotation = rotationMap[axis];
               
            foreach (var loc in selected)
            {
                // if baseStage size = 1, axis value can be (-4, -3, .., 3, 4)
                int index = Mathf.RoundToInt(Vector3.Dot(axisMap[axis].horizontal, loc)) +
                            ((axisMarker[axis].Length + 1) / 2 - 1);
                MarkAxis(axis, index);
            }
            
            return true;
        }

        private int RoundAbs(float a)
        {
            return Mathf.Abs(Mathf.RoundToInt(a));
        }

       
    }