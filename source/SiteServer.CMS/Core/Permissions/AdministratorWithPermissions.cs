﻿using System.Collections.Generic;
using BaiRong.Core;
using BaiRong.Core.Model.Enumerations;

namespace SiteServer.CMS.Core.Permissions
{
    public class AdministratorWithPermissions
    {
        protected string UserName;

        private string[] _roles;
        private List<string> _permissionList;
        private readonly string _rolesKey;
        private readonly string _permissionListKey;

        public AdministratorWithPermissions(string userName)
        {
            UserName = userName;
            _rolesKey = PermissionsManager.GetRolesKey(userName);
            _permissionListKey = PermissionsManager.GetPermissionListKey(userName);
        }

        public bool IsConsoleAdministrator => EPredefinedRoleUtils.IsConsoleAdministrator(Roles);

        public bool IsSystemAdministrator => EPredefinedRoleUtils.IsSystemAdministrator(Roles);

        public bool IsAdministrator => EPredefinedRoleUtils.IsAdministrator(Roles);

        public List<string> PermissionList
        {
            get
            {
                if (_permissionList == null)
                {
                    if (!string.IsNullOrEmpty(UserName) && !string.Equals(UserName, AdminManager.AnonymousUserName))
                    {
                        if (CacheUtils.Get(_permissionListKey) != null)
                        {
                            _permissionList = CacheUtils.Get(_permissionListKey) as List<string>;
                        }
                        else
                        {
                            if (EPredefinedRoleUtils.IsConsoleAdministrator(Roles))
                            {
                                _permissionList = new List<string>();
                                foreach (var permission in PermissionConfigManager.Instance.GeneralPermissions)
                                {
                                    _permissionList.Add(permission.Name);
                                }
                            }
                            else if (EPredefinedRoleUtils.IsSystemAdministrator(Roles))
                            {
                                _permissionList = new List<string>
                                {
                                    AppManager.Permissions.Settings.AdminManagement
                                };
                            }
                            else
                            {
                                _permissionList = BaiRongDataProvider.PermissionsInRolesDao.GetGeneralPermissionList(Roles);
                            }

                            CacheUtils.InsertMinutes(_permissionListKey, _permissionList, 30);
                        }
                    }
                }
                return _permissionList ?? (_permissionList = new List<string>());
            }
        }

        public string[] Roles
        {
            get
            {
                if (_roles == null)
                {
                    if (!string.IsNullOrEmpty(UserName) && !string.Equals(UserName, AdminManager.AnonymousUserName))
                    {
                        if (CacheUtils.Get(_rolesKey) != null)
                        {
                            _roles = (string[])CacheUtils.Get(_rolesKey);
                        }
                        else
                        {
                            _roles = BaiRongDataProvider.RoleDao.GetRolesForUser(UserName);
                            CacheUtils.InsertMinutes(_rolesKey, _roles, 30);
                        }
                    }
                }
                if (_roles != null && _roles.Length > 0)
                {
                    return _roles;
                }
                return new[] { EPredefinedRoleUtils.GetValue(EPredefinedRole.Administrator) };
            }
        }

        public bool IsInRole(string role)
        {
            foreach (var r in Roles)
            {
                if (role == r) return true;
            }
            return false;
        }

        public static AdministratorWithPermissions GetAnonymousUserWithPermissions()
        {
            var userWithPermissions = new AdministratorWithPermissions(AdminManager.AnonymousUserName);
            return userWithPermissions;
        }
    }
}
