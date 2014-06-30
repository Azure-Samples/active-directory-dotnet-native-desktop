using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TodoListClient
{
 
    // TODO OVERRIDE Clear()
    class FileCache : TokenCache
    {
        public string CacheFilePath;
        private static readonly object FileLock = new object();

        public FileCache(string filePath=@".\TokenCache.dat")
        {
            CacheFilePath = filePath;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
        }

         void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                this.Deserialize(File.Exists(CacheFilePath) ?  ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),null,DataProtectionScope.CurrentUser) : null);
            }
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                lock (FileLock)
                {                    
                    File.WriteAllBytes(CacheFilePath, ProtectedData.Protect(this.Serialize(),null,DataProtectionScope.CurrentUser));
                }
            }
        }
    }

}
