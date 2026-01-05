using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.Services
{
    public class DataService
    {
        private readonly string _dataDirectory;
        private readonly string _dtuserDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public DataService()
        {
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _dtuserDirectory = Path.Combine("D:", "woldvirtual3d", "NDEXE", "DTUSER");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_dtuserDirectory);
        }

        public void SavePCInfo(PCInfo pcInfo)
        {
            string filePath = Path.Combine(_dtuserDirectory, "pc_info.json");
            string jsonString = JsonSerializer.Serialize(pcInfo, _jsonOptions);
            File.WriteAllText(filePath, jsonString);
        }

        public PCInfo? LoadPCInfo()
        {
            string filePath = Path.Combine(_dtuserDirectory, "pc_info.json");
            if (!File.Exists(filePath))
                return null;

            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<PCInfo>(jsonString, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public void SaveUserAccount(UserAccount userAccount)
        {
            string filePath = Path.Combine(_dtuserDirectory, "user_account.json");
            string jsonString = JsonSerializer.Serialize(userAccount, _jsonOptions);
            File.WriteAllText(filePath, jsonString);
        }

        public UserAccount? LoadUserAccount()
        {
            string filePath = Path.Combine(_dtuserDirectory, "user_account.json");
            if (!File.Exists(filePath))
                return null;

            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<UserAccount>(jsonString, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public string CreatePCInfoZip(PCInfo pcInfo)
        {
            string zipFileName = $"pc_hash_{pcInfo.UniqueHash}.zip";
            string zipFilePath = Path.Combine(_dataDirectory, zipFileName);

            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                // Crear archivo de información del PC
                var pcInfoEntry = zipArchive.CreateEntry("pc_info.json");
                using (var writer = new StreamWriter(pcInfoEntry.Open()))
                {
                    string jsonString = JsonSerializer.Serialize(pcInfo, _jsonOptions);
                    writer.Write(jsonString);
                }

                // Crear archivo README
                var readmeEntry = zipArchive.CreateEntry("README.txt");
                using (var writer = new StreamWriter(readmeEntry.Open()))
                {
                    writer.WriteLine("=== WoldVirtual3D - Información del PC ===");
                    writer.WriteLine($"Hash único del PC: {pcInfo.UniqueHash}");
                    writer.WriteLine($"Fecha de registro: {pcInfo.RegistrationDate:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Placa base: {pcInfo.Motherboard}");
                    writer.WriteLine($"Procesador: {pcInfo.Processor}");
                    writer.WriteLine($"Nombre de máquina: {pcInfo.MachineName}");
                    writer.WriteLine($"Sistema operativo: {pcInfo.OSVersion}");
                    writer.WriteLine($"Tipo de sistema: {pcInfo.SystemType}");
                    writer.WriteLine();
                    writer.WriteLine("IMPORTANTE: Guarde este archivo en un lugar seguro.");
                    writer.WriteLine("Este hash es único para su PC y se requiere para el registro de usuario.");
                }
            }

            return zipFilePath;
        }

        public string CreateUserAccountZip(UserAccount userAccount)
        {
            string zipFileName = $"user_account_{userAccount.AccountHash}.zip";
            // Usar carpeta temporal
            string tempPath = Path.GetTempPath();
            string zipFilePath = Path.Combine(tempPath, zipFileName);

            // Asegurar que el archivo no exista
            if (File.Exists(zipFilePath))
            {
                try { File.Delete(zipFilePath); } catch { }
            }

            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                // Crear archivo de cuenta de usuario
                var userAccountEntry = zipArchive.CreateEntry("user_account.json");
                using (var writer = new StreamWriter(userAccountEntry.Open()))
                {
                    string jsonString = JsonSerializer.Serialize(userAccount, _jsonOptions);
                    writer.Write(jsonString);
                }

                // Crear archivo README
                var readmeEntry = zipArchive.CreateEntry("README.txt");
                using (var writer = new StreamWriter(readmeEntry.Open()))
                {
                    writer.WriteLine("=== WoldVirtual3D - Cuenta de Usuario ===");
                    writer.WriteLine($"Hash único de cuenta: {userAccount.AccountHash}");
                    writer.WriteLine($"Usuario: {userAccount.Username}");
                    writer.WriteLine($"Fecha de creación: {userAccount.CreationDate:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Avatar: {userAccount.AvatarType}");
                    writer.WriteLine($"Validado: {(userAccount.IsValidated ? "Sí" : "No")}");
                    writer.WriteLine();
                    writer.WriteLine("IMPORTANTE: Guarde este archivo en un lugar seguro.");
                    writer.WriteLine("Contiene toda la información de su cuenta de usuario.");
                }
            }

            return zipFilePath;
        }

        public bool ValidateUserCredentials(string username, string password)
        {
            var userAccount = LoadUserAccount();
            if (userAccount == null || !userAccount.IsValidated)
                return false;

            return string.Equals(userAccount.Username, username, StringComparison.OrdinalIgnoreCase) &&
                   userAccount.VerifyPassword(password);
        }

        public UserAccount? GetValidatedUserAccount(string username)
        {
            var userAccount = LoadUserAccount();
            if (userAccount == null || !userAccount.IsValidated ||
                !string.Equals(userAccount.Username, username, StringComparison.OrdinalIgnoreCase))
                return null;

            return userAccount;
        }
    }
}
