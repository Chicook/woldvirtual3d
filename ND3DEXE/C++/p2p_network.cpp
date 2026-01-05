#include "p2p_network.h"
#include <godot_cpp/core/class_db.hpp>
#include <godot_cpp/variant/utility_functions.hpp>
#include <godot_cpp/variant/json.hpp>
#include <random>
#include <sstream>
#include <iomanip>
#include <chrono>
#include <cstring>
#include <vector>
#include <errno.h>

#ifdef _WIN32
    #include <winsock2.h>
    #include <ws2tcpip.h>
    #include <io.h>
    #pragma comment(lib, "ws2_32.lib")
    #define close closesocket
    #define errno WSAGetLastError()
#else
    #include <sys/socket.h>
    #include <netinet/in.h>
    #include <arpa/inet.h>
    #include <unistd.h>
    #include <errno.h>
    #include <fcntl.h>
#endif

using namespace godot;

namespace WoldVirtual3D {

void P2PNetworkCpp::_bind_methods() {
    ClassDB::bind_method(D_METHOD("initialize", "user_id"), &P2PNetworkCpp::initialize);
    ClassDB::bind_method(D_METHOD("shutdown"), &P2PNetworkCpp::shutdown);
    ClassDB::bind_method(D_METHOD("connect_to_node", "node_id", "ip", "port"), &P2PNetworkCpp::connect_to_node);
    ClassDB::bind_method(D_METHOD("disconnect_node", "node_id"), &P2PNetworkCpp::disconnect_node);
    ClassDB::bind_method(D_METHOD("get_connected_nodes"), &P2PNetworkCpp::get_connected_nodes);
    ClassDB::bind_method(D_METHOD("is_node_connected", "node_id"), &P2PNetworkCpp::is_node_connected);
    ClassDB::bind_method(D_METHOD("send_message", "node_id", "message_type", "data"), &P2PNetworkCpp::send_message);
    ClassDB::bind_method(D_METHOD("broadcast_message", "message_type", "data"), &P2PNetworkCpp::broadcast_message);
    ClassDB::bind_method(D_METHOD("ping_node", "node_id"), &P2PNetworkCpp::ping_node);
    ClassDB::bind_method(D_METHOD("set_on_node_connected_callback", "callback"), &P2PNetworkCpp::set_on_node_connected_callback);
    ClassDB::bind_method(D_METHOD("set_on_node_disconnected_callback", "callback"), &P2PNetworkCpp::set_on_node_disconnected_callback);
    ClassDB::bind_method(D_METHOD("set_on_message_received_callback", "callback"), &P2PNetworkCpp::set_on_message_received_callback);
    ClassDB::bind_method(D_METHOD("get_local_node_id"), &P2PNetworkCpp::get_local_node_id);
    ClassDB::bind_method(D_METHOD("get_is_running"), &P2PNetworkCpp::get_is_running);
    ClassDB::bind_method(D_METHOD("get_connected_nodes_count"), &P2PNetworkCpp::get_connected_nodes_count);

    ADD_PROPERTY(PropertyInfo(Variant::STRING, "local_node_id", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_READ_ONLY), "", "get_local_node_id");
    ADD_PROPERTY(PropertyInfo(Variant::BOOL, "is_running", PROPERTY_HINT_NONE, "", PROPERTY_USAGE_READ_ONLY), "", "get_is_running");
}

P2PNetworkCpp::P2PNetworkCpp() {
    udp_port = 7777;
    tcp_port = 7778;
    is_running = false;
    udp_socket = nullptr;
    tcp_listener = nullptr;
    generate_node_id();
}

P2PNetworkCpp::~P2PNetworkCpp() {
    shutdown();
}

void P2PNetworkCpp::_ready() {
    UtilityFunctions::print("P2PNetworkCpp: Inicializado con NodeID: ", local_node_id);
}

void P2PNetworkCpp::_exit_tree() {
    shutdown();
}

void P2PNetworkCpp::generate_node_id() {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<> dis(0, 15);
    
    std::stringstream ss;
    ss << std::hex;
    for (int i = 0; i < 32; ++i) {
        ss << dis(gen);
    }
    
    local_node_id = String(ss.str().c_str());
}

void P2PNetworkCpp::initialize(const String& user_id) {
    if (is_running) {
        UtilityFunctions::print("P2PNetworkCpp: Red ya está inicializada");
        return;
    }

    local_user_id = user_id;
    UtilityFunctions::print("P2PNetworkCpp: Inicializando red P2P para usuario: ", user_id);

#ifdef _WIN32
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        UtilityFunctions::printerr("P2PNetworkCpp: Error al inicializar Winsock");
        return;
    }
#endif

    // Crear socket UDP
    int sock = socket(AF_INET, SOCK_DGRAM, 0);
    if (sock < 0) {
#ifdef _WIN32
        int error = WSAGetLastError();
        UtilityFunctions::printerr("P2PNetworkCpp: Error al crear socket UDP: ", error);
#else
        UtilityFunctions::printerr("P2PNetworkCpp: Error al crear socket UDP: ", strerror(errno));
#endif
        return;
    }

    struct sockaddr_in addr;
    memset(&addr, 0, sizeof(addr));
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = INADDR_ANY;
    addr.sin_port = htons(udp_port);

    // Configurar opciones del socket
    int opt = 1;
#ifdef _WIN32
    if (setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, (char*)&opt, sizeof(opt)) < 0) {
        UtilityFunctions::printerr("P2PNetworkCpp: Error al configurar SO_REUSEADDR");
    }
    
    // Configurar buffer de recepción
    int recv_buf_size = 65536;
    setsockopt(sock, SOL_SOCKET, SO_RCVBUF, (char*)&recv_buf_size, sizeof(recv_buf_size));
#else
    if (setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt)) < 0) {
        UtilityFunctions::printerr("P2PNetworkCpp: Error al configurar SO_REUSEADDR: ", strerror(errno));
    }
    
    // Configurar buffer de recepción
    int recv_buf_size = 65536;
    setsockopt(sock, SOL_SOCKET, SO_RCVBUF, &recv_buf_size, sizeof(recv_buf_size));
#endif

    if (bind(sock, (struct sockaddr*)&addr, sizeof(addr)) < 0) {
#ifdef _WIN32
        int error = WSAGetLastError();
        UtilityFunctions::printerr("P2PNetworkCpp: Error al hacer bind del socket UDP: ", error);
        closesocket(sock);
#else
        UtilityFunctions::printerr("P2PNetworkCpp: Error al hacer bind del socket UDP: ", strerror(errno));
        close(sock);
#endif
        return;
    }

    udp_socket = (void*)(intptr_t)sock;
    
    // Inicializar servidor TCP (opcional, para conexiones estables)
    int tcp_sock = socket(AF_INET, SOCK_STREAM, 0);
    if (tcp_sock >= 0) {
        struct sockaddr_in tcp_addr;
        memset(&tcp_addr, 0, sizeof(tcp_addr));
        tcp_addr.sin_family = AF_INET;
        tcp_addr.sin_addr.s_addr = INADDR_ANY;
        tcp_addr.sin_port = htons(tcp_port);
        
#ifdef _WIN32
        setsockopt(tcp_sock, SOL_SOCKET, SO_REUSEADDR, (char*)&opt, sizeof(opt));
#else
        setsockopt(tcp_sock, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));
#endif
        
        if (bind(tcp_sock, (struct sockaddr*)&tcp_addr, sizeof(tcp_addr)) == 0) {
            if (listen(tcp_sock, 10) == 0) {
                tcp_listener = (void*)(intptr_t)tcp_sock;
                UtilityFunctions::print("P2PNetworkCpp: Servidor TCP iniciado en puerto: ", tcp_port);
            } else {
#ifdef _WIN32
                closesocket(tcp_sock);
#else
                close(tcp_sock);
#endif
            }
        } else {
#ifdef _WIN32
            closesocket(tcp_sock);
#else
            close(tcp_sock);
#endif
        }
    }
    
    is_running = true;

    network_thread = std::thread(&P2PNetworkCpp::network_loop, this);

    UtilityFunctions::print("P2PNetworkCpp: Red P2P inicializada correctamente en puerto UDP: ", udp_port);
}

void P2PNetworkCpp::shutdown() {
    if (!is_running) {
        return;
    }

    UtilityFunctions::print("P2PNetworkCpp: Cerrando red P2P...");

    is_running = false;

    if (network_thread.joinable()) {
        network_thread.join();
    }

    if (udp_socket) {
        int sock = (int)(intptr_t)udp_socket;
#ifdef _WIN32
        closesocket(sock);
#else
        close(sock);
#endif
        udp_socket = nullptr;
    }
    
    if (tcp_listener) {
        int sock = (int)(intptr_t)tcp_listener;
#ifdef _WIN32
        closesocket(sock);
        WSACleanup();
#else
        close(sock);
#endif
        tcp_listener = nullptr;
    }
    
#ifdef _WIN32
    if (!udp_socket && !tcp_listener) {
        WSACleanup();
    }
#endif

    {
        std::lock_guard<std::mutex> lock(nodes_mutex);
        connected_nodes.clear();
    }

    UtilityFunctions::print("P2PNetworkCpp: Red P2P cerrada");
}

void P2PNetworkCpp::network_loop() {
    while (is_running) {
        process_udp_messages();
        if (tcp_listener) {
            process_tcp_connections();
        }
        maintain_connections();
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
}

void P2PNetworkCpp::process_udp_messages() {
    if (!udp_socket) return;

    int sock = (int)(intptr_t)udp_socket;
    char buffer[4096];
    struct sockaddr_in from_addr;
    socklen_t from_len = sizeof(from_addr);

    // Configurar socket como no bloqueante para mejor rendimiento
#ifdef _WIN32
    unsigned long mode = 1;
    ioctlsocket(sock, FIONBIO, &mode);
    int bytes_received = recvfrom(sock, buffer, sizeof(buffer) - 1, 0, (struct sockaddr*)&from_addr, &from_len);
    if (bytes_received == SOCKET_ERROR) {
        int error = WSAGetLastError();
        if (error != WSAEWOULDBLOCK && error != WSAECONNRESET) {
            // Error real, no solo "no hay datos"
        }
        return;
    }
#else
    // Linux/Unix: usar recvfrom con flags MSG_DONTWAIT
    int bytes_received = recvfrom(sock, buffer, sizeof(buffer) - 1, MSG_DONTWAIT, (struct sockaddr*)&from_addr, &from_len);
    if (bytes_received < 0) {
        if (errno == EAGAIN || errno == EWOULDBLOCK) {
            // No hay datos disponibles, es normal
            return;
        }
        // Error real
        return;
    }
#endif

    if (bytes_received > 0) {
        buffer[bytes_received] = '\0';
        String message = String(buffer);
        
        char ip_str[INET_ADDRSTRLEN];
#ifdef _WIN32
        InetNtopA(AF_INET, &from_addr.sin_addr, ip_str, INET_ADDRSTRLEN);
#else
        inet_ntop(AF_INET, &from_addr.sin_addr, ip_str, INET_ADDRSTRLEN);
#endif
        String sender_ip = String(ip_str);
        int sender_port = ntohs(from_addr.sin_port);
        
        handle_message(message, sender_ip, sender_port);
    }
}

void P2PNetworkCpp::handle_message(const String& message, const String& sender_ip, int sender_port) {
    // Parsear mensaje JSON básico
    Variant json_result = JSON::parse_string(message);
    if (json_result.get_type() == Variant::DICTIONARY) {
        Dictionary msg = json_result;
        String msg_type = msg.get("type", "");
        String from_node_id = msg.get("from_node_id", "");

        if (msg_type == "discovery") {
            // Responder con información del nodo
            Dictionary response;
            response["type"] = "discovery_response";
            response["from_node_id"] = local_node_id;
            response["node_id"] = local_node_id;
            response["user_id"] = local_user_id;
            response["port"] = tcp_port;

            String json_response = JSON::stringify(response);
            send_udp_message(json_response, sender_ip, sender_port);
        }
        else if (msg_type == "connect") {
            std::lock_guard<std::mutex> lock(nodes_mutex);
            
            std::string node_key = from_node_id.utf8().get_data();
            auto it = connected_nodes.find(node_key);
            
            if (it == connected_nodes.end()) {
                // Nuevo nodo
                NodeInfo node;
                node.node_id = from_node_id;
                node.user_id = msg.get("user_id", "");
                node.ip_address = sender_ip;
                node.port = sender_port;
                node.is_connected = true;
                node.last_seen = std::chrono::steady_clock::now();

                connected_nodes[node_key] = node;

                if (on_node_connected_callback.is_valid()) {
                    Dictionary node_dict;
                    node_dict["node_id"] = node.node_id;
                    node_dict["user_id"] = node.user_id;
                    node_dict["ip_address"] = node.ip_address;
                    node_dict["port"] = node.port;
                    on_node_connected_callback.call(node_dict);
                }

                UtilityFunctions::print("P2PNetworkCpp: Nodo conectado: ", from_node_id);
            } else {
                // Actualizar nodo existente
                it->second.last_seen = std::chrono::steady_clock::now();
                it->second.is_connected = true;
            }
        }
        else if (msg_type == "disconnect") {
            disconnect_node(from_node_id);
        }
        else if (msg_type == "ping") {
            // Responder a ping
            Dictionary pong;
            pong["type"] = "pong";
            pong["from_node_id"] = local_node_id;
            String pong_json = JSON::stringify(pong);
            send_udp_message(pong_json, sender_ip, sender_port);
        }
        else if (msg_type == "data") {
            if (on_message_received_callback.is_valid()) {
                on_message_received_callback.call(msg);
            }
        }
    }
}

void P2PNetworkCpp::send_udp_message(const String& message, const String& ip, int port) {
    if (!udp_socket || !is_running) return;

    int sock = (int)(intptr_t)udp_socket;
    struct sockaddr_in addr;
    memset(&addr, 0, sizeof(addr));
    addr.sin_family = AF_INET;
    addr.sin_port = htons(port);
    
    CharString ip_utf8 = ip.utf8();
    int result;
#ifdef _WIN32
    result = InetPtonA(AF_INET, ip_utf8.get_data(), &addr.sin_addr);
    if (result != 1) {
        UtilityFunctions::printerr("P2PNetworkCpp: Error al convertir IP: ", ip);
        return;
    }
#else
    result = inet_pton(AF_INET, ip_utf8.get_data(), &addr.sin_addr);
    if (result != 1) {
        UtilityFunctions::printerr("P2PNetworkCpp: Error al convertir IP: ", ip);
        return;
    }
#endif

    CharString msg_utf8 = message.utf8();
    int bytes_sent = sendto(sock, msg_utf8.get_data(), msg_utf8.length(), 0, (struct sockaddr*)&addr, sizeof(addr));
    
    if (bytes_sent < 0) {
#ifdef _WIN32
        int error = WSAGetLastError();
        UtilityFunctions::printerr("P2PNetworkCpp: Error al enviar mensaje UDP: ", error);
#else
        UtilityFunctions::printerr("P2PNetworkCpp: Error al enviar mensaje UDP: ", strerror(errno));
#endif
    }
}

void P2PNetworkCpp::maintain_connections() {
    std::lock_guard<std::mutex> lock(nodes_mutex);
    
    auto now = std::chrono::steady_clock::now();
    const auto timeout = std::chrono::seconds(30); // 30 segundos de timeout
    std::vector<std::string> nodes_to_remove;

    for (auto& pair : connected_nodes) {
        auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(now - pair.second.last_seen);
        
        if (elapsed > timeout) {
            nodes_to_remove.push_back(pair.first);
            UtilityFunctions::print("P2PNetworkCpp: Nodo timeout: ", String(pair.first.c_str()));
        }
    }

    for (const auto& node_key : nodes_to_remove) {
        auto it = connected_nodes.find(node_key);
        if (it != connected_nodes.end()) {
            String node_id = it->second.node_id;
            connected_nodes.erase(it);
            
            if (on_node_disconnected_callback.is_valid()) {
                on_node_disconnected_callback.call(node_id);
            }
        }
    }
}

void P2PNetworkCpp::connect_to_node(const String& node_id, const String& ip, int port) {
    Dictionary connect_msg;
    connect_msg["type"] = "connect";
    connect_msg["from_node_id"] = local_node_id;
    connect_msg["user_id"] = local_user_id;

    String json_msg = JSON::stringify(connect_msg);
    send_udp_message(json_msg, ip, port);
}

void P2PNetworkCpp::disconnect_node(const String& node_id) {
    std::lock_guard<std::mutex> lock(nodes_mutex);
    auto it = connected_nodes.find(node_id.utf8().get_data());
    if (it != connected_nodes.end()) {
        connected_nodes.erase(it);
        
        if (on_node_disconnected_callback.is_valid()) {
            on_node_disconnected_callback.call(node_id);
        }

        UtilityFunctions::print("P2PNetworkCpp: Nodo desconectado: ", node_id);
    }
}

Array P2PNetworkCpp::get_connected_nodes() const {
    Array result;
    std::lock_guard<std::mutex> lock(const_cast<std::mutex&>(nodes_mutex));
    
    for (const auto& pair : connected_nodes) {
        Dictionary node_dict;
        node_dict["node_id"] = pair.second.node_id;
        node_dict["user_id"] = pair.second.user_id;
        node_dict["ip_address"] = pair.second.ip_address;
        node_dict["port"] = pair.second.port;
        node_dict["is_connected"] = pair.second.is_connected;
        result.append(node_dict);
    }
    
    return result;
}

bool P2PNetworkCpp::is_node_connected(const String& node_id) const {
    std::lock_guard<std::mutex> lock(const_cast<std::mutex&>(nodes_mutex));
    return connected_nodes.find(node_id.utf8().get_data()) != connected_nodes.end();
}

void P2PNetworkCpp::send_message(const String& node_id, const String& message_type, const Dictionary& data) {
    std::lock_guard<std::mutex> lock(nodes_mutex);
    auto it = connected_nodes.find(node_id.utf8().get_data());
    if (it == connected_nodes.end()) {
        UtilityFunctions::printerr("P2PNetworkCpp: Nodo no encontrado: ", node_id);
        return;
    }

    Dictionary msg;
    msg["type"] = message_type;
    msg["from_node_id"] = local_node_id;
    msg["to_node_id"] = node_id;
    msg["data"] = data;

    String json_msg = JSON::stringify(msg);
    send_udp_message(json_msg, it->second.ip_address, it->second.port);
}

void P2PNetworkCpp::broadcast_message(const String& message_type, const Dictionary& data) {
    std::lock_guard<std::mutex> lock(nodes_mutex);
    
    for (const auto& pair : connected_nodes) {
        Dictionary msg;
        msg["type"] = message_type;
        msg["from_node_id"] = local_node_id;
        msg["to_node_id"] = pair.second.node_id;
        msg["data"] = data;

        String json_msg = JSON::stringify(msg);
        send_udp_message(json_msg, pair.second.ip_address, pair.second.port);
    }
}

void P2PNetworkCpp::ping_node(const String& node_id) {
    std::lock_guard<std::mutex> lock(nodes_mutex);
    auto it = connected_nodes.find(node_id.utf8().get_data());
    if (it == connected_nodes.end()) {
        UtilityFunctions::printerr("P2PNetworkCpp: Nodo no encontrado para ping: ", node_id);
        return;
    }

    Dictionary ping_msg;
    ping_msg["type"] = "ping";
    ping_msg["from_node_id"] = local_node_id;
    ping_msg["to_node_id"] = node_id;

    String json_msg = JSON::stringify(ping_msg);
    send_udp_message(json_msg, it->second.ip_address, it->second.port);
}

void P2PNetworkCpp::set_on_node_connected_callback(const Callable& callback) {
    on_node_connected_callback = callback;
}

void P2PNetworkCpp::set_on_node_disconnected_callback(const Callable& callback) {
    on_node_disconnected_callback = callback;
}

void P2PNetworkCpp::set_on_message_received_callback(const Callable& callback) {
    on_message_received_callback = callback;
}

int P2PNetworkCpp::get_connected_nodes_count() const {
    std::lock_guard<std::mutex> lock(const_cast<std::mutex&>(nodes_mutex));
    return connected_nodes.size();
}

void P2PNetworkCpp::process_tcp_connections() {
    if (!tcp_listener) return;

    // Implementación básica de TCP para conexiones estables
    // Esto permite transferencias de datos más grandes y confiables
    
    int listener_sock = (int)(intptr_t)tcp_listener;
    
#ifdef _WIN32
    unsigned long mode = 1;
    ioctlsocket(listener_sock, FIONBIO, &mode);
    
    struct sockaddr_in client_addr;
    int client_len = sizeof(client_addr);
    int client_sock = accept(listener_sock, (struct sockaddr*)&client_addr, &client_len);
    
    if (client_sock != INVALID_SOCKET) {
        // Manejar conexión TCP en hilo separado (simplificado)
        closesocket(client_sock);
    }
#else
    struct sockaddr_in client_addr;
    socklen_t client_len = sizeof(client_addr);
    int client_sock = accept(listener_sock, (struct sockaddr*)&client_addr, &client_len);
    
    if (client_sock >= 0) {
        // Manejar conexión TCP en hilo separado (simplificado)
        close(client_sock);
    }
#endif
}

} // namespace WoldVirtual3D

