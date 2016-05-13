using System.Linq.Expressions;

namespace Breadbox
{
    public class VideoBuffer
    {
        private readonly Expression _hblank;
        private readonly Expression _vblank;
        private readonly int[] _buffer;
        private readonly Expression _bufferExpression;
        private int _bufferPointer;
        private readonly Expression _bufferPointerExpression;

        public VideoBuffer(Expression hblank, Expression vblank, int width, int height)
        {
            _hblank = hblank;
            _vblank = vblank;
            _buffer = new int[width * height];
            _bufferExpression = Expression.Constant(_buffer);
            _bufferPointerExpression = Util.Member(() => _bufferPointer);
        }

        public Expression Write(Expression pixel)
        {
            var writeIndex = Expression.ArrayAccess(_bufferExpression,
                Expression.PostIncrementAssign(_bufferPointerExpression));
            var writeExpression = Expression.Assign(writeIndex, pixel);
            var verticalResetExpression = Expression.Assign(_bufferPointerExpression, Expression.Constant(0));
            return Expression.IfThenElse(_vblank, verticalResetExpression, Expression.IfThen(Expression.Not(_hblank), writeExpression));
        }
    }
}
