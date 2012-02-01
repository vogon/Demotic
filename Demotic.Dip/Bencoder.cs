using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    internal class Bencoder
    {
        public Bencoder()
        {
            _fragments = new LinkedList<byte[]>();
            _nestingStack = new Stack<BencodingComplexType>();
        }

        public Bencoder StartDictionary()
        {
            _nestingStack.Push(BencodingComplexType.Dictionary);
            InternalWriteString("d");

            return this;
        }

        public Bencoder StartList()
        {
            _nestingStack.Push(BencodingComplexType.List);
            InternalWriteString("l");

            return this;
        }

        public Bencoder Integer(long value)
        {
            InternalWriteString("i{0}e", value);

            return this;
        }

        public Bencoder ByteString(byte[] value)
        {
            InternalWriteString("{0}:", value.Length);
            InternalWriteByteArray(value);

            return this;
        }

        public Bencoder FinishDictionary()
        {
            if (_nestingStack.Count == 0 || 
                _nestingStack.Pop() != BencodingComplexType.Dictionary)
            {
                throw new BadBencodingException("not in a dictionary");
            }

            InternalWriteString("e");

            return this;
        }

        public Bencoder FinishList()
        {
            if (_nestingStack.Count == 0 || 
                _nestingStack.Pop() != BencodingComplexType.List)
            {
                throw new BadBencodingException("not in a list");
            }

            InternalWriteString("e");

            return this;
        }

        public byte[] Encoded
        {
            get
            {
                if (_nestingStack.Count > 0)
                {
                    // nesting stack isn't empty!
                    throw new BadBencodingException("{0} unclosed frame(s)");
                }

                return ConcatArrays(_fragments);
            }
        }

        private void InternalWriteByteArray(byte[] data)
        {
            _fragments.AddLast(data);
        }

        private void InternalWriteString(string s, params object[] args)
        {
            _fragments.AddLast(Encoding.ASCII.GetBytes(string.Format(s, args: args)));
        }

        private Stack<BencodingComplexType> _nestingStack;
        private LinkedList<byte[]> _fragments;

        private static T[] ConcatArrays<T>(IEnumerable<T[]> arrs)
        {
            T[] joined = new T[arrs.Sum(arr => arr.Length)];
            int start = 0;

            foreach (T[] arr in arrs)
            {
                Array.Copy(arr, 0, joined, start, arr.Length);
                start += arr.Length;
            }

            return joined;
        }        
    }
}
