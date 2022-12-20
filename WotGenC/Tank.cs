using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace WotGenC
{
    [DataContract]
    public class Tank : IEquatable<Tank>, IComparable<Tank>
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Nom { get; set; }
        [DataMember] public Tier Te { get; set; }
        [DataMember] public string Image { get; set; }

        [DataMember] public TankType Type { get; set; }

        public Tank(string id, string nom, Tier t, string image, TankType type)
        {
            Id = id;
            Nom = nom;
            Te = t;
            Image = image;
            Type = type;
        }
        public bool Equals([AllowNull] Tank other)
        {
            if (other is null) return false;
            return GetType() == other.GetType() && string.Equals(Nom, other.Nom, StringComparison.CurrentCultureIgnoreCase);
        }

        public int CompareTo(Tank other)
        {
            //If equals to itself => 0
            if (ReferenceEquals(this, other)) return 0;
            
            //If equals to null => 1
            if (ReferenceEquals(null, other)) return 1;

            var teComparison = Te.CompareTo(other.Te);

            //If this is == 0, then it means they are the same
            if (teComparison != 0) return teComparison;
            
            //Then we compare the name
            var nomComparison = string.Compare(Nom, other.Nom, StringComparison.Ordinal);
            
            //Same goes
            return nomComparison != 0 ? nomComparison : string.Compare(Id, other.Id, StringComparison.Ordinal);
        }
    }
}
