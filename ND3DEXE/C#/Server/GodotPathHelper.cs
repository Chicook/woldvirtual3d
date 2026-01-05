using System;
using System.IO;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer.Server
{
    public static class GodotPathHelper
    {
        public static string? RequestGodotPath(string projectRoot)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Seleccionar Godot.exe";
                dialog.Filter = "Ejecutable Godot|godot.exe|Todos los archivos|*.*";
                dialog.FileName = "godot.exe";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = dialog.FileName;
                    string configFile = Path.Combine(projectRoot, "godot_path.txt");
                    
                    try
                    {
                        File.WriteAllText(configFile, selectedPath);
                        System.Diagnostics.Debug.WriteLine($"Ruta de Godot guardada: {selectedPath}");
                        return selectedPath;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al guardar ruta: {ex.Message}");
                        return null;
                    }
                }
            }
            
            return null;
        }

        public static string? GetSavedGodotPath(string projectRoot)
        {
            string configFile = Path.Combine(projectRoot, "godot_path.txt");
            if (File.Exists(configFile))
            {
                string path = File.ReadAllText(configFile).Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }
    }
}

