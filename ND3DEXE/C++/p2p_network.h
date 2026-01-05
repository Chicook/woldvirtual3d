#ifndef P2P_NETWORK_H
#define P2P_NETWORK_H

#include <godot_cpp/classes/node.hpp>
#include <godot_cpp/core/binder_common.hpp>
#include <godot_cpp/variant/string.hpp>
#include <godot_cpp/variant/dictionary.hpp>
#include <godot_cpp/variant/array.hpp>
#include <thread>
#include <mutex>
#include <unordered_map>
#include <string>

using namespace godot;

namespace WoldVirtual3D {

/**
 * Sistema de red P2P distribuida en C++
 * Responsabilidad: Gestión de bajo nivel de conexiones P2P
 */
class P2PNetworkCpp : public Node {
    GDCLASS(P2PNetworkCpp, Node);

private:
    struct NodeInfo {
        String node_id;
        String user_id;
        String ip_address;
        int port;
        bool is_connected;
        std::chrono::steady_clock::time_point last_seen;
        
        NodeInfo() : port(0), is_connected(false), last_seen(std::chrono::steady_clock::now()) {}
    };

    int udp_port;
    int tcp_port;
    bool is_running;
    String local_node_id;
    String local_user_id;
    
    std::thread network_thread;
    std::mutex nodes_mutex;
    std::unordered_map<std::string, NodeInfo> connected_nodes;
    
    void* udp_socket;
    void* tcp_listener;

protected:
    static void _bind_methods();

public:
    P2PNetworkCpp();
    ~P2PNetworkCpp();

    void _ready() override;
    void _exit_tree() override;

    // Inicialización
    void initialize(const String& user_id);
    void shutdown();

    // Gestión de nodos
    void connect_to_node(const String& node_id, const String& ip, int port);
    void disconnect_node(const String& node_id);
    Array get_connected_nodes() const;
    bool is_node_connected(const String& node_id) const;

    // Envío de mensajes
    void send_message(const String& node_id, const String& message_type, const Dictionary& data);
    void broadcast_message(const String& message_type, const Dictionary& data);
    void ping_node(const String& node_id);

    // Callbacks
    void set_on_node_connected_callback(const Callable& callback);
    void set_on_node_disconnected_callback(const Callable& callback);
    void set_on_message_received_callback(const Callable& callback);

    // Getters
    String get_local_node_id() const { return local_node_id; }
    bool get_is_running() const { return is_running; }
    int get_connected_nodes_count() const;

private:
    void network_loop();
    void process_udp_messages();
    void process_tcp_connections();
    void handle_message(const String& message, const String& sender_ip, int sender_port);
    void maintain_connections();
    void generate_node_id();
    void send_udp_message(const String& message, const String& ip, int port);
    
    Callable on_node_connected_callback;
    Callable on_node_disconnected_callback;
    Callable on_message_received_callback;
};

} // namespace WoldVirtual3D

#endif // P2P_NETWORK_H

