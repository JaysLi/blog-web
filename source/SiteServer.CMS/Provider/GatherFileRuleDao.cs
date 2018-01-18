using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using BaiRong.Core;
using BaiRong.Core.Data;
using BaiRong.Core.Model.Enumerations;
using SiteServer.CMS.Model;
using SiteServer.Plugin;
using SiteServer.Plugin.Models;

namespace SiteServer.CMS.Provider
{
	public class GatherFileRuleDao : DataProviderBase
	{
        private const string SqlSelectGatherFileRule = "SELECT GatherRuleName, PublishmentSystemID, GatherUrl, Charset, LastGatherDate, IsToFile, FilePath, IsSaveRelatedFiles, IsRemoveScripts, StyleDirectoryPath, ScriptDirectoryPath, ImageDirectoryPath, NodeID, IsSaveImage, IsChecked, ContentExclude, ContentHtmlClearCollection, ContentHtmlClearTagCollection, ContentTitleStart, ContentTitleEnd, ContentContentStart, ContentContentEnd, ContentAttributes, ContentAttributesXML, IsAutoCreate FROM siteserver_GatherFileRule WHERE GatherRuleName = @GatherRuleName AND PublishmentSystemID = @PublishmentSystemID";

        private const string SqlSelectAllGatherFileRuleByPsId = "SELECT GatherRuleName, PublishmentSystemID, GatherUrl, Charset, LastGatherDate, IsToFile, FilePath, IsSaveRelatedFiles, IsRemoveScripts, StyleDirectoryPath, ScriptDirectoryPath, ImageDirectoryPath, NodeID, IsSaveImage, IsChecked, ContentExclude, ContentHtmlClearCollection, ContentHtmlClearTagCollection, ContentTitleStart, ContentTitleEnd, ContentContentStart, ContentContentEnd, ContentAttributes, ContentAttributesXML, IsAutoCreate FROM siteserver_GatherFileRule WHERE PublishmentSystemID = @PublishmentSystemID";

		private const string SqlSelectGatherFileRuleNameByPsId = "SELECT GatherRuleName FROM siteserver_GatherFileRule WHERE PublishmentSystemID = @PublishmentSystemID";

		private const string SqlInsertGatherFileRule = @"
INSERT INTO siteserver_GatherFileRule 
(GatherRuleName, PublishmentSystemID, GatherUrl, Charset, LastGatherDate, IsToFile, FilePath, IsSaveRelatedFiles, IsRemoveScripts, StyleDirectoryPath, ScriptDirectoryPath, ImageDirectoryPath, NodeID, IsSaveImage, IsChecked, ContentExclude, ContentHtmlClearCollection, ContentHtmlClearTagCollection, ContentTitleStart, ContentTitleEnd, ContentContentStart, ContentContentEnd, ContentAttributes, ContentAttributesXML, IsAutoCreate) VALUES (@GatherRuleName, @PublishmentSystemID, @GatherUrl, @Charset, @LastGatherDate, @IsToFile, @FilePath, @IsSaveRelatedFiles, @IsRemoveScripts, @StyleDirectoryPath, @ScriptDirectoryPath, @ImageDirectoryPath, @NodeID, @IsSaveImage, @IsChecked, @ContentExclude, @ContentHtmlClearCollection, @ContentHtmlClearTagCollection, @ContentTitleStart, @ContentTitleEnd, @ContentContentStart, @ContentContentEnd, @ContentAttributes, @ContentAttributesXML, @IsAutoCreate)";

		private const string SqlUpdateGatherFileRule = @"
UPDATE siteserver_GatherFileRule SET 
GatherUrl = @GatherUrl, Charset = @Charset, LastGatherDate = @LastGatherDate, IsToFile = @IsToFile, FilePath = @FilePath, IsSaveRelatedFiles = @IsSaveRelatedFiles, IsRemoveScripts = @IsRemoveScripts, StyleDirectoryPath = @StyleDirectoryPath, ScriptDirectoryPath = @ScriptDirectoryPath, ImageDirectoryPath = @ImageDirectoryPath, NodeID = @NodeID, IsSaveImage = @IsSaveImage, IsChecked = @IsChecked, ContentExclude = @ContentExclude, ContentHtmlClearCollection = @ContentHtmlClearCollection, ContentHtmlClearTagCollection = @ContentHtmlClearTagCollection, ContentTitleStart = @ContentTitleStart, ContentTitleEnd = @ContentTitleEnd, ContentContentStart = @ContentContentStart, ContentContentEnd = @ContentContentEnd, ContentAttributes = @ContentAttributes, ContentAttributesXML = @ContentAttributesXML, IsAutoCreate = @IsAutoCreate WHERE GatherRuleName = @GatherRuleName AND PublishmentSystemID = @PublishmentSystemID";

		private const string SqlUpdateLastGatherDate = "UPDATE siteserver_GatherFileRule SET LastGatherDate = @LastGatherDate WHERE GatherRuleName = @GatherRuleName AND PublishmentSystemID = @PublishmentSystemID";

		private const string SqlDeleteGatherFileRule = "DELETE FROM siteserver_GatherFileRule WHERE GatherRuleName = @GatherRuleName AND PublishmentSystemID = @PublishmentSystemID";

		private const string ParmGatherFileRuleName = "@GatherRuleName";
		private const string ParmPublishmentSystemId = "@PublishmentSystemID";
        private const string ParmGatherUrl = "@GatherUrl";
        private const string ParmCharset = "@Charset";
        private const string ParmLastGatherDate = "@LastGatherDate";
        private const string ParmIsToFile = "@IsToFile";
        private const string ParmFilePath = "@FilePath";
        private const string ParmIsSaveRelatedFiles = "@IsSaveRelatedFiles";
        private const string ParmIsRemoveScripts = "@IsRemoveScripts";
        private const string ParmStyleDirectoryPath = "@StyleDirectoryPath";
        private const string ParmScriptDirectoryPath = "@ScriptDirectoryPath";
        private const string ParmImageDirectoryPath = "@ImageDirectoryPath";

        private const string ParmNodeId = "@NodeID";
        private const string ParmIsSaveImage = "@IsSaveImage";
        private const string ParmIsChecked = "@IsChecked";
        private const string ParmContentExclude = "@ContentExclude";
        private const string ParmContentHtmlClearCollection = "@ContentHtmlClearCollection";
        private const string ParmContentHtmlClearTagCollection = "@ContentHtmlClearTagCollection";
		private const string ParmContentTitleStart = "@ContentTitleStart";
		private const string ParmContentTitleEnd = "@ContentTitleEnd";
		private const string ParmContentContentStart = "@ContentContentStart";
		private const string ParmContentContentEnd = "@ContentContentEnd";
        private const string ParmContentAttributes = "@ContentAttributes";
        private const string ParmContentAttributesXml = "@ContentAttributesXML";
        private const string ParmIsAutoCreate = "@IsAutoCreate";

        public void Insert(GatherFileRuleInfo gatherFileRuleInfo) 
		{
			var insertParms = new IDataParameter[]
			{
				GetParameter(ParmGatherFileRuleName, DataType.NVarChar, 50, gatherFileRuleInfo.GatherRuleName),
				GetParameter(ParmPublishmentSystemId, DataType.Integer, gatherFileRuleInfo.PublishmentSystemId),
				GetParameter(ParmGatherUrl, DataType.NVarChar, 255, gatherFileRuleInfo.GatherUrl),
				GetParameter(ParmCharset, DataType.VarChar, 50, ECharsetUtils.GetValue(gatherFileRuleInfo.Charset)),
                GetParameter(ParmLastGatherDate, DataType.DateTime, gatherFileRuleInfo.LastGatherDate),
                GetParameter(ParmIsToFile, DataType.VarChar, 18, gatherFileRuleInfo.IsToFile.ToString()),
                GetParameter(ParmFilePath, DataType.NVarChar, 255, gatherFileRuleInfo.FilePath),
                GetParameter(ParmIsSaveRelatedFiles, DataType.VarChar, 18, gatherFileRuleInfo.IsSaveRelatedFiles.ToString()),
                GetParameter(ParmIsRemoveScripts, DataType.VarChar, 18, gatherFileRuleInfo.IsRemoveScripts.ToString()),
                GetParameter(ParmStyleDirectoryPath, DataType.NVarChar, 255, gatherFileRuleInfo.StyleDirectoryPath),
                GetParameter(ParmScriptDirectoryPath, DataType.NVarChar, 255, gatherFileRuleInfo.ScriptDirectoryPath),
                GetParameter(ParmImageDirectoryPath, DataType.NVarChar, 255, gatherFileRuleInfo.ImageDirectoryPath),

                GetParameter(ParmNodeId, DataType.Integer, gatherFileRuleInfo.NodeId),
				GetParameter(ParmIsSaveImage, DataType.VarChar, 18, gatherFileRuleInfo.IsSaveImage.ToString()),
                GetParameter(ParmIsChecked, DataType.VarChar, 18, gatherFileRuleInfo.IsChecked.ToString()),
                GetParameter(ParmContentExclude, DataType.NText, gatherFileRuleInfo.ContentExclude),
				GetParameter(ParmContentHtmlClearCollection, DataType.NVarChar, 255, gatherFileRuleInfo.ContentHtmlClearCollection),
                GetParameter(ParmContentHtmlClearTagCollection, DataType.NVarChar, 255, gatherFileRuleInfo.ContentHtmlClearTagCollection),
				GetParameter(ParmContentTitleStart, DataType.NText, gatherFileRuleInfo.ContentTitleStart),
				GetParameter(ParmContentTitleEnd, DataType.NText, gatherFileRuleInfo.ContentTitleEnd),
				GetParameter(ParmContentContentStart, DataType.NText, gatherFileRuleInfo.ContentContentStart),
				GetParameter(ParmContentContentEnd, DataType.NText, gatherFileRuleInfo.ContentContentEnd),
                GetParameter(ParmContentAttributes, DataType.NText, gatherFileRuleInfo.ContentAttributes),
                GetParameter(ParmContentAttributesXml, DataType.NText, gatherFileRuleInfo.ContentAttributesXml),
                GetParameter(ParmIsAutoCreate, DataType.VarChar, 18, gatherFileRuleInfo.IsAutoCreate.ToString())
            };

            ExecuteNonQuery(SqlInsertGatherFileRule, insertParms);
		}

		public void UpdateLastGatherDate(string gatherRuleName, int publishmentSystemId)
		{
			var parms = new IDataParameter[]
			{
				GetParameter(ParmLastGatherDate, DataType.DateTime, DateTime.Now),
				GetParameter(ParmGatherFileRuleName, DataType.NVarChar, 50, gatherRuleName),
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};
							
			ExecuteNonQuery(SqlUpdateLastGatherDate, parms);
		}

		public void Update(GatherFileRuleInfo gatherFileRuleInfo) 
		{

			var updateParms = new IDataParameter[]
			{
				GetParameter(ParmGatherUrl, DataType.NVarChar, 255, gatherFileRuleInfo.GatherUrl),
				GetParameter(ParmCharset, DataType.VarChar, 50, ECharsetUtils.GetValue(gatherFileRuleInfo.Charset)),
                GetParameter(ParmLastGatherDate, DataType.DateTime, gatherFileRuleInfo.LastGatherDate),
                GetParameter(ParmIsToFile, DataType.VarChar, 18, gatherFileRuleInfo.IsToFile.ToString()),
                GetParameter(ParmFilePath, DataType.NVarChar, 255, gatherFileRuleInfo.FilePath),
                GetParameter(ParmIsSaveRelatedFiles, DataType.VarChar, 18, gatherFileRuleInfo.IsSaveRelatedFiles.ToString()),
                GetParameter(ParmIsRemoveScripts, DataType.VarChar, 18, gatherFileRuleInfo.IsRemoveScripts.ToString()),
                GetParameter(ParmStyleDirectoryPath, DataType.NVarChar, 255, gatherFileRuleInfo.StyleDirectoryPath),
                GetParameter(ParmScriptDirectoryPath, DataType.NVarChar, 255, gatherFileRuleInfo.ScriptDirectoryPath),
                GetParameter(ParmImageDirectoryPath, DataType.NVarChar, 255, gatherFileRuleInfo.ImageDirectoryPath),

                GetParameter(ParmNodeId, DataType.Integer, gatherFileRuleInfo.NodeId),
				GetParameter(ParmIsSaveImage, DataType.VarChar, 18, gatherFileRuleInfo.IsSaveImage.ToString()),
                GetParameter(ParmIsChecked, DataType.VarChar, 18, gatherFileRuleInfo.IsChecked.ToString()),
                GetParameter(ParmContentExclude, DataType.NText, gatherFileRuleInfo.ContentExclude),
				GetParameter(ParmContentHtmlClearCollection, DataType.NVarChar, 255, gatherFileRuleInfo.ContentHtmlClearCollection),
                GetParameter(ParmContentHtmlClearTagCollection, DataType.NVarChar, 255, gatherFileRuleInfo.ContentHtmlClearTagCollection),
				GetParameter(ParmContentTitleStart, DataType.NText, gatherFileRuleInfo.ContentTitleStart),
				GetParameter(ParmContentTitleEnd, DataType.NText, gatherFileRuleInfo.ContentTitleEnd),
				GetParameter(ParmContentContentStart, DataType.NText, gatherFileRuleInfo.ContentContentStart),
				GetParameter(ParmContentContentEnd, DataType.NText, gatherFileRuleInfo.ContentContentEnd),
                GetParameter(ParmContentAttributes, DataType.NText, gatherFileRuleInfo.ContentAttributes),
                GetParameter(ParmContentAttributesXml, DataType.NText, gatherFileRuleInfo.ContentAttributesXml),
				GetParameter(ParmGatherFileRuleName, DataType.NVarChar, 50, gatherFileRuleInfo.GatherRuleName),
                GetParameter(ParmIsAutoCreate, DataType.VarChar, 18, gatherFileRuleInfo.IsAutoCreate.ToString()),
                GetParameter(ParmPublishmentSystemId, DataType.Integer, gatherFileRuleInfo.PublishmentSystemId)
			};

            ExecuteNonQuery(SqlUpdateGatherFileRule, updateParms);
		}

		public void Delete(string gatherRuleName, int publishmentSystemId)
		{
			var parms = new IDataParameter[]
			{
				GetParameter(ParmGatherFileRuleName, DataType.NVarChar, 50, gatherRuleName),
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};
							
			ExecuteNonQuery(SqlDeleteGatherFileRule, parms);
		}

		public GatherFileRuleInfo GetGatherFileRuleInfo(string gatherRuleName, int publishmentSystemId)
		{
			GatherFileRuleInfo gatherFileRuleInfo = null;
			
			var parms = new IDataParameter[]
			{
				GetParameter(ParmGatherFileRuleName, DataType.NVarChar, 50, gatherRuleName),
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};
			
			using (var rdr = ExecuteReader(SqlSelectGatherFileRule, parms)) 
			{
				if (rdr.Read())
				{
				    var i = 0;
                    gatherFileRuleInfo = new GatherFileRuleInfo(GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), ECharsetUtils.GetEnumType(GetString(rdr, i++)), GetDateTime(rdr, i++), GetBool(rdr, i++), GetString(rdr, i++), GetBool(rdr, i++), GetBool(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetBool(rdr, i++), GetBool(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetBool(rdr, i));
				}
				rdr.Close();
			}

			return gatherFileRuleInfo;
		}

		public string GetImportGatherRuleName(int publishmentSystemId, string gatherRuleName)
		{
			string importGatherRuleName;
			if (gatherRuleName.IndexOf("_", StringComparison.Ordinal) != -1)
			{
				var gatherRuleNameCount = 0;
				var lastGatherRuleName = gatherRuleName.Substring(gatherRuleName.LastIndexOf("_", StringComparison.Ordinal) + 1);
				var firstGatherRuleName = gatherRuleName.Substring(0, gatherRuleName.Length - lastGatherRuleName.Length);
				try
				{
					gatherRuleNameCount = int.Parse(lastGatherRuleName);
				}
			    catch
			    {
			        // ignored
			    }
			    gatherRuleNameCount++;
				importGatherRuleName = firstGatherRuleName + gatherRuleNameCount;
			}
			else
			{
				importGatherRuleName = gatherRuleName + "_1";
			}

			var parms = new IDataParameter[]
			{
				GetParameter(ParmGatherFileRuleName, DataType.NVarChar, 50, importGatherRuleName),
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};

			using (var rdr = ExecuteReader(SqlSelectGatherFileRule, parms))
			{
				if (rdr.Read())
				{
					importGatherRuleName = GetImportGatherRuleName(publishmentSystemId, importGatherRuleName);
				}
				rdr.Close();
			}

			return importGatherRuleName;
		}

		public IEnumerable GetDataSource(int publishmentSystemId)
		{
			var parms = new IDataParameter[]
			{
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};

			var enumerable = (IEnumerable)ExecuteReader(SqlSelectAllGatherFileRuleByPsId, parms);
			return enumerable;
		}

		public ArrayList GetGatherFileRuleInfoArrayList(int publishmentSystemId)
		{
			var list = new ArrayList();

			var parms = new IDataParameter[]
			{
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};

			using (var rdr = ExecuteReader(SqlSelectAllGatherFileRuleByPsId, parms))
			{
				while (rdr.Read())
				{
				    var i = 0;
                    var gatherFileRuleInfo = new GatherFileRuleInfo(GetString(rdr, i++), GetInt(rdr, i++), GetString(rdr, i++), ECharsetUtils.GetEnumType(GetString(rdr, i++)), GetDateTime(rdr, i++), GetBool(rdr, i++), GetString(rdr, i++), GetBool(rdr, i++), GetBool(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetInt(rdr, i++), GetBool(rdr, i++), GetBool(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetString(rdr, i++), GetBool(rdr, i));

                    list.Add(gatherFileRuleInfo);
				}
				rdr.Close();
			}

			return list;
		}

		public List<string> GetGatherRuleNameArrayList(int publishmentSystemId)
		{
			var list = new List<string>();

			var parms = new IDataParameter[]
			{
				GetParameter(ParmPublishmentSystemId, DataType.Integer, publishmentSystemId)
			};

			using (var rdr = ExecuteReader(SqlSelectGatherFileRuleNameByPsId, parms)) 
			{
				while (rdr.Read()) 
				{					
					list.Add(GetString(rdr, 0));
				}
				rdr.Close();
			}

			return list;
		}

        public void OpenAuto(int publishmentSystemId, List<string> gatherRuleNameCollection)
        {
            string sql =
                $"UPDATE siteserver_GatherFileRule SET IsAutoCreate = 'True' WHERE PublishmentSystemID = {publishmentSystemId} AND GatherRuleName in ({TranslateUtils.ToSqlInStringWithQuote(gatherRuleNameCollection)})";
            ExecuteNonQuery(sql);
        }

        public void CloseAuto(int publishmentSystemId, List<string> gatherRuleNameCollection)
        {
            string sql =
                $"UPDATE siteserver_GatherFileRule SET IsAutoCreate = 'False' WHERE PublishmentSystemID = {publishmentSystemId} AND GatherRuleName in ({TranslateUtils.ToSqlInStringWithQuote(gatherRuleNameCollection)})";
            ExecuteNonQuery(sql);
        }

    }
}
