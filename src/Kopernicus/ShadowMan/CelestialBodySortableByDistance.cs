using System;
using System.Collections.Generic;
namespace scatterer
{
    public class celestialBodySortableByDistance : IEquatable<celestialBodySortableByDistance>, IComparable<celestialBodySortableByDistance>
    {

        public CelestialBody CelestialBody { get; set; }

        public float Distance { get; set; }

        public bool usesScatterer { get; set; }
        public int scattererIndex { get; set; }

        public override string ToString()
        {
            return "dist: " + Distance + "   CelestialBodyName: " + CelestialBody.name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            celestialBodySortableByDistance objAsCelestialBodySortableByDistance = obj as celestialBodySortableByDistance;
            if (objAsCelestialBodySortableByDistance == null) return false;
            else return Equals(objAsCelestialBodySortableByDistance);
        }

        public int SortByNameAscending(string name1, string name2)
        {

            return name1.CompareTo(name2);
        }

        // Default comparer for Part type.
        public int CompareTo(celestialBodySortableByDistance comparecelestialBodySortableByDistance)
        {
            // A null value means that this object is greater.
            if (comparecelestialBodySortableByDistance == null)
                return 1;

            else
                return this.Distance.CompareTo(comparecelestialBodySortableByDistance.Distance);
        }

        public override int GetHashCode()
        {
            return (int)Distance;
        }

        public bool Equals(celestialBodySortableByDistance other)
        {
            if (other == null) return false;
            return (this.Distance.Equals(other.Distance));
        }
        // Should also override == and != operators.

    }
}
