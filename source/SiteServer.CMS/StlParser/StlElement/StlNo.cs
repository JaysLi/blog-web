using SiteServer.CMS.StlParser.Model;
using System.Collections.Generic;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "ʧ��ģ��", Description = "ͨ�� stl:no ��ǩ��ģ������ʾʧ��ģ��")]
    public sealed class StlNo
    {
        public const string ElementName = "stl:no";
        public const string ElementName2 = "stl:failuretemplate";

        public static SortedList<string, string> AttributeList => null;
    }
}
