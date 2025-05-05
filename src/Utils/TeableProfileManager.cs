using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PSTeable.Models;

namespace PSTeable.Utils
{
    /// <summary>
    /// Manages Teable connection profiles
    /// </summary>
    public static class TeableProfileManager
    {
        private static readonly string ProfilesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PSTeable",
            "Profiles");
            
        /// <summary>
        /// Saves a connection profile
        /// </summary>
        /// <param name="profile">The profile to save</param>
        public static void SaveProfile(TeableConnectionProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }
            
            if (string.IsNullOrEmpty(profile.Name))
            {
                throw new ArgumentException("Profile name cannot be empty", nameof(profile));
            }
            
            // Ensure the profiles directory exists
            if (!Directory.Exists(ProfilesDirectory))
            {
                Directory.CreateDirectory(ProfilesDirectory);
            }
            
            // Convert the token to an encrypted string
            string encryptedToken = EncryptToken(profile.Token);
            
            // Create a serializable version of the profile
            var serializableProfile = new
            {
                profile.Name,
                Token = encryptedToken,
                profile.BaseUrl,
                profile.CreatedAt,
                profile.LastUsed
            };
            
            // Serialize the profile to JSON
            string json = JsonSerializer.Serialize(serializableProfile, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            // Save the profile to a file
            string filePath = GetProfilePath(profile.Name);
            File.WriteAllText(filePath, json);
        }
        
        /// <summary>
        /// Gets a connection profile by name
        /// </summary>
        /// <param name="name">The name of the profile</param>
        /// <returns>The connection profile, or null if not found</returns>
        public static TeableConnectionProfile GetProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Profile name cannot be empty", nameof(name));
            }
            
            string filePath = GetProfilePath(name);
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            try
            {
                // Read the profile from the file
                string json = File.ReadAllText(filePath);
                
                // Deserialize the profile
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                
                // Create a new profile
                var profile = new TeableConnectionProfile
                {
                    Name = root.GetProperty("Name").GetString(),
                    BaseUrl = root.GetProperty("BaseUrl").GetString(),
                    CreatedAt = root.GetProperty("CreatedAt").GetDateTime(),
                    LastUsed = root.GetProperty("LastUsed").GetDateTime()
                };
                
                // Decrypt the token
                string encryptedToken = root.GetProperty("Token").GetString();
                profile.Token = DecryptToken(encryptedToken);
                
                return profile;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load profile '{name}': {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets all connection profiles
        /// </summary>
        /// <returns>A list of connection profiles</returns>
        public static List<TeableConnectionProfile> GetAllProfiles()
        {
            if (!Directory.Exists(ProfilesDirectory))
            {
                return new List<TeableConnectionProfile>();
            }
            
            var profiles = new List<TeableConnectionProfile>();
            
            foreach (string filePath in Directory.GetFiles(ProfilesDirectory, "*.json"))
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(filePath);
                    var profile = GetProfile(name);
                    if (profile != null)
                    {
                        profiles.Add(profile);
                    }
                }
                catch
                {
                    // Skip profiles that fail to load
                }
            }
            
            return profiles;
        }
        
        /// <summary>
        /// Removes a connection profile
        /// </summary>
        /// <param name="name">The name of the profile to remove</param>
        /// <returns>True if the profile was removed, false if it was not found</returns>
        public static bool RemoveProfile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Profile name cannot be empty", nameof(name));
            }
            
            string filePath = GetProfilePath(name);
            if (!File.Exists(filePath))
            {
                return false;
            }
            
            File.Delete(filePath);
            return true;
        }
        
        /// <summary>
        /// Updates the last used timestamp for a profile
        /// </summary>
        /// <param name="name">The name of the profile</param>
        public static void UpdateLastUsed(string name)
        {
            var profile = GetProfile(name);
            if (profile != null)
            {
                profile.LastUsed = DateTime.Now;
                SaveProfile(profile);
            }
        }
        
        /// <summary>
        /// Gets the file path for a profile
        /// </summary>
        /// <param name="name">The name of the profile</param>
        /// <returns>The file path</returns>
        private static string GetProfilePath(string name)
        {
            // Sanitize the name to ensure it's a valid filename
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            
            return Path.Combine(ProfilesDirectory, $"{name}.json");
        }
        
        /// <summary>
        /// Encrypts a token
        /// </summary>
        /// <param name="token">The token to encrypt</param>
        /// <returns>The encrypted token</returns>
        private static string EncryptToken(SecureString token)
        {
            if (token == null)
            {
                return null;
            }
            
            // Convert the secure string to a byte array
            byte[] tokenBytes = SecureStringToBytes(token);
            
            try
            {
                // Use DPAPI to encrypt the token
                byte[] encryptedBytes = ProtectedData.Protect(
                    tokenBytes,
                    null,
                    DataProtectionScope.CurrentUser);
                
                // Convert the encrypted bytes to a base64 string
                return Convert.ToBase64String(encryptedBytes);
            }
            finally
            {
                // Clear the token bytes
                Array.Clear(tokenBytes, 0, tokenBytes.Length);
            }
        }
        
        /// <summary>
        /// Decrypts a token
        /// </summary>
        /// <param name="encryptedToken">The encrypted token</param>
        /// <returns>The decrypted token</returns>
        private static SecureString DecryptToken(string encryptedToken)
        {
            if (string.IsNullOrEmpty(encryptedToken))
            {
                return null;
            }
            
            try
            {
                // Convert the base64 string to bytes
                byte[] encryptedBytes = Convert.FromBase64String(encryptedToken);
                
                // Use DPAPI to decrypt the token
                byte[] tokenBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser);
                
                // Convert the bytes to a secure string
                return BytesToSecureString(tokenBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to decrypt token", ex);
            }
        }
        
        /// <summary>
        /// Converts a secure string to a byte array
        /// </summary>
        /// <param name="secureString">The secure string</param>
        /// <returns>The byte array</returns>
        private static byte[] SecureStringToBytes(SecureString secureString)
        {
            if (secureString == null)
            {
                return null;
            }
            
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(secureString);
                string plainText = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
                return Encoding.UTF8.GetBytes(plainText);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
                }
            }
        }
        
        /// <summary>
        /// Converts a byte array to a secure string
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The secure string</returns>
        private static SecureString BytesToSecureString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            
            var secureString = new SecureString();
            string plainText = Encoding.UTF8.GetString(bytes);
            
            foreach (char c in plainText)
            {
                secureString.AppendChar(c);
            }
            
            secureString.MakeReadOnly();
            return secureString;
        }
    }
}

