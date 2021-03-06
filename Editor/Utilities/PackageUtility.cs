/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Utilities
{
    public static class PackageUtility
    {
        private static readonly List<RemoveRequest> _removeRequests = new List<RemoveRequest>();
        private static readonly List<Action<bool>> _removeRequestFinished = new List<Action<bool>>();

        private static bool _hasListRequest;
        private static ListRequest _listInstalledPackagesRequest;
        private static readonly List<string> _packageNamesToCheck = new List<string>();
        private static readonly List<Action<bool, bool>> _checkRequestFinished = new List<Action<bool, bool>>();


        /// <summary>
        ///     Adds a package dependency to the Project. This is the equivalent of installing a package.
        ///     <para>--- To install the latest compatible version of a package, specify only the package.</para>
        ///     <para>--- To install a git package, specify a git url</para>
        ///     <para> --- To install a local package, specify a value in the format "file:pathtopackagefolder".</para>
        ///     <para>--- To install a local tarball package, specify a value in the format "file:pathto/package-file.tgz".</para>
        ///     <para>ArgumentException is thrown if identifier is null or empty.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="success"> returns true or false based on if the package installation was successful</param>
        public static void AddPackage(string name, Action<bool> success)
        {
            var request = Client.Add(name);
            EditorApplication.update += AddPackageProgress;



            void AddPackageProgress()
            {
                if (request.IsCompleted)
                {
                    if (request.Status >= StatusCode.Failure)
                    {
                        Debug.LogError(request.Error.message);
                    }

                    success.Invoke(request.Status == StatusCode.Success);
                    EditorApplication.update -= AddPackageProgress;
                }
            }
        }

        /// <summary>
        ///     Adds a package dependency to the Project. This is the equivalent of installing a package.
        ///     <para>--- To install the latest compatible version of a package, specify only the package.</para>
        ///     <para>--- To install a git package, specify a git url</para>
        ///     <para> --- To install a local package, specify a value in the format "file:pathtopackagefolder".</para>
        ///     <para>--- To install a local tarball package, specify a value in the format "file:pathto/package-file.tgz".</para>
        ///     <para>ArgumentException is thrown if identifier is null or empty.</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="success"> returns true or false based on if the package installation was successful</param>
        public static void AddPackageAndEmbed(string name, Action<bool> success)
        {
            var request = Client.Add(name);
            EditorApplication.update += AddPackageProgress;



            void AddPackageProgress()
            {
                if (request.IsCompleted)
                {
                    if (request.Status >= StatusCode.Failure)
                    {
                        Debug.LogError(request.Error.message);
                        success.Invoke(false);
                    }
                    else
                    {
                        var packageName = request.Result.name;
                        ListRequest listRequest = Client.List(true);
                        EditorApplication.update += CheckForAddedPackageProgress;

                       void CheckForAddedPackageProgress()
                       {
                           bool packageFound = false;
                            if (listRequest.IsCompleted)
                            {
                                if (listRequest.Status == StatusCode.Success)
                                {
                                    foreach (var package in listRequest.Result)
                                    {
                                        // Only retrieve packages that are currently installed in the
                                        // project (and are neither Built-In nor already Embedded)
                                        if (package.isDirectDependency
                                         && package.source
                                         != PackageSource.BuiltIn
                                         && package.source
                                         != PackageSource.Embedded)
                                        {
                                            if (package.name.Equals(packageName))
                                            {
                                                packageFound = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (packageFound)
                                    {
                                        var embedRequest = Client.Embed(packageName);
                                        EditorApplication.update += EmbedRequestProgress;



                                        void EmbedRequestProgress()
                                        {
                                            if (embedRequest.IsCompleted)
                                            {
                                                if (embedRequest.Status == StatusCode.Success)
                                                {
                                                    Debug.Log("Embedded: " + embedRequest.Result.packageId);
                                                }
                                                else if (embedRequest.Status >= StatusCode.Failure)
                                                {
                                                    Debug.LogError(embedRequest.Error.message);
                                                }

                                                success.Invoke(request.Status == StatusCode.Success);
                                                EditorApplication.update -= EmbedRequestProgress;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError($"Could not find package: [{packageName}]");
                                        success.Invoke(false);
                                    }

                            
                                }
                                else
                                {
                                    Debug.LogError(listRequest.Error.message);
                                    success.Invoke(false);
                                }

                                EditorApplication.update -= CheckForAddedPackageProgress;

                                // Embed(targetPackage);

                            }
                        }
                    }

                 
                    EditorApplication.update -= AddPackageProgress;
                }
            }
        }

       
        public static void PrintPackageList()
        {
           

            ListRequest listRequest = Client.List(true);
            EditorApplication.update += PrintPackageListProgress;



            void PrintPackageListProgress()
            {
                if (!listRequest.IsCompleted)
                {
                    return;
                }

                foreach (var re in listRequest.Result)
                {
                    Debug.Log($"id {re.packageId} | package Name: {re.name}");
                }

                EditorApplication.update -= PrintPackageListProgress;
            }
        }


        public static void RemovePackage(string name, Action<bool> success)
        {
            var request = Client.Remove(name);
            EditorApplication.update += RemovePackageProgress;



            void RemovePackageProgress()
            {
                if (request.IsCompleted)
                {
                    if (request.Status >= StatusCode.Failure)
                    {
                        Debug.LogError(request.Error.message);
                    }

                    success.Invoke(request.Status == StatusCode.Success);
                    EditorApplication.update -= RemovePackageProgress;
                }
            }



            _removeRequestFinished.Add(success);
            _removeRequests.Add(Client.Remove(name));
        }

        /// <summary>
        ///     Lists the packages the Project depends on
        /// </summary>
        /// <param name="name">Name of package</param>
        /// <param name="successAndHasPackage">
        ///     first bool returns true if the operation was successful. The second bool returns
        ///     true if the package exists
        /// </param>
        public static void HasPackageInstalled(string name, Action<bool, bool> successAndHasPackage, bool offline = true)
        {
            if (!_hasListRequest)
            {
                _hasListRequest = true;
                _listInstalledPackagesRequest = Client.List(offline);
                EditorApplication.update += ClientListProgress;
            }

            _packageNamesToCheck.Add(name);
            _checkRequestFinished.Add(successAndHasPackage);
        }


        private static void ClientListProgress()
        {
            if (!_listInstalledPackagesRequest.IsCompleted)
            {
                return;
            }

            for (var i = 0; i < _packageNamesToCheck.Count; i++)
            {
                _checkRequestFinished[i].Invoke(_listInstalledPackagesRequest.Status == StatusCode.Success, _listInstalledPackagesRequest.Status == StatusCode.Success && _listInstalledPackagesRequest.Result.Any(e => e.name.Contains(_packageNamesToCheck[i])));
            }


            _checkRequestFinished.Clear();
            _packageNamesToCheck.Clear();
            _hasListRequest = false;

            EditorApplication.update -= ClientListProgress;
        }
    }
}
