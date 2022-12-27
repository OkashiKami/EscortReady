// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using VRChat.API.Model;

namespace EscortsReady
{
    public class AssetUrlObject
    {
    }

    public class UnityPackage
    {
        public string? id;
        public DateTime created_at;
        public string? assetUrl;
        public AssetUrlObject? assetUrlObject;
        public string? unityVersion;
        public long unitySortNumber;
        public int assetVersion;
        public string? platform;
    }

    public class UnityPackageUrlObject
    {
        public string? unityPackageUrl;
    }

    public class Avatar
    {
        public string? id;
        public string? name;
        public string? description;
        public string? authorId;
        public string? authorName;
        public List<string>? tags;
        public string? assetUrl;
        public AssetUrlObject? assetUrlObject;
        public string? imageUrl;
        public string? thumbnailImageUrl;
        public ReleaseStatus releaseStatus;
        public int version;
        public bool featured;
        public List<UnityPackage>? unityPackages;
        public string? unityPackageUrl;
        public UnityPackageUrlObject? unityPackageUrlObject;
        public DateTime created_at;
        public DateTime updated_at;
    }
}