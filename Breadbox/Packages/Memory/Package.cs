using System.Linq.Expressions;

namespace Breadbox.Packages.Memory
{
    public abstract class Package
    {
        private readonly int[] _ram;
        private readonly Expression _ramExpression;
        private readonly Expression _valueMask;
        private readonly Expression _addressMask;

        protected Package(int addressBits, int dataBits)
        {
            var valueMask = (1 << dataBits) - 1;
            var capacity = (1 << addressBits);
            var addressMask = capacity - 1;

            _valueMask = Expression.Constant(valueMask);
            _addressMask = Expression.Constant(addressMask);
            _ram = new int[capacity];
            _ramExpression = Util.Member(() => _ram);
        }

        public Expression Read(Expression address)
        {
            return ReadUnchecked(Expression.And(address, _addressMask));
        }

        public Expression ReadUnchecked(Expression address)
        {
            return Expression.ArrayAccess(_ramExpression, address);
        }

        public Expression Write(Expression address, Expression value)
        {
            return WriteUnchecked(Expression.And(address, _addressMask), Expression.And(value, _valueMask));
        }

        public Expression WriteUnchecked(Expression address, Expression value)
        {
            return Expression.Assign(Expression.ArrayAccess(_ramExpression, address), value);

        }
    }
}
