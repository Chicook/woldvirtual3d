#ifndef DATABASE_MANAGER_H
#define DATABASE_MANAGER_H

#include <godot_cpp/classes/node.hpp>
#include <godot_cpp/core/binder_common.hpp>
#include <godot_cpp/variant/string.hpp>
#include <godot_cpp/variant/dictionary.hpp>
#include <godot_cpp/variant/array.hpp>
#include <sqlite3.h>
#include <mutex>

using namespace godot;

namespace WoldVirtual3D {

/**
 * Gestor de base de datos SQLite en C++
 * Responsabilidad: Operaciones de BD de alto rendimiento
 */
class DatabaseManager : public Node {
    GDCLASS(DatabaseManager, Node);

private:
    sqlite3* db;
    String db_path;
    std::mutex db_mutex;
    bool is_initialized;

protected:
    static void _bind_methods();

public:
    DatabaseManager();
    ~DatabaseManager();

    void _ready() override;

    // Inicialización
    bool initialize(const String& path);
    void close();

    // Operaciones de usuario
    String create_user(const String& username, const String& password_hash, const String& email = "");
    Dictionary authenticate_user(const String& username, const String& password_hash);
    bool update_last_login(const String& user_id);
    Dictionary get_user_by_id(const String& user_id);
    Dictionary get_last_active_session();

    // Operaciones de configuración
    bool save_user_settings(const String& user_id, const Dictionary& settings);
    Dictionary get_user_settings(const String& user_id);

    // Utilidades
    bool execute_query(const String& query);
    Array execute_select(const String& query);
    bool get_is_initialized() const { return is_initialized; }

private:
    void create_tables();
    String hash_password(const String& password);
    bool table_exists(const String& table_name);
};

} // namespace WoldVirtual3D

#endif // DATABASE_MANAGER_H

