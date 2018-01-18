using System.Collections.Generic;
using SiteServer.CMS.StlParser.Model;

namespace SiteServer.CMS.StlParser.StlElement
{
    [Stl(Usage = "����ģ��", Description = "ͨ�� stl:loading ��ǩ��ģ���д�����������ʾ������")]
    public sealed class StlLoading
    {
        public const string ElementName = "stl:loading";

        public static SortedList<string, string> AttributeList => null;
    }
}
