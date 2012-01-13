using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal struct XmlTimeSpan : IEquatable<XmlTimeSpan>
    {
        private readonly bool _positive;
        private readonly int _years;
        private readonly int _months;
        private readonly int _days;
        private readonly int _hours;
        private readonly int _minutes;
        private readonly double _seconds;

        public XmlTimeSpan(bool positive, int years, int months, int days, int hours, int minutes, double seconds)
        {
            _positive = positive;
            _years = years;
            _months = months;
            _days = days;
            _hours = hours;
            _minutes = minutes;
            _seconds = seconds;
        }

        public bool Positive { get { return _positive; } }

        public double Seconds { get { return _seconds; } }

        public int Minutes { get { return _minutes; } }

        public int Hours { get { return _hours; } }

        public int Days { get { return _days; } }

        public int Months { get { return _months; } }

        public int Years { get { return _years; } }

        public override bool Equals(object obj)
        {
            if (!(obj is XmlTimeSpan))
                return false;

            return Equals((XmlTimeSpan)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _positive.GetHashCode();
                result = (result << 5 + result) ^ _years.GetHashCode();
                result = (result << 5 + result) ^ _months.GetHashCode();
                result = (result << 5 + result) ^ _days.GetHashCode();
                result = (result << 5 + result) ^ _hours.GetHashCode();
                result = (result << 5 + result) ^ _minutes.GetHashCode();
                result = (result << 5 + result) ^ _seconds.GetHashCode();

                return result;
            }
        }

        public bool Equals(XmlTimeSpan other)
        {
            return
                _positive == other._positive &&
                _years == other._years &&
                _months == other._months &&
                _days == other._days &&
                _hours == other._hours &&
                _minutes == other._minutes &&
                _seconds == other._seconds;
        }

        public static bool operator ==(XmlTimeSpan a, XmlTimeSpan b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(XmlTimeSpan a, XmlTimeSpan b)
        {
            return !(a == b);
        }
    }
}
