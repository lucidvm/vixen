using System.IO;
using System.Xml.Serialization;

namespace LucidVM.Vixen
{
    // old xml serdes convenience functions from LunaVM
    // nothing really wrong with them, may as well use em
    static class XML
    {
        public static T Read<T>(string xml) where T : class
        {
            XmlSerializer serial = new XmlSerializer(typeof(T));
            StringReader read = new StringReader(xml);
            return serial.Deserialize(read) as T;
        }

        public static T Read<T>(string xml, string root) where T : class
        {
            XmlRootAttribute rootatt = new XmlRootAttribute(root);
            XmlSerializer serial = new XmlSerializer(typeof(T), rootatt);
            StringReader read = new StringReader(xml);
            return serial.Deserialize(read) as T;
        }

        public static string Write<T>(T obj) where T : class
        {
            XmlSerializer serial = new XmlSerializer(typeof(T));
            StringWriter write = new StringWriter();
            serial.Serialize(write, obj);
            return write.GetStringBuilder().ToString();
        }
    }
}
