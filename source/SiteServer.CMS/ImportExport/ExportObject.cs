﻿using System.Collections;
using System.Collections.Generic;
using BaiRong.Core;
using BaiRong.Core.IO;
using BaiRong.Core.IO.FileManagement;
using BaiRong.Core.Model.Enumerations;
using SiteServer.CMS.Core;
using SiteServer.CMS.ImportExport.Components;
using SiteServer.CMS.Model;
using SiteServer.CMS.Model.Enumerations;
using SiteServer.Plugin.Models;

namespace SiteServer.CMS.ImportExport
{
    public class ExportObject
    {
        private readonly PublishmentSystemInfo _publishmentSystemInfo;
        private readonly string _publishmentSystemPath;

        public ExportObject(int publishmentSystemId)
        {
            _publishmentSystemInfo = PublishmentSystemManager.GetPublishmentSystemInfo(publishmentSystemId);
            _publishmentSystemPath = PathUtils.Combine(WebConfigUtils.PhysicalApplicationPath, _publishmentSystemInfo.PublishmentSystemDir);
        }

        /// <summary>
        /// 将发布系统文件保存到站点模板中
        /// </summary>
        public void ExportFilesToSite(string siteTemplatePath, bool isSaveAll, ArrayList lowerFileSystemArrayList, bool isCreateMetadataDirectory)
        {
            DirectoryUtils.CreateDirectoryIfNotExists(siteTemplatePath);

            var publishmentSystemDirList = DataProvider.PublishmentSystemDao.GetLowerPublishmentSystemDirListThatNotIsHeadquarters();

            var fileSystems = FileManager.GetFileSystemInfoExtendCollection(PathUtility.GetPublishmentSystemPath(_publishmentSystemInfo), true);
            foreach (FileSystemInfoExtend fileSystem in fileSystems)
            {
                if (isSaveAll || lowerFileSystemArrayList.Contains(fileSystem.Name.ToLower()))
                {
                    var srcPath = PathUtils.Combine(_publishmentSystemPath, fileSystem.Name);
                    var destPath = PathUtils.Combine(siteTemplatePath, fileSystem.Name);

                    if (fileSystem.IsDirectory)
                    {
                        var isPublishmentSystemDirectory = false;

                        if (_publishmentSystemInfo.IsHeadquarters)
                        {
                            foreach (var publishmentSystemDir in publishmentSystemDirList)
                            {
                                if (StringUtils.EqualsIgnoreCase(publishmentSystemDir, fileSystem.Name))
                                {
                                    isPublishmentSystemDirectory = true;
                                }
                            }
                        }
                        if (!isPublishmentSystemDirectory && !DirectoryUtils.IsSystemDirectory(fileSystem.Name))
                        {
                            DirectoryUtils.CreateDirectoryIfNotExists(destPath);
                            DirectoryUtils.MoveDirectory(srcPath, destPath, false);
                        }
                    }
                    else
                    {
                        if (!PathUtility.IsSystemFile(fileSystem.Name))
                        {
                            FileUtils.CopyFile(srcPath, destPath);
                        }
                    }
                }
            }

            if (isCreateMetadataDirectory)
            {
                var siteTemplateMetadataPath = PathUtility.GetSiteTemplateMetadataPath(siteTemplatePath, string.Empty);
                DirectoryUtils.CreateDirectoryIfNotExists(siteTemplateMetadataPath);
            }
        }

        public void ExportFiles(string filePath)
        {
            var filesDirectoryPath = PathUtils.Combine(DirectoryUtils.GetDirectoryPath(filePath), PathUtils.GetFileNameWithoutExtension(filePath));

            DirectoryUtils.DeleteDirectoryIfExists(filesDirectoryPath);
            FileUtils.DeleteFileIfExists(filePath);

            DirectoryUtils.Copy(_publishmentSystemPath, filesDirectoryPath);

            ZipUtils.PackFiles(filePath, filesDirectoryPath);

            DirectoryUtils.DeleteDirectoryIfExists(filesDirectoryPath);
        }

        public string ExportSingleTableStyle(ETableStyle tableStyle, string tableName, int relatedIdentity)
        {
            var filePath = PathUtils.GetTemporaryFilesPath("tableStyle.zip");
            var styleDirectoryPath = PathUtils.GetTemporaryFilesPath("TableStyle");
            TableStyleIe.SingleExportTableStyles(tableStyle, tableName, _publishmentSystemInfo.PublishmentSystemId, relatedIdentity, styleDirectoryPath);
            ZipUtils.PackFiles(filePath, styleDirectoryPath);

            DirectoryUtils.DeleteDirectoryIfExists(styleDirectoryPath);

            return PathUtils.GetFileName(filePath);
        }

        public static string ExportRootSingleTableStyle(ETableStyle tableStyle, string tableName)
        {
            var filePath = PathUtils.GetTemporaryFilesPath("tableStyle.zip");
            var styleDirectoryPath = PathUtils.GetTemporaryFilesPath("TableStyle");
            TableStyleIe.SingleExportTableStyles(tableStyle, tableName, styleDirectoryPath);
            ZipUtils.PackFiles(filePath, styleDirectoryPath);

            DirectoryUtils.DeleteDirectoryIfExists(styleDirectoryPath);

            return PathUtils.GetFileName(filePath);
        }

        public void ExportConfiguration(string configurationFilePath)
        {
            var configIe = new ConfigurationIe(_publishmentSystemInfo.PublishmentSystemId, configurationFilePath);
            configIe.Export();
        }

        /// <summary>
        /// 导出网站模板至指定的文件地址
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportTemplates(string filePath)
        {
            var templateIe = new TemplateIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            templateIe.ExportTemplates();
        }

        public void ExportTemplates(string filePath, List<int> templateIdList)
        {
            var templateIe = new TemplateIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            templateIe.ExportTemplates(templateIdList);
        }


        /// <summary>
        /// 导出网站菜单显示方式至指定的文件地址
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportMenuDisplay(string filePath)
        {
            var menuDisplayIe = new MenuDisplayIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            menuDisplayIe.ExportMenuDisplay();
        }

        public void ExportTagStyle(string filePath)
        {
            var tagStyleIe = new TagStyleIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            tagStyleIe.ExportTagStyle();
        }

        public string ExportTagStyle(TagStyleInfo styleInfo)
        {
            var filePath = PathUtils.GetTemporaryFilesPath(styleInfo.StyleName + ".xml");

            FileUtils.DeleteFileIfExists(filePath);

            var tagStyleIe = new TagStyleIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            tagStyleIe.ExportTagStyle(styleInfo);

            return PathUtils.GetFileName(filePath);
        }

        /// <summary>
        /// 导出固定广告至指定的文件地址
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportAd(string filePath)
        {
            var adIe = new AdvIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            adIe.ExportAd();
        }

        /// <summary>
        /// 导出搜索引擎
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportSeo(string filePath)
        {
            var seoIe = new SeoIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            seoIe.ExportSeo();
        }

        /// <summary>
        /// 导出自定义模板语言
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportStlTag(string filePath)
        {
            var stlTagIe = new StlTagIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            stlTagIe.ExportStlTag();
        }

        /// <summary>
        /// 导出采集规则至指定的文件地址
        /// </summary>
        /// <param name="filePath"></param>
        public void ExportGatherRule(string filePath)
        {
            var gatherRuleInfoArrayList = DataProvider.GatherRuleDao.GetGatherRuleInfoArrayList(_publishmentSystemInfo.PublishmentSystemId);
            ExportGatherRule(filePath, gatherRuleInfoArrayList);
        }

        public void ExportGatherRule(string filePath, ArrayList gatherRuleInfoArrayList)
        {
            var gatherRuleIe = new GatherRuleIe(_publishmentSystemInfo.PublishmentSystemId, filePath);
            gatherRuleIe.ExportGatherRule(gatherRuleInfoArrayList);
        }

        public void ExportInput(string inputDirectoryPath)
        {
            DirectoryUtils.CreateDirectoryIfNotExists(inputDirectoryPath);

            var inputIe = new InputIe(_publishmentSystemInfo.PublishmentSystemId, inputDirectoryPath);
            var inputIdList = DataProvider.InputDao.GetInputIdList(_publishmentSystemInfo.PublishmentSystemId);
            foreach (var inputId in inputIdList)
            {
                inputIe.ExportInput(inputId);
            }
        }

        public string ExportInput(int inputId)
        {
            var directoryPath = PathUtils.GetTemporaryFilesPath("input");
            var filePath = PathUtils.GetTemporaryFilesPath("input.zip");

            FileUtils.DeleteFileIfExists(filePath);
            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);

            var inputIe = new InputIe(_publishmentSystemInfo.PublishmentSystemId, directoryPath);
            inputIe.ExportInput(inputId);

            ZipUtils.PackFiles(filePath, directoryPath);

            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);

            return PathUtils.GetFileName(filePath);
        }

        public void ExportRelatedField(string relatedFieldDirectoryPath)
        {
            DirectoryUtils.CreateDirectoryIfNotExists(relatedFieldDirectoryPath);

            var relatedFieldIe = new RelatedFieldIe(_publishmentSystemInfo.PublishmentSystemId, relatedFieldDirectoryPath);
            var relatedFieldInfoArrayList = DataProvider.RelatedFieldDao.GetRelatedFieldInfoArrayList(_publishmentSystemInfo.PublishmentSystemId);
            foreach (RelatedFieldInfo relatedFieldInfo in relatedFieldInfoArrayList)
            {
                relatedFieldIe.ExportRelatedField(relatedFieldInfo);
            }
        }

        public string ExportRelatedField(int relatedFieldId)
        {
            var directoryPath = PathUtils.GetTemporaryFilesPath("relatedField");
            var filePath = PathUtils.GetTemporaryFilesPath("relatedField.zip");

            FileUtils.DeleteFileIfExists(filePath);
            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(directoryPath);

            var relatedFieldInfo = DataProvider.RelatedFieldDao.GetRelatedFieldInfo(relatedFieldId);

            var relatedFieldIe = new RelatedFieldIe(_publishmentSystemInfo.PublishmentSystemId, directoryPath);
            relatedFieldIe.ExportRelatedField(relatedFieldInfo);

            ZipUtils.PackFiles(filePath, directoryPath);

            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);

            return PathUtils.GetFileName(filePath);
        }


        // 导出网站所有相关辅助表以及除提交表单外的所有表样式
        public void ExportTablesAndStyles(string tableDirectoryPath)
        {
            DirectoryUtils.CreateDirectoryIfNotExists(tableDirectoryPath);
            var tableIe = new AuxiliaryTableIe(tableDirectoryPath);
            var styleIe = new TableStyleIe(tableDirectoryPath);

            var publishmentSystemInfo = PublishmentSystemManager.GetPublishmentSystemInfo(_publishmentSystemInfo.PublishmentSystemId);
            var tableNameList = PublishmentSystemManager.GetAuxiliaryTableNameList(publishmentSystemInfo);

            foreach (var tableName in tableNameList)
            {
                tableIe.ExportAuxiliaryTable(tableName);
                styleIe.ExportTableStyles(publishmentSystemInfo.PublishmentSystemId, tableName);
            }

            styleIe.ExportTableStyles(publishmentSystemInfo.PublishmentSystemId, DataProvider.NodeDao.TableName);
            styleIe.ExportTableStyles(publishmentSystemInfo.PublishmentSystemId, DataProvider.PublishmentSystemDao.TableName);
        }


        /// <summary>
        /// 导出网站内容至默认的临时文件地址
        /// </summary>
        public void ExportSiteContent(string siteContentDirectoryPath, bool isSaveContents, bool isSaveAllChannels, List<int> nodeIdArrayList)
        {
            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(siteContentDirectoryPath);

            var allNodeIdList = DataProvider.NodeDao.GetNodeIdListByPublishmentSystemId(_publishmentSystemInfo.PublishmentSystemId);

            var includeNodeIdArrayList = new ArrayList();
            foreach (int nodeId in nodeIdArrayList)
            {
                var nodeInfo = NodeManager.GetNodeInfo(_publishmentSystemInfo.PublishmentSystemId, nodeId);
                var parentIdArrayList = TranslateUtils.StringCollectionToIntList(nodeInfo.ParentsPath);
                foreach (int parentId in parentIdArrayList)
                {
                    if (!includeNodeIdArrayList.Contains(parentId))
                    {
                        includeNodeIdArrayList.Add(parentId);
                    }
                }
                if (!includeNodeIdArrayList.Contains(nodeId))
                {
                    includeNodeIdArrayList.Add(nodeId);
                }
            }

            var siteContentIe = new SiteContentIe(_publishmentSystemInfo, siteContentDirectoryPath);
            foreach (int nodeId in allNodeIdList)
            {
                if (!isSaveAllChannels)
                {
                    if (!includeNodeIdArrayList.Contains(nodeId)) continue;
                }
                siteContentIe.Export(_publishmentSystemInfo.PublishmentSystemId, nodeId, isSaveContents);
            }
        }

        public void ExportMetadata(string siteTemplateName, string webSiteUrl, string description, string samplePicPath, string metadataPath)
        {
            var siteTemplateInfo = new SiteTemplateInfo
            {
                SiteTemplateName = siteTemplateName,
                PicFileName = samplePicPath,
                WebSiteUrl = webSiteUrl,
                Description = description
            };

            var xmlPath = PathUtils.Combine(metadataPath, DirectoryUtils.SiteTemplates.FileMetadata);
            Serializer.SaveAsXML(siteTemplateInfo, xmlPath);
        }


        public string ExportChannels(List<int> nodeIdList)
        {
            var filePath = PathUtils.GetTemporaryFilesPath(EBackupTypeUtils.GetValue(EBackupType.ChannelsAndContents) + ".zip");
            return ExportChannels(nodeIdList, filePath);
        }

        public string ExportChannels(List<int> nodeIdList, string filePath)
        {
            var siteContentDirectoryPath = PathUtils.Combine(DirectoryUtils.GetDirectoryPath(filePath), PathUtils.GetFileNameWithoutExtension(filePath));

            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(siteContentDirectoryPath);

            var siteContentIe = new SiteContentIe(_publishmentSystemInfo, siteContentDirectoryPath);
            var allNodeIdList = new List<int>();
            foreach (int nodeId in nodeIdList)
            {
                if (!allNodeIdList.Contains(nodeId))
                {
                    allNodeIdList.Add(nodeId);
                    var nodeInfo = NodeManager.GetNodeInfo(_publishmentSystemInfo.PublishmentSystemId, nodeId);
                    var childNodeIdList = DataProvider.NodeDao.GetNodeIdListByScopeType(nodeInfo.NodeId, EScopeType.Descendant, string.Empty, string.Empty);
                    allNodeIdList.AddRange(childNodeIdList);
                }
            }
            foreach (int nodeId in allNodeIdList)
            {
                siteContentIe.Export(_publishmentSystemInfo.PublishmentSystemId, nodeId, true);
            } 
             
            var imageUploadDirectoryPath = PathUtils.Combine(siteContentDirectoryPath, _publishmentSystemInfo.Additional.ImageUploadDirectoryName);
            DirectoryUtils.DeleteDirectoryIfExists(imageUploadDirectoryPath);
            DirectoryUtils.Copy(PathUtils.Combine(_publishmentSystemPath, _publishmentSystemInfo.Additional.ImageUploadDirectoryName), imageUploadDirectoryPath);

            var videoUploadDirectoryPath = PathUtils.Combine(siteContentDirectoryPath, _publishmentSystemInfo.Additional.VideoUploadDirectoryName);
            DirectoryUtils.DeleteDirectoryIfExists(videoUploadDirectoryPath);
            DirectoryUtils.Copy(PathUtils.Combine(_publishmentSystemPath, _publishmentSystemInfo.Additional.VideoUploadDirectoryName), videoUploadDirectoryPath);

            var fileUploadDirectoryPath = PathUtils.Combine(siteContentDirectoryPath, _publishmentSystemInfo.Additional.FileUploadDirectoryName);
            DirectoryUtils.DeleteDirectoryIfExists(fileUploadDirectoryPath);
            DirectoryUtils.Copy(PathUtils.Combine(_publishmentSystemPath, _publishmentSystemInfo.Additional.FileUploadDirectoryName), fileUploadDirectoryPath);

            Atom.Core.AtomFeed feed = AtomUtility.GetEmptyFeed();  
            var entry = AtomUtility.GetEmptyEntry();  
            AtomUtility.AddDcElement(entry.AdditionalElements, "ImageUploadDirectoryName", _publishmentSystemInfo.Additional.ImageUploadDirectoryName);
            AtomUtility.AddDcElement(entry.AdditionalElements, "VideoUploadDirectoryName", _publishmentSystemInfo.Additional.VideoUploadDirectoryName);
            AtomUtility.AddDcElement(entry.AdditionalElements, "FileUploadDirectoryName", _publishmentSystemInfo.Additional.FileUploadDirectoryName);

            feed.Entries.Add(entry);
            var uploadFolderPath = PathUtils.Combine(siteContentDirectoryPath, BackupUtility.UploadFolderName); 
            DirectoryUtils.CreateDirectoryIfNotExists(uploadFolderPath);
            var uploadFilePath = PathUtils.Combine(uploadFolderPath, BackupUtility.UploadFileName); 
            feed.Save(uploadFilePath);

            ZipUtils.PackFiles(filePath, siteContentDirectoryPath);

            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);

            return PathUtils.GetFileName(filePath);
        }

        public bool ExportContents(string filePath, int nodeId, List<int> contentIdArrayList, bool isPeriods, string dateFrom, string dateTo, ETriState checkedState)
        {
            var siteContentDirectoryPath = PathUtils.Combine(DirectoryUtils.GetDirectoryPath(filePath), PathUtils.GetFileNameWithoutExtension(filePath));

            FileUtils.DeleteFileIfExists(filePath);
            DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(siteContentDirectoryPath);

            var siteContentIe = new SiteContentIe(_publishmentSystemInfo, siteContentDirectoryPath);
            var isExport = siteContentIe.ExportContents(_publishmentSystemInfo, nodeId, contentIdArrayList, isPeriods, dateFrom, dateTo, checkedState);
            if (isExport)
            {
                ZipUtils.PackFiles(filePath, siteContentDirectoryPath);
                DirectoryUtils.DeleteDirectoryIfExists(siteContentDirectoryPath);
            }
            return isExport;
        }
    }
}
