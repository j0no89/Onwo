using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onwo
{
    public sealed class Bool
    {
        private static readonly Bool _true = new Bool(true);
        private static readonly Bool _false = new Bool(false);
        public static Bool True => _true;
        public static Bool False => _false;
        private Bool(bool value)
        {
            Value = value;
        }
        public bool Value { get; }

        public static Bool operator !(Bool @bool)
        {
            return ReferenceEquals(@bool, Bool.True)
                ? Bool.False
                : Bool.True;
        }

        public static implicit operator bool(Bool b)
            => b.Value;

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(IFormatProvider formatProvider)
            => Value.ToString(formatProvider);
    }
}
