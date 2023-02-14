using System;
using System.Collections.Generic;
using System.Text;

namespace Rowbot
{
    public abstract class RowTarget : IDisposable
    {
        protected bool Completed { get; private set; } = false;
        protected bool Initialized { get; private set; } = false;

        public void Complete()
        {
            if (!Initialized)
                throw new InvalidOperationException("Init must be called before Complete()");
            if (Completed)
                throw new InvalidOperationException("Complete already called and can only be called once.");
            Completed = true;
            OnComplete();
        }

        public abstract void Dispose();

        public void Init(ColumnInfo[] columns)
        {
            if (columns is null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (Initialized)
                throw new InvalidOperationException("Init has already been called and can only be called once.");
            Initialized = true;
            OnInit(columns);
        }

        public void WriteRow(object[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (!Initialized)
                throw new InvalidOperationException("Init must be called before WriteRows");
            if (Completed)
                throw new InvalidOperationException("Complete already called. Not allowed to write more rows");
            OnWriteRow(values);
        }

        /// <summary>
        ///  Guaranteed to only be called once and never be called before init.
        /// </summary>
        protected abstract void OnComplete();
        /// <summary>
        /// Guaraneteed to only be called once.
        /// </summary>
        /// <param name="columns"></param>
        protected abstract void OnInit(ColumnInfo[] columns);
        /// <summary>
        /// Guaranteed to never be called before init or after completed.
        /// </summary>
        /// <param name="values"></param>
        protected abstract void OnWriteRow(object[] values);
    }
}
