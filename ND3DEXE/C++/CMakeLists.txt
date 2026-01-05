cmake_minimum_required(VERSION 3.16)
project(WoldVirtual3D_CPP)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Buscar Godot
find_path(GODOT_CPP_INCLUDE_DIR godot_cpp/classes/node.hpp
    PATHS
    ${CMAKE_SOURCE_DIR}/godot-cpp/include
    ${CMAKE_SOURCE_DIR}/../godot-cpp/include
    /usr/include
    /usr/local/include
)

# Buscar SQLite3
find_library(SQLITE3_LIBRARY
    NAMES sqlite3
    PATHS
    /usr/lib
    /usr/local/lib
    ${CMAKE_SOURCE_DIR}/lib
)

# Buscar OpenSSL
find_package(OpenSSL REQUIRED)

# Archivos fuente
set(SOURCES
    register_types.cpp
    p2p_network.cpp
    database_manager.cpp
)

set(HEADERS
    register_types.h
    p2p_network.h
    database_manager.h
)

# Crear la biblioteca
add_library(${PROJECT_NAME} SHARED ${SOURCES} ${HEADERS})

# Incluir directorios
target_include_directories(${PROJECT_NAME} PRIVATE
    ${GODOT_CPP_INCLUDE_DIR}
    ${OPENSSL_INCLUDE_DIR}
)

# Enlazar bibliotecas
target_link_libraries(${PROJECT_NAME} PRIVATE
    ${SQLITE3_LIBRARY}
    ${OPENSSL_LIBRARIES}
)

# Configuración específica de plataforma
if(WIN32)
    target_link_libraries(${PROJECT_NAME} PRIVATE ws2_32)
endif()

# Configuración de compilación
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    target_compile_definitions(${PROJECT_NAME} PRIVATE DEBUG_ENABLED)
endif()

