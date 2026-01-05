using System;
using System.Collections.Generic;

namespace WoldVirtual3D.Viewer.Models
{
    /// <summary>
    /// Modelo de datos para una escena 3D
    /// </summary>
    public class SceneData
    {
        /// <summary>
        /// Ruta de la escena (formato res://)
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la escena
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de carga
        /// </summary>
        public DateTime LoadedAt { get; set; }

        /// <summary>
        /// Estado de la escena
        /// </summary>
        public SceneState State { get; set; } = SceneState.Unloaded;

        /// <summary>
        /// Metadatos adicionales de la escena
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Estados posibles de una escena
    /// </summary>
    public enum SceneState
    {
        Unloaded,
        Loading,
        Loaded,
        Error
    }
}

