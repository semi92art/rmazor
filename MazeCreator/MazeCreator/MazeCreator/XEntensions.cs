using System.Xml.Linq;

namespace MazeCreator
{
    public static class XEntensions
    {
        public static XName ToXName(this string _LocalName)
        {
            return XName.Get(_LocalName, "http://www.w3.org/2000/svg");
        }
    }
}