using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WotGenC
{
    [CollectionDataContract]
    public class ListOfTanks : List<Tank>
    {
    }
}