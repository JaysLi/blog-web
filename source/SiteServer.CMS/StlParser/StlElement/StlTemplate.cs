using SiteServer.CMS.StlParser.Model;
using System.Collections.Generic;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "��ʾģ��", Description = "ͨ�� stl:template ��ǩ��ģ���ж�����ʾģ��")]
    public sealed class StlTemplate
    {
        public const string ElementName = "stl:template";

        public static SortedList<string, string> AttributeList => null;
    }
}
