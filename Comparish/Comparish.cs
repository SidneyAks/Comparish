using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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

    static class Meaning
    {
        public static bool MeaninglyEquals<T1, T2>(T1 item1, T2 item2, object DataMeaning,
            bool requireMatchingTypes = false, bool AllowVacuouslyComparison = false,
            bool BSubSetsA = false)
        {
            return MeaninglyEquals(item1, item2, DataMeaning, requireMatchingTypes, AllowVacuouslyComparison, BSubSetsA);
        }

        public static bool MeaninglyEquals<T1, T2>(T1 item1, T2 item2, object DataMeaning, out Dictionary<string,Comparishon> propertyMatches,
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

            propertyMatches = new Dictionary<string, Comparishon>();

            using (var enum1 = item1Props.GetEnumerator())
            using (var enum2 = item2Props.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    var v1 = enum1.Current.GetValue(item1);
                    var v2 = enum2.Current.GetValue(item2);

                    propertyMatches[enum1.Current.Name] = new Comparishon(v1, v2, DataMeaning, enum1.Current.Name);
                }
            }

            return propertyMatches.Values.All(x => x.Matches);
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

    [DebuggerDisplay("{DebuggerDisplay}")]
    [JsonConverter(typeof(DirectPropertySerializer))]
    public class Comparishon
    {
        [Flags]
        public enum SummaryOutputDisplays
        {
            OnlyMismatch = 0
            
        }

        public string JsonSummarization(SummaryOutputDisplays OutputDisplay = SummaryOutputDisplays.OnlyMismatch, bool IndentOutput = true)
        {
            switch (OutputDisplay)
            {
                case SummaryOutputDisplays.OnlyMismatch:
                    return JsonConvert.SerializeObject(this, IndentOutput ? Formatting.Indented : Formatting.None);
            }
            throw new Exception("How the hell did you even get here?");
        }

        private string DebuggerDisplay => $"{FieldName}{MatchStatement}";

        public bool Matches
        {
            get
            {
                try
                {
                    if (matches == null)
                    {

                        LowLevelNoMatch = true;
                        if (v1?.Equals(v2) ?? v2 == null)
                        {
                            return (matches = true).Value;
                        }

                        if (v1 is string || v2 is string)
                        {
                            return (matches = false).Value;
                        }

                        if (v1 is IEnumerable l1 && v2 is IEnumerable l2)
                        {
                            if (ListLikeItemsAreEqual(l1, l2, DataMeaning))
                            {
                                return (matches = true).Value;
                            }
                        }

                        LowLevelNoMatch = false;
                        if (Meaning.MeaninglyEquals(v1, v2, DataMeaning, out _childrenEvaluations, BSubSetsA: AllowPropertySubsetting))
                        {
                            return (matches = true).Value;
                        }

                        return (matches = false).Value;
                    }

                    return matches.Value;
                }
                catch (Exception ex)
                {
                    this.ComparisonException = ex;
                    return (matches = false).Value;
                }

            }
        }
        private bool? matches { get; set; }

        public object v1 { get; }
        public object v2 { get; }

        private string MatchStatement => $"{(Matches ? "Match" : $"No Match : {{{v1},{v2}}}")}";

        internal object Serialization
        {
            get
            {
                if (Matches) return "Matches";
                if (!LowLevelNoMatch) return ChildrenEvaluations;
                if (LowLevelNoMatch) return $"No Match : {{{v1},{v2}}}";
                return "I really don't know what happened here, this should be an impossible state. Please file a bug";
            }
        }

        public string FieldName { get; }
        public object DataMeaning { get; }

        public bool AllowPropertySubsetting { get; }

        public Dictionary<string, Comparishon> ChildrenEvaluations {
            get
            {
                if (_childrenEvaluations == null)
                {
                    var foo = Matches;
                }
                return _childrenEvaluations;
            }
        }
        private Dictionary<string, Comparishon> _childrenEvaluations;


        public Comparishon(object v1, object v2, object DataMeaning, string FieldName = null,
            bool AllowPropertySubsetting = false)
        {
            this.FieldName = FieldName;
            this.v1 = v1;
            this.v2 = v2;
            this.DataMeaning = DataMeaning;

            this.AllowPropertySubsetting = AllowPropertySubsetting;
        }


        public bool GetMatchDetails(out Dictionary<string, Comparishon> Evaluations)
        {
            Evaluations = ChildrenEvaluations;
            return Matches;
        }

        private bool LowLevelNoMatch;

        private Exception ComparisonException { get; set; }

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
                    if (Meaning.MeaninglyEquals(v1, v2, meaning, out var _))
                    {
                        continue;
                    }

                    return false;

                }
            }
            return true;
        }

        public static implicit operator bool(Comparishon c)
        {
            return c.Matches;
        }
    }

    public class DirectPropertySerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var name = value as Comparishon;
            serializer.Serialize(writer, name.Serialization);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
