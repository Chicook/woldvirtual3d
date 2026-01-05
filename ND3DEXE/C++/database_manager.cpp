#include "database_manager.h"
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/utility_functions.hpp>
#include <godot_cpp/variant/string.hpp>
#include <godot_cpp/variant/json.hpp>
#include <openssl/sha.h>
#include <sstream>
#include <iomanip>
#include <ctime>

using namespace godot;

namespace WoldVirtual3D {

void DatabaseManager::_bind_methods() {
    ClassDB::bind_method(D_METHOD("initialize", "path"), &DatabaseManager::initialize);
    ClassDB::bind_method(D_METHOD("close"), &DatabaseManager::close);
    ClassDB::bind_method(D_METHOD("create_user", "username", "password_hash", "email"), &DatabaseManager::create_user);
    ClassDB::bind_method(D_METHOD("authenticate_user", "username", "password_hash"), &DatabaseManager::authenticate_user);
    ClassDB::bind_method(D_METHOD("update_last_login", "user_id"), &DatabaseManager::update_last_login);
    ClassDB::bind_method(D_METHOD("get_user_by_id", "user_id"), &DatabaseManager::get_user_by_id);
    ClassDB::bind_method(D_METHOD("get_last_active_session"), &DatabaseManager::get_last_active_session);
    ClassDB::bind_method(D_METHOD("save_user_settings", "user_id", "settings"), &DatabaseManager::save_user_settings);
    ClassDB::bind_method(D_METHOD("get_user_settings", "user_id"), &DatabaseManager::get_user_settings);
    ClassDB::bind_method(D_METHOD("execute_query", "query"), &DatabaseManager::execute_query);
    ClassDB::bind_method(D_METHOD("execute_select", "query"), &DatabaseManager::execute_select);
    ClassDB::bind_method(D_METHOD("get_is_initialized"), &DatabaseManager::get_is_initialized);

    ADD_PROPERTY(PropertyInfo(Variant::BOOL, "is_initialized", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_READ_ONLY), "", "get_is_initialized");
}

DatabaseManager::DatabaseManager() {
    db = nullptr;
    is_initialized = false;
}

DatabaseManager::~DatabaseManager() {
    close();
}

void DatabaseManager::_ready() {
    UtilityFunctions::print("DatabaseManager: Inicializado");
}

bool DatabaseManager::initialize(const String& path) {
    std::lock_guard<std::mutex> lock(db_mutex);

    if (is_initialized && db != nullptr) {
        UtilityFunctions::print("DatabaseManager: Base de datos ya inicializada");
        return true;
    }

    db_path = path;
    CharString path_utf8 = path.utf8();

    int rc = sqlite3_open(path_utf8.get_data(), &db);
    if (rc != SQLITE_OK) {
        UtilityFunctions::printerr("DatabaseManager: Error al abrir BD: ", sqlite3_errmsg(db));
        sqlite3_close(db);
        db = nullptr;
        return false;
    }

    create_tables();
    is_initialized = true;
    UtilityFunctions::print("DatabaseManager: Base de datos inicializada: ", path);
    return true;
}

void DatabaseManager::close() {
    std::lock_guard<std::mutex> lock(db_mutex);

    if (db != nullptr) {
        sqlite3_close(db);
        db = nullptr;
        is_initialized = false;
        UtilityFunctions::print("DatabaseManager: Base de datos cerrada");
    }
}

void DatabaseManager::create_tables() {
    if (db == nullptr) return;

    const char* create_users = R"(
        CREATE TABLE IF NOT EXISTS users (
            id TEXT PRIMARY KEY,
            username TEXT UNIQUE NOT NULL,
            password_hash TEXT NOT NULL,
            email TEXT,
            created_at TEXT NOT NULL,
            last_login TEXT,
            is_active INTEGER DEFAULT 1
        )
    )";

    const char* create_sessions = R"(
        CREATE TABLE IF NOT EXISTS sessions (
            id TEXT PRIMARY KEY,
            user_id TEXT NOT NULL,
            username TEXT NOT NULL,
            last_login TEXT NOT NULL,
            FOREIGN KEY (user_id) REFERENCES users(id)
        )
    )";

    const char* create_settings = R"(
        CREATE TABLE IF NOT EXISTS user_settings (
            user_id TEXT PRIMARY KEY,
            avatar_data TEXT,
            preferences TEXT,
            world_id TEXT,
            FOREIGN KEY (user_id) REFERENCES users(id)
        )
    )";

    char* err_msg = nullptr;
    
    if (sqlite3_exec(db, create_users, nullptr, nullptr, &err_msg) != SQLITE_OK) {
        UtilityFunctions::printerr("DatabaseManager: Error al crear tabla users: ", err_msg);
        sqlite3_free(err_msg);
    }

    if (sqlite3_exec(db, create_sessions, nullptr, nullptr, &err_msg) != SQLITE_OK) {
        UtilityFunctions::printerr("DatabaseManager: Error al crear tabla sessions: ", err_msg);
        sqlite3_free(err_msg);
    }

    if (sqlite3_exec(db, create_settings, nullptr, nullptr, &err_msg) != SQLITE_OK) {
        UtilityFunctions::printerr("DatabaseManager: Error al crear tabla user_settings: ", err_msg);
        sqlite3_free(err_msg);
    }
}

String DatabaseManager::hash_password(const String& password) {
    CharString pwd_utf8 = password.utf8();
    unsigned char hash[SHA256_DIGEST_LENGTH];
    SHA256_CTX sha256;
    SHA256_Init(&sha256);
    SHA256_Update(&sha256, pwd_utf8.get_data(), pwd_utf8.length());
    SHA256_Final(hash, &sha256);

    std::stringstream ss;
    for (int i = 0; i < SHA256_DIGEST_LENGTH; i++) {
        ss << std::hex << std::setw(2) << std::setfill('0') << (int)hash[i];
    }

    return String(ss.str().c_str());
}

String DatabaseManager::create_user(const String& username, const String& password_hash, const String& email) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return "";

    // Generar ID único
    std::stringstream ss;
    ss << std::time(nullptr) << "_" << std::rand();
    String user_id = String(ss.str().c_str());

    // Obtener timestamp actual
    std::time_t now = std::time(nullptr);
    char time_str[64];
    std::strftime(time_str, sizeof(time_str), "%Y-%m-%d %H:%M:%S", std::localtime(&now));
    String created_at = String(time_str);

    const char* sql = "INSERT INTO users (id, username, password_hash, email, created_at, is_active) VALUES (?, ?, ?, ?, ?, 1)";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        CharString id_utf8 = user_id.utf8();
        CharString username_utf8 = username.utf8();
        CharString hash_utf8 = password_hash.utf8();
        CharString email_utf8 = email.utf8();
        CharString created_utf8 = created_at.utf8();

        sqlite3_bind_text(stmt, 1, id_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 2, username_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 3, hash_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 4, email_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 5, created_utf8.get_data(), -1, SQLITE_STATIC);

        if (sqlite3_step(stmt) == SQLITE_DONE) {
            // Crear registro de settings
            const char* settings_sql = "INSERT INTO user_settings (user_id, preferences, world_id) VALUES (?, '{}', '')";
            sqlite3_stmt* settings_stmt;
            if (sqlite3_prepare_v2(db, settings_sql, -1, &settings_stmt, nullptr) == SQLITE_OK) {
                sqlite3_bind_text(settings_stmt, 1, id_utf8.get_data(), -1, SQLITE_STATIC);
                sqlite3_step(settings_stmt);
                sqlite3_finalize(settings_stmt);
            }

            sqlite3_finalize(stmt);
            UtilityFunctions::print("DatabaseManager: Usuario creado: ", username);
            return user_id;
        } else {
            sqlite3_finalize(stmt);
            UtilityFunctions::printerr("DatabaseManager: Error al crear usuario");
            return "";
        }
    }

    return "";
}

Dictionary DatabaseManager::authenticate_user(const String& username, const String& password_hash) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return Dictionary();

    const char* sql = "SELECT id, username, email, created_at, last_login FROM users WHERE username = ? AND password_hash = ? AND is_active = 1";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        CharString username_utf8 = username.utf8();
        CharString hash_utf8 = password_hash.utf8();

        sqlite3_bind_text(stmt, 1, username_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 2, hash_utf8.get_data(), -1, SQLITE_STATIC);

        if (sqlite3_step(stmt) == SQLITE_ROW) {
            Dictionary user;
            user["id"] = String((char*)sqlite3_column_text(stmt, 0));
            user["username"] = String((char*)sqlite3_column_text(stmt, 1));
            user["email"] = String((char*)sqlite3_column_text(stmt, 2));
            user["created_at"] = String((char*)sqlite3_column_text(stmt, 3));
            user["last_login"] = String((char*)sqlite3_column_text(stmt, 4));

            sqlite3_finalize(stmt);
            return user;
        }

        sqlite3_finalize(stmt);
    }

    return Dictionary();
}

bool DatabaseManager::update_last_login(const String& user_id) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return false;

    std::time_t now = std::time(nullptr);
    char time_str[64];
    std::strftime(time_str, sizeof(time_str), "%Y-%m-%d %H:%M:%S", std::localtime(&now));
    String last_login = String(time_str);

    const char* sql = "UPDATE users SET last_login = ? WHERE id = ?";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        CharString login_utf8 = last_login.utf8();
        CharString id_utf8 = user_id.utf8();

        sqlite3_bind_text(stmt, 1, login_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 2, id_utf8.get_data(), -1, SQLITE_STATIC);

        bool success = sqlite3_step(stmt) == SQLITE_DONE;
        sqlite3_finalize(stmt);
        return success;
    }

    return false;
}

Dictionary DatabaseManager::get_user_by_id(const String& user_id) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return Dictionary();

    const char* sql = "SELECT id, username, email, created_at, last_login FROM users WHERE id = ? AND is_active = 1";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        CharString id_utf8 = user_id.utf8();
        sqlite3_bind_text(stmt, 1, id_utf8.get_data(), -1, SQLITE_STATIC);

        if (sqlite3_step(stmt) == SQLITE_ROW) {
            Dictionary user;
            user["id"] = String((char*)sqlite3_column_text(stmt, 0));
            user["username"] = String((char*)sqlite3_column_text(stmt, 1));
            user["email"] = String((char*)sqlite3_column_text(stmt, 2));
            user["created_at"] = String((char*)sqlite3_column_text(stmt, 3));
            user["last_login"] = String((char*)sqlite3_column_text(stmt, 4));

            sqlite3_finalize(stmt);
            return user;
        }

        sqlite3_finalize(stmt);
    }

    return Dictionary();
}

Dictionary DatabaseManager::get_last_active_session() {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return Dictionary();

    const char* sql = "SELECT user_id, username, last_login FROM sessions ORDER BY last_login DESC LIMIT 1";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        if (sqlite3_step(stmt) == SQLITE_ROW) {
            Dictionary session;
            session["user_id"] = String((char*)sqlite3_column_text(stmt, 0));
            session["username"] = String((char*)sqlite3_column_text(stmt, 1));
            session["last_login"] = String((char*)sqlite3_column_text(stmt, 2));

            sqlite3_finalize(stmt);
            return session;
        }

        sqlite3_finalize(stmt);
    }

    return Dictionary();
}

bool DatabaseManager::save_user_settings(const String& user_id, const Dictionary& settings) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return false;

    String preferences = JSON::stringify(settings);
    const char* sql = "INSERT OR REPLACE INTO user_settings (user_id, preferences) VALUES (?, ?)";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        CharString id_utf8 = user_id.utf8();
        CharString prefs_utf8 = preferences.utf8();

        sqlite3_bind_text(stmt, 1, id_utf8.get_data(), -1, SQLITE_STATIC);
        sqlite3_bind_text(stmt, 2, prefs_utf8.get_data(), -1, SQLITE_STATIC);

        bool success = sqlite3_step(stmt) == SQLITE_DONE;
        sqlite3_finalize(stmt);
        return success;
    }

    return false;
}

Dictionary DatabaseManager::get_user_settings(const String& user_id) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return Dictionary();

    const char* sql = "SELECT preferences FROM user_settings WHERE user_id = ?";
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, sql, -1, &stmt, nullptr) == SQLITE_OK) {
        CharString id_utf8 = user_id.utf8();
        sqlite3_bind_text(stmt, 1, id_utf8.get_data(), -1, SQLITE_STATIC);

        if (sqlite3_step(stmt) == SQLITE_ROW) {
            String prefs_json = String((char*)sqlite3_column_text(stmt, 0));
            Variant parsed = JSON::parse_string(prefs_json);
            sqlite3_finalize(stmt);
            
            if (parsed.get_type() == Variant::DICTIONARY) {
                return parsed;
            }
        }

        sqlite3_finalize(stmt);
    }

    return Dictionary();
}

bool DatabaseManager::execute_query(const String& query) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return false;

    CharString query_utf8 = query.utf8();
    char* err_msg = nullptr;

    int rc = sqlite3_exec(db, query_utf8.get_data(), nullptr, nullptr, &err_msg);
    if (rc != SQLITE_OK) {
        UtilityFunctions::printerr("DatabaseManager: Error en query: ", err_msg);
        sqlite3_free(err_msg);
        return false;
    }

    return true;
}

Array DatabaseManager::execute_select(const String& query) {
    std::lock_guard<std::mutex> lock(db_mutex);
    if (db == nullptr) return Array();

    Array results;
    CharString query_utf8 = query.utf8();
    sqlite3_stmt* stmt;

    if (sqlite3_prepare_v2(db, query_utf8.get_data(), -1, &stmt, nullptr) == SQLITE_OK) {
        while (sqlite3_step(stmt) == SQLITE_ROW) {
            Dictionary row;
            int column_count = sqlite3_column_count(stmt);
            for (int i = 0; i < column_count; i++) {
                String column_name = String(sqlite3_column_name(stmt, i));
                String column_value = String((char*)sqlite3_column_text(stmt, i));
                row[column_name] = column_value;
            }
            results.append(row);
        }
        sqlite3_finalize(stmt);
    }

    return results;
}

bool DatabaseManager::table_exists(const String& table_name) {
    // Implementación simplificada
    return true;
}

} // namespace WoldVirtual3D

