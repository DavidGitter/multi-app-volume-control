using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ThreadSafeBool
{
    private bool value = false;
    private object lockObject = new object();

    public bool Value
    {
        get
        {
            lock (lockObject)
            {
                return value;
            }
        }
        set
        {
            lock (lockObject)
            {
                this.value = value;
            }
        }
    }
}