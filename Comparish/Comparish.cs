using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Comparish
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataDescriptorAttribute : Attribute
    {
        internal object meaning { get; private set; }

        public DataDescriptorAttribute(object Meaning)
        {
            meaning = Meaning;
        }
    }

    public static class Meaning
    {
        public static bool MeaninglyEquals<T1, T2>(T1 item1, T2 item2, object DataMeaning,
            bool requireMatchingTypes = false, bool AllowVacuouslyComparison = false,
            bool BSubSetsA = false)
        {
            if (requireMatchingTypes)
            {
                EnsureMatchingTypeRequirement(item1, item2);
            }

            var item1Props = GetMeaningfulProperties(item1, DataMeaning);
            var item2Props = GetMeaningfulProperties(item2, DataMeaning);

            if (!AllowVacuouslyComparison)
            {
                EnsureVacuousComparisonRequirement(item1, item2, DataMeaning);
            }

            EnsureDataSetComparisonRequirement(item1, item2, DataMeaning, BSubSetsA);

            using (var enum1 = item1Props.GetEnumerator())
            using (var enum2 = item2Props.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    var v1 = enum1.Current.GetValue(item1);
                    var v2 = enum2.Current.GetValue(item2);

                    if (v1?.Equals(v2) ?? v2 == null)
                    {
                        continue;
                    }

                    if (v1 is string || v2 is string)
                    {
                        return false;
                    }

                    if (v1 is IEnumerable l1 && v2 is IEnumerable l2)
                    {
                        if (ListLikeItemsAreEqual(l1, l2, DataMeaning))
                        {
                            continue;
                        }
                    } 
                    else if (Meaning.MeaninglyEquals(v1, v2, DataMeaning))
                    {
                        continue;
                    }


                    return false;
                }
            }

            return true;
        }

        private static bool ListLikeItemsAreEqual(IEnumerable l1, IEnumerable l2, object meaning)
        {
            var enum1 = l1.GetEnumerator();
            var enum2 = l2.GetEnumerator();
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    var v1 = enum1.Current;
                    var v2 = enum2.Current;

                    if (v1?.Equals(v2) ?? v2 == null)
                    {
                        continue;
                    }
                    if (Meaning.MeaninglyEquals(v1, v2, meaning))
                    {
                        continue;
                    }

                    return false;

                }
            }
            return true;
        }

        private static void EnsureMatchingTypeRequirement<T1, T2>(T1 item1, T2 item2)
        {
            throw new IncompatibleMeaningException($"Cannot meaningfully compare {item1.GetType().Name} to {item2.GetType().Name} when requireMatchingTypes is true)");
        }

        private static void EnsureVacuousComparisonRequirement<T1, T2>(T1 Item1, T2 Item2, object DataMeaning)
        {
            var item1Properties = GetMeaningfulProperties(Item1, DataMeaning);
            var item2Properties = GetMeaningfulProperties(Item2, DataMeaning);

            var item1err = item1Properties.Count() > 0 ? null : Environment.NewLine + $"Type {typeof(T1).Name} has no {DataMeaning} meaning fields";
            var item2err = item2Properties.Count() > 0 ? null : Environment.NewLine + $"Type {typeof(T2).Name} has no {DataMeaning} meaning fields";

            if (!string.IsNullOrEmpty(item1err) || !string.IsNullOrEmpty(item2err))
                throw new IncompatibleMeaningException($"Cannot meaningfully compare {typeof(T1).Name} to {typeof(T2).Name} by {DataMeaning} meaning." + item1err + item2err);

        }

        private static void EnsureDataSetComparisonRequirement<T1, T2>(T1 Item1, T2 Item2, object DataMeaning, bool subsetting)
        {
            var item1Properties = GetMeaningfulProperties(Item1, DataMeaning);
            var item2Properties = GetMeaningfulProperties(Item2, DataMeaning);

            using (var enum1 = item1Properties.GetEnumerator())
            using (var enum2 = item2Properties.GetEnumerator())
            {
                var enum1HasMore = true;
                var enum2HasMore = true;
                while ((enum1HasMore = enum1.MoveNext()) & (enum2HasMore = enum2.MoveNext()))
                {
                    if (enum1.Current.Name != enum2.Current.Name ||
                        enum1.Current.PropertyType != enum2.Current.PropertyType)
                    {
                        break;
                    }
                }

                if (enum2HasMore || (!subsetting && enum1HasMore))
                {
                    throw new IncompatibleMeaningException($"Cannot meaningfully compare {typeof(T1).Name} to {typeof(T2).Name} by {DataMeaning} meaning." + Environment.NewLine +
                        $"Type {typeof(T1).Name} and {typeof(T2).GetType().Name} do not share meaningful property sets." + Environment.NewLine +
                        $"Type {typeof(T1).Name} has {String.Join(",", item1Properties.Select(x => x.Name))}" + Environment.NewLine +
                        $"Type {typeof(T2).Name} has {String.Join(",", item2Properties.Select(x => x.Name))}");
                }
            }
        }

        internal static IEnumerable<PropertyInfo> GetMeaningfulProperties<T>(T Item, object DataMeaning)
        {
            return Item?.GetType().GetProperties().Where(x =>
            {
                var attr = (DataDescriptorAttribute)x.GetCustomAttributes(typeof(DataDescriptorAttribute), false).FirstOrDefault();
                return attr?.meaning.GetType() == DataMeaning?.GetType() && (attr?.meaning.Equals(DataMeaning) ?? DataMeaning == null);
            }).OrderBy(x => x.Name);
        }
    }

    public class IncompatibleMeaningException : Exception
    {
        public IncompatibleMeaningException(string message, Exception ex = null) : base(message, ex)
        {

        }
    }
}
