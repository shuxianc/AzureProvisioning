using System.Collections.Generic;

namespace AzureProvisioning.DAG
{
    public class Vertex<T>
    {
        /// <summary>
        /// Child Vertexes of current Vertex. (Edge direction: current -> child)
        /// </summary>
        public List<Vertex<T>> Childrens { get; private set; }

        public T Data { get; private set; }

        /// <summary>
        /// This value will be altered during topological sort
        /// </summary>
        public int ParentCount { get; set; }

        /// <summary>
        /// AddChild is used when constructing a DAG
        /// </summary>
        /// <param name="child">Child to add</param>
        public void AddChild(Vertex<T> child)
        {
            Childrens.Add(child);
            child.ParentCount++;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Payload</param>
        public Vertex(T data)
        {
            Data = data;
            Childrens = new List<Vertex<T>>();
            ParentCount = 0;
        }
    }
}
