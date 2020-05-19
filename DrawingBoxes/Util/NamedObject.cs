using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VaporDAW
{
    public class NamedObject<T>
    {
        public T Object { get; set; }
        public string Name { get; set; }

        public NamedObject(T obj)
        {
            this.Object = obj;
            this.Name = obj?.ToString();
        }

        public NamedObject(T obj, string name)
        {
            this.Object = obj;
            this.Name = name;
        }
    }
}
