using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comparish
{
    public enum DataDescriptors
    {
        Metadata,
        Semantic
    }

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
        public static bool MeaninglyEquals<T1,T2>(this T1 item1, T2 item2, object DataMeaning, bool requireMatchingTypes = false, bool BSubSetsA = false, bool AllowVacuouslyComparison = false)
        {
            if (requireMatchingTypes && item1.GetType() != item2.GetType())
            {
                throw new IncompatibleMeaningException($"Cannot meaningfully compare {item1.GetType().Name} to {item2.GetType().Name} by {DataMeaning} meaing when requireMatchingTypes is true)");
            }

            var item1Props = item1.GetType().GetProperties().Where(x => {
                var attr = (DataDescriptorAttribute)x.GetCustomAttributes(typeof(DataDescriptorAttribute), false).FirstOrDefault();
                return attr?.meaning.GetType() == DataMeaning?.GetType() && (attr?.meaning.Equals(DataMeaning) ?? DataMeaning == null);
            }).OrderBy(x => x.Name);

            var item2Props = item2.GetType().GetProperties().Where(x => {
                var attr = (DataDescriptorAttribute)x.GetCustomAttributes(typeof(DataDescriptorAttribute), false).FirstOrDefault();
                return attr?.meaning.GetType() == DataMeaning?.GetType() && (attr?.meaning.Equals(DataMeaning) ?? DataMeaning == null);
            }).OrderBy(x => x.Name);

            if (!AllowVacuouslyComparison)
            {
                var item1err = item1Props.Count() > 0 ? null : Environment.NewLine + $"Type {item1.GetType().Name} has no {DataMeaning} meaning fields";
                var item2err = item2Props.Count() > 0 ? null : Environment.NewLine + $"Type {item2.GetType().Name} has no {DataMeaning} meaning fields";

                if (!string.IsNullOrEmpty(item1err) || !string.IsNullOrEmpty(item2err))
                    throw new IncompatibleMeaningException($"Cannot meaningfully compare {item1.GetType().Name} to {item2.GetType().Name} by {DataMeaning} meaing." + item1err + item2err);
            }

            using (var enum1 = item1Props.GetEnumerator())
            using (var enum2 = item2Props.GetEnumerator())
            {
                var enum1HasMore = true;//enum1.MoveNext();
                var enum2HasMore = true;//enum2.MoveNext();
                while((enum1HasMore = enum1.MoveNext()) & (enum2HasMore = enum2.MoveNext()))
                {
                    if (enum1.Current.Name != enum2.Current.Name ||
                        enum1.Current.PropertyType != enum2.Current.PropertyType)
                    {
                        break;
                    }
                } //while ((enum1HasMore = enum1.MoveNext()) & (enum2HasMore = enum2.MoveNext()));

                if (enum2HasMore || (!BSubSetsA && enum1HasMore))
                {
                    throw new IncompatibleMeaningException($"Cannot meaningfully compare {item1.GetType().Name} to {item2.GetType().Name} by {DataMeaning} meaning." + Environment.NewLine +
                        $"Type {item1.GetType().Name} and {item2.GetType().Name} do not share meaningful property sets." + Environment.NewLine +
                        $"Type {item1.GetType().Name} has {String.Join(",", item1Props.Select(x => x.Name))}" + Environment.NewLine +
                        $"Type {item2.GetType().Name} has {String.Join(",", item2Props.Select(x => x.Name))}");
                }
            }

            using (var enum1 = item1Props.GetEnumerator())
            using (var enum2 = item2Props.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    var v1 = enum1.Current.GetValue(item1);
                    var v2 = enum2.Current.GetValue(item2);

                    if (!(enum1.Current.GetValue(item1)?.Equals(enum2.Current.GetValue(item2)) ?? item2 == null))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
   }

    public class IncompatibleMeaningException : Exception
    {
        public IncompatibleMeaningException(string message, Exception ex = null) : base(message, ex)
        {

        }
    }
}
