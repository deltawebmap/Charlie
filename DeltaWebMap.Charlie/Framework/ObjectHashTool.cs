using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    public static class ObjectHashTool
    {
        public static ushort HashObject(object o)
        {
            ulong incrementer = 0;
            try
            {
                _HashClass(o, ref incrementer);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return (ushort)incrementer;
        }

        private static void _HashClass(object o, ref ulong incrementer)
        {
            var props = o.GetType().GetProperties();
            foreach(var p in props)
            {
                object value = p.GetValue(o);
                if (value == null)
                {
                    incrementer -= 1;
                    return;
                }
                if(p.PropertyType.IsArray)
                {
                    Array valueArr = (Array)value;
                    foreach (var va in valueArr)
                        _HashObject(va, ref incrementer);
                } else
                {
                    _HashObject(value, ref incrementer);
                }
            }
        }

        private static void _HashObject(object o, ref ulong incrementer)
        {
            Type type = o.GetType();
            if (type == typeof(string))
            {
                foreach (char c in (string)o)
                    incrementer += c;
            } else if (type == typeof(bool)) {
                if ((bool)o)
                    incrementer += 0xFF;
                else
                    incrementer += 0xAA;
            } else if (type == typeof(byte))
            {
                incrementer += (byte)o;
            }
            else if (type == typeof(ushort))
            {
                incrementer += (ushort)o;
            }
            else if (type == typeof(short))
            {
                incrementer += (ulong)((short)o);
            }
            else if (type == typeof(int))
            {
                incrementer += (ulong)((int)o);
            }
            else if (type == typeof(uint))
            {
                incrementer += (uint)o;
            }
            else if (type == typeof(long))
            {
                incrementer += (ulong)((long)o);
            }
            else if (type == typeof(ulong))
            {
                incrementer += (ulong)o;
            }
            else if (type == typeof(float))
            {
                incrementer += (ulong)((float)o);
            }
            else if (type == typeof(double))
            {
                incrementer += (ulong)((double)o);
            } else if (type == typeof(DeltaAsset) || type == typeof(ItemEntry_ConsumableAddStatusValue) || type == typeof(DinosaurEntryStatusComponent) || type == typeof(DinosaurEntryFood))
            {
                _HashClass(o, ref incrementer);
            }
        }
    }
}
