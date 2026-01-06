// NOTE: Shader automatically converted from Godot Engine 4.5.1.stable's StandardMaterial3D.

shader_type spatial;
render_mode blend_mix, depth_draw_always, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color, filter_linear_mipmap, repeat_enable;

uniform float roughness : hint_range(0.0, 1.0);
uniform sampler2D texture_metallic : hint_default_white, filter_linear_mipmap, repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r, filter_linear_mipmap, repeat_enable;

uniform float specular : hint_range(0.0, 1.0, 0.01);
uniform float metallic : hint_range(0.0, 1.0, 0.01);

uniform sampler2D texture_refraction : filter_linear_mipmap, repeat_enable;
uniform float refraction : hint_range(-1.0, 1.0, 0.001);
uniform vec4 refraction_texture_channel;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;
uniform sampler2D depth_texture : hint_depth_texture, repeat_disable, filter_nearest;

uniform sampler2D texture_normal : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform float normal_scale : hint_range(-16.0, 16.0);

uniform vec3 uv1_scale;
uniform vec3 uv1_offset;

uniform vec2 island_center = vec2(0.0, 0.0);
uniform float island_radius : hint_range(0.1, 50.0, 0.1) = 12.0;
uniform float island_falloff : hint_range(0.1, 50.0, 0.1) = 5.0;

void vertex() {
	// Sistema de olas oceánicas avanzado
	vec2 animated_uv = UV * uv1_scale.xy + uv1_offset.xy;
	float time = TIME;

	// Olas grandes del océano (olas principales)
	float large_wave1 = sin(animated_uv.x * 3.0 + time * 0.3) * 0.02;
	float large_wave2 = sin(animated_uv.y * 2.5 + time * 0.25) * 0.015;
	float large_wave3 = cos((animated_uv.x + animated_uv.y) * 4.0 + time * 0.4) * 0.018;

	// Olas medianas (olas secundarias)
	float medium_wave1 = sin(animated_uv.x * 8.0 + time * 0.8) * 0.008;
	float medium_wave2 = cos(animated_uv.y * 6.0 + time * 0.6) * 0.006;

	// Olas pequeñas (capilares/ripples)
	float small_wave1 = sin(animated_uv.x * 15.0 + time * 1.2) * 0.003;
	float small_wave2 = cos(animated_uv.y * 12.0 + time * 1.0) * 0.002;

	// Combinar todas las olas con diferentes direcciones
	animated_uv.x += large_wave1 + large_wave2 + medium_wave1 + small_wave1;
	animated_uv.y += large_wave3 + medium_wave2 + small_wave2;

	// Movimiento diagonal para simular corriente oceánica
	float current_x = sin(time * 0.1 + animated_uv.y * 2.0) * 0.005;
	float current_y = cos(time * 0.15 + animated_uv.x * 1.8) * 0.004;

	animated_uv.x += current_x;
	animated_uv.y += current_y;

	UV = animated_uv;
}

void fragment() {
	vec2 base_uv = UV;
	float time = TIME;

	// Detección de geometría cercana: descartar si hay tierra delante
	float scene_depth = textureLod(depth_texture, SCREEN_UV, 0.0).r;
	if (scene_depth < DEPTH - 0.01) {
		ALPHA = 0.0;
		discard;
	}

	// Animación simplificada pero fluida
	base_uv.x += sin(base_uv.y * 15.0 + time * 0.8) * 0.01;
	base_uv.y += cos(base_uv.x * 12.0 + time * 0.6) * 0.008;

	vec4 albedo_tex = texture(texture_albedo, base_uv);

	// Detección de costa mediante distancia a la isla
	vec2 local_pos = VERTEX.xz;
	float island_distance = distance(local_pos, island_center);
	float coast_factor = smoothstep(island_radius, island_radius + island_falloff, island_distance);
	coast_factor = clamp(coast_factor, 0.0, 1.0);

	// Calcular altura de olas para efectos de espuma
	float wave_height = sin(base_uv.x * 3.0 + time * 0.3) * 0.5 + sin(base_uv.y * 2.0 + time * 0.2) * 0.3;

	// Efecto de espuma en las crestas
	float foam_amount = smoothstep(0.2, 0.6, wave_height);
	vec3 foam_color = vec3(0.95, 0.98, 1.0);
	vec3 shore_color = vec3(0.3, 0.6, 0.9);
	vec3 ocean_color = vec3(0.1, 0.3, 0.8);

	vec3 water_color = mix(shore_color, ocean_color, clamp(wave_height * coast_factor, 0.0, 1.0));
	ALBEDO = mix(albedo.rgb * albedo_tex.rgb, foam_color, foam_amount * 0.4);
	ALBEDO = mix(ALBEDO, water_color, 0.5);

	ALPHA = mix(0.0, 1.0, coast_factor);

	float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	SPECULAR = specular;

	vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;

	// Normal Map animado
	vec2 normal_uv = base_uv + vec2(time * 0.1, time * 0.08);
	NORMAL_MAP = texture(texture_normal, normal_uv).rgb;
	NORMAL_MAP_DEPTH = normal_scale;

	// Refracción estándar
	vec3 unpacked_normal = NORMAL_MAP;
	unpacked_normal.xy = unpacked_normal.xy * 2.0 - 1.0;
	unpacked_normal.z = sqrt(max(0.0, 1.0 - dot(unpacked_normal.xy, unpacked_normal.xy)));
	vec3 ref_normal = normalize(mix(
			NORMAL,
			TANGENT * unpacked_normal.x + BINORMAL * unpacked_normal.y + NORMAL * unpacked_normal.z,
			NORMAL_MAP_DEPTH));
	vec2 ref_ofs = SCREEN_UV - ref_normal.xy * dot(texture(texture_refraction, base_uv), refraction_texture_channel) * refraction;

	float ref_amount = (1.0 - albedo.a * albedo_tex.a);
	float refraction_depth_tex = textureLod(depth_texture, ref_ofs, 0.0).r;
	vec4 refraction_view_pos = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, refraction_depth_tex, 1.0);
	refraction_view_pos.xyz /= refraction_view_pos.w;

	EMISSION += textureLod(screen_texture, mix(SCREEN_UV, ref_ofs, smoothstep(0.0, 1.0, VERTEX.z - refraction_view_pos.z)), ROUGHNESS * 8.0).rgb * ref_amount * EXPOSURE;
	ALBEDO *= 1.0 - ref_amount;

	ALPHA = max(ALPHA, 0.0);
}
