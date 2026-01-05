using System;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer.Models;

namespace WoldVirtual3D.Viewer.Services
{
    /// <summary>
    /// Gestor de escenas del metaverso
    /// Responsabilidad: Cargar, cambiar y gestionar escenas 3D
    /// </summary>
    public class SceneManager
    {
        private string? currentScenePath;
        private SceneData? currentScene;

        /// <summary>
        /// Carga una escena de forma asíncrona
        /// </summary>
        public async Task<bool> LoadSceneAsync(GodotService godotService, string scenePath)
        {
            if (godotService == null)
                return false;

            try
            {
                // Validar que la escena existe
                if (!ValidateScenePath(scenePath))
                {
                    return false;
                }

                // Cargar datos de la escena
                currentScene = new SceneData
                {
                    Path = scenePath,
                    Name = System.IO.Path.GetFileNameWithoutExtension(scenePath),
                    LoadedAt = DateTime.Now
                };

                currentScenePath = scenePath;

                // Aquí se enviaría el comando a Godot para cargar la escena
                // Por ahora simulamos la carga
                await Task.Delay(500);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar escena: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Valida que la ruta de la escena sea correcta
        /// </summary>
        private bool ValidateScenePath(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                return false;

            // Las rutas de Godot deben empezar con res://
            if (!scenePath.StartsWith("res://"))
                return false;

            return true;
        }

        /// <summary>
        /// Obtiene la escena actual
        /// </summary>
        public SceneData? GetCurrentScene()
        {
            return currentScene;
        }

        /// <summary>
        /// Cambia a una nueva escena
        /// </summary>
        public async Task<bool> ChangeSceneAsync(GodotService godotService, string newScenePath)
        {
            if (godotService == null)
                return false;

            // Descargar escena actual si existe
            if (currentScene != null)
            {
                await UnloadCurrentSceneAsync();
            }

            // Cargar nueva escena
            return await LoadSceneAsync(godotService, newScenePath);
        }

        /// <summary>
        /// Descarga la escena actual
        /// </summary>
        private async Task UnloadCurrentSceneAsync()
        {
            if (currentScene == null)
                return;

            // Limpiar recursos de la escena
            currentScene = null;
            currentScenePath = null;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Verifica si hay una escena cargada
        /// </summary>
        public bool HasSceneLoaded()
        {
            return currentScene != null;
        }
    }
}

